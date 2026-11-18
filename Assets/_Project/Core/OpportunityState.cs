using System;

namespace Signal.Core
{
    public enum OpportunityPhase
    {
        Dormant = 0,
        Available = 1,
        Accepted = 2,
        Resolved = 3
    }

    public enum OpportunityResolution
    {
        None = 0,
        Delivered = 1
    }

    /// <summary>
    /// Small runtime lifecycle for one opportunity. Declining does not fail it;
    /// returning a parcel releases the commitment and makes the offer available again.
    /// </summary>
    public sealed class OpportunityState
    {
        public string OpportunityId { get; }
        public OpportunityPhase Phase { get; private set; }
        public OpportunityResolution Resolution { get; private set; }
        public Guid ParcelInstanceId { get; private set; }
        public int AcceptanceCount { get; private set; }
        public int DeclineCount { get; private set; }
        public int LastAcceptedDay { get; private set; }
        public int LastAcceptedMinuteOfDay { get; private set; }
        public int LastDeclinedDay { get; private set; }
        public int LastDeclinedMinuteOfDay { get; private set; }
        public int ResolvedDay { get; private set; }
        public int ResolvedMinuteOfDay { get; private set; }

        public OpportunityState(string opportunityId)
        {
            if (string.IsNullOrWhiteSpace(opportunityId))
                throw new ArgumentException("An opportunity id is required.", nameof(opportunityId));
            OpportunityId = opportunityId;
            Phase = OpportunityPhase.Dormant;
        }

        public bool MakeAvailable()
        {
            if (Phase != OpportunityPhase.Dormant)
                return false;
            Phase = OpportunityPhase.Available;
            return true;
        }

        public bool RecordDecline(int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Available)
                return false;
            DeclineCount++;
            LastDeclinedDay = day;
            LastDeclinedMinuteOfDay = minuteOfDay;
            return true;
        }

        public bool TryAccept(Guid parcelInstanceId, int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Available || parcelInstanceId == Guid.Empty)
                return false;

            ParcelInstanceId = parcelInstanceId;
            AcceptanceCount++;
            LastAcceptedDay = day;
            LastAcceptedMinuteOfDay = minuteOfDay;
            Phase = OpportunityPhase.Accepted;
            return true;
        }

        public bool TryReturnParcel(int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Accepted)
                return false;

            Phase = OpportunityPhase.Available;
            return true;
        }

        public bool TryResolveDelivered(int day, int minuteOfDay)
        {
            ValidateTime(day, minuteOfDay);
            if (Phase != OpportunityPhase.Accepted)
                return false;

            Phase = OpportunityPhase.Resolved;
            Resolution = OpportunityResolution.Delivered;
            ResolvedDay = day;
            ResolvedMinuteOfDay = minuteOfDay;
            return true;
        }

        public bool TryExpire()
        {
            if (Phase != OpportunityPhase.Available)
                return false;
            Phase = OpportunityPhase.Dormant;
            return true;
        }

        private static void ValidateTime(int day, int minuteOfDay)
        {
            if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(minuteOfDay));
        }
    }
}
