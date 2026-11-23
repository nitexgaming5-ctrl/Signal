using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public sealed class ProductionTransaction
    {
        public ProductionTransaction(Guid transactionId, Guid recipeId, Guid outputDefinitionId,
            int quantity, EconomicActorId operatorActor, int day, int minuteOfDay)
        {
            if (transactionId == Guid.Empty || recipeId == Guid.Empty || outputDefinitionId == Guid.Empty)
                throw new ArgumentException("Production transaction IDs must be valid.");
            if (quantity < 1 || !operatorActor.IsValid || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Production quantity and time must be valid.");
            TransactionId = transactionId;
            RecipeId = recipeId;
            OutputDefinitionId = outputDefinitionId;
            Quantity = quantity;
            OperatorActor = operatorActor;
            Day = day;
            MinuteOfDay = minuteOfDay;
        }
        public Guid TransactionId { get; }
        public Guid RecipeId { get; }
        public Guid OutputDefinitionId { get; }
        public int Quantity { get; }
        public EconomicActorId OperatorActor { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class ProductionTransactionLedger
    {
        private readonly List<ProductionTransaction> _transactions = new List<ProductionTransaction>();
        private readonly HashSet<Guid> _ids = new HashSet<Guid>();
        private readonly IReadOnlyList<ProductionTransaction> _readOnly;
        public ProductionTransactionLedger() { _readOnly = _transactions.AsReadOnly(); }
        public IReadOnlyList<ProductionTransaction> Transactions => _readOnly;
        public int Count => _transactions.Count;
        public bool Contains(Guid id) => _ids.Contains(id);

        public bool TryRecord(ProductionTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                transaction.RecipeId == Guid.Empty || transaction.OutputDefinitionId == Guid.Empty ||
                transaction.Quantity < 1 || !transaction.OperatorActor.IsValid || transaction.Day < 1 ||
                transaction.MinuteOfDay < 0 || transaction.MinuteOfDay >= 1440 ||
                !_ids.Add(transaction.TransactionId))
                return false;
            _transactions.Add(transaction);
            return true;
        }

        public void Clear() { _transactions.Clear(); _ids.Clear(); }
    }
}