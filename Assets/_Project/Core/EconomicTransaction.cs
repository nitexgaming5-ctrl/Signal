using System;
using System.Collections.Generic;
namespace Signal.Core
{
    public enum AssetTransferReason { Gift = 0, Purchase = 1, Theft = 2, ServicePayment = 3, SupplierDelivery = 4, Other = 5 }

    public sealed class EconomicTransaction
    {
        internal EconomicTransaction(Guid transactionId, Guid itemInstanceId, Guid itemDefinitionId, int quantity,
            EconomicActorId fromActor, EconomicActorId toActor, AssetTransferReason reason, int day, int minuteOfDay)
        {
            TransactionId = transactionId; ItemInstanceId = itemInstanceId; ItemDefinitionId = itemDefinitionId;
            Quantity = quantity; FromActor = fromActor; ToActor = toActor; Reason = reason; Day = day; MinuteOfDay = minuteOfDay;
        }
        public Guid TransactionId { get; }
        public Guid ItemInstanceId { get; }
        public Guid ItemDefinitionId { get; }
        public int Quantity { get; }
        public EconomicActorId FromActor { get; }
        public EconomicActorId ToActor { get; }
        public AssetTransferReason Reason { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class EconomicTransactionLedger
    {
        private readonly List<EconomicTransaction> _transactions = new List<EconomicTransaction>();
        private readonly HashSet<Guid> _transactionIds = new HashSet<Guid>();
        private readonly IReadOnlyList<EconomicTransaction> _readOnlyTransactions;
        public EconomicTransactionLedger() { _readOnlyTransactions = _transactions.AsReadOnly(); }
        public IReadOnlyList<EconomicTransaction> Transactions => _readOnlyTransactions;
        public int Count => _transactions.Count;
        public bool Contains(Guid transactionId) => _transactionIds.Contains(transactionId);
        public bool TryRecord(EconomicTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty || transaction.ItemInstanceId == Guid.Empty ||
                transaction.ItemDefinitionId == Guid.Empty || transaction.Quantity < 1 ||
                !transaction.FromActor.IsValid || !transaction.ToActor.IsValid || transaction.FromActor == transaction.ToActor ||
                transaction.Day < 1 || transaction.MinuteOfDay < 0 || transaction.MinuteOfDay >= 1440 ||
                !Enum.IsDefined(typeof(AssetTransferReason), transaction.Reason) || !_transactionIds.Add(transaction.TransactionId)) return false;
            _transactions.Add(transaction);
            return true;
        }
    }
}
