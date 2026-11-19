using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>A completed sale of a specific physical stack quantity to one authored business.</summary>
    public sealed class ShopSaleTransaction
    {
        internal ShopSaleTransaction(Guid transactionId, string businessId, EconomicActorId seller,
            Guid itemInstanceId, Guid itemDefinitionId, int quantity, int unitPrice, int totalAmount,
            ItemInstanceState itemState, int day, int minuteOfDay)
        {
            TransactionId = transactionId;
            BusinessId = businessId;
            Seller = seller;
            ItemInstanceId = itemInstanceId;
            ItemDefinitionId = itemDefinitionId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalAmount = totalAmount;
            ItemState = itemState;
            Day = day;
            MinuteOfDay = minuteOfDay;
        }

        public Guid TransactionId { get; }
        public string BusinessId { get; }
        public EconomicActorId Seller { get; }
        public Guid ItemInstanceId { get; }
        public Guid ItemDefinitionId { get; }
        public int Quantity { get; }
        public int UnitPrice { get; }
        public int TotalAmount { get; }
        public ItemInstanceState ItemState { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class ShopSaleTransactionLedger
    {
        private readonly List<ShopSaleTransaction> _transactions = new List<ShopSaleTransaction>();
        private readonly HashSet<Guid> _ids = new HashSet<Guid>();
        private readonly IReadOnlyList<ShopSaleTransaction> _readOnly;

        public ShopSaleTransactionLedger() { _readOnly = _transactions.AsReadOnly(); }
        public IReadOnlyList<ShopSaleTransaction> Transactions => _readOnly;
        public int Count => _transactions.Count;

        public bool TryRecord(ShopSaleTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                string.IsNullOrWhiteSpace(transaction.BusinessId) || !transaction.Seller.IsValid ||
                transaction.ItemInstanceId == Guid.Empty || transaction.ItemDefinitionId == Guid.Empty ||
                transaction.Quantity < 1 || transaction.UnitPrice < 1 || transaction.TotalAmount < 1 ||
                (long)transaction.Quantity * transaction.UnitPrice != transaction.TotalAmount ||
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
