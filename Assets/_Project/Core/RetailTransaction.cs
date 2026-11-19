using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>A completed purchase from an unlimited authored business catalogue.</summary>
    public sealed class RetailTransaction
    {
        internal RetailTransaction(Guid transactionId, string businessId, EconomicActorId customer,
            EconomicActorId owner, Guid itemDefinitionId, int quantity, int totalAmount, int day, int minuteOfDay)
        {
            TransactionId = transactionId;
            BusinessId = businessId;
            Customer = customer;
            Owner = owner;
            ItemDefinitionId = itemDefinitionId;
            Quantity = quantity;
            TotalAmount = totalAmount;
            Day = day;
            MinuteOfDay = minuteOfDay;
        }

        public Guid TransactionId { get; }
        public string BusinessId { get; }
        public EconomicActorId Customer { get; }
        public EconomicActorId Owner { get; }
        public Guid ItemDefinitionId { get; }
        public int Quantity { get; }
        public int TotalAmount { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class RetailTransactionLedger
    {
        private readonly List<RetailTransaction> _transactions = new List<RetailTransaction>();
        private readonly HashSet<Guid> _ids = new HashSet<Guid>();
        private readonly IReadOnlyList<RetailTransaction> _readOnly;

        public RetailTransactionLedger() { _readOnly = _transactions.AsReadOnly(); }
        public IReadOnlyList<RetailTransaction> Transactions => _readOnly;
        public int Count => _transactions.Count;

        public bool TryRecord(RetailTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                string.IsNullOrWhiteSpace(transaction.BusinessId) ||
                !transaction.Customer.IsValid || !transaction.Owner.IsValid ||
                transaction.Customer == transaction.Owner || transaction.ItemDefinitionId == Guid.Empty ||
                transaction.Quantity < 1 || transaction.TotalAmount < 1 ||
                transaction.Day < 1 || transaction.MinuteOfDay < 0 || transaction.MinuteOfDay >= 1440 ||
                !_ids.Add(transaction.TransactionId))
                return false;

            _transactions.Add(transaction);
            return true;
        }

        public bool Contains(Guid transactionId) => _ids.Contains(transactionId);

        public void Clear()
        {
            _transactions.Clear();
            _ids.Clear();
        }
    }
}
