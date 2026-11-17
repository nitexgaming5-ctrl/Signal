using System;

namespace Signal.Core
{
    public enum RentOutcome
    {
        NotDue,
        Paid,
        Missed
    }

    /// <summary>Rent due every few days. Missing a payment is counted, never forgiven silently.</summary>
    public sealed class RentLedger
    {
        public RentLedger(int amount, int intervalDays)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Rent cannot be negative.");
            if (intervalDays < 1)
                throw new ArgumentOutOfRangeException(nameof(intervalDays), "Interval must be at least 1 day.");

            Amount = amount;
            IntervalDays = intervalDays;
        }

        public int Amount { get; }

        public int IntervalDays { get; }

        public int MissedPayments { get; private set; }

        public bool IsDue(int day)
        {
            return day >= 1 && day % IntervalDays == 0;
        }

        public RentOutcome Settle(int day, PlayerStats stats)
        {
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));

            if (!IsDue(day))
                return RentOutcome.NotDue;

            if (Amount == 0 || stats.TrySpend(Amount))
                return RentOutcome.Paid;

            MissedPayments++;
            return RentOutcome.Missed;
        }

        public void Restore(int missedPayments)
        {
            MissedPayments = Math.Max(0, missedPayments);
        }
    }
}
