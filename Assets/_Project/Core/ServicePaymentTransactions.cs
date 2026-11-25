using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>A clean CARD payment for a completed service, separate from physical asset transfers.</summary>
    public sealed class ServicePaymentTransaction
    {
        internal ServicePaymentTransaction(Guid transactionId, Guid orderId, Guid itemInstanceId,
            EconomicActorId payer, EconomicActorId payee, int amount, int day, int minuteOfDay)
        {
            TransactionId = transactionId;
            OrderId = orderId;
            ItemInstanceId = itemInstanceId;
            Payer = payer;
            Payee = payee;
            Amount = amount;
            Day = day;
            MinuteOfDay = minuteOfDay;
        }

        public Guid TransactionId { get; }
        public Guid OrderId { get; }
        public Guid ItemInstanceId { get; }
        public EconomicActorId Payer { get; }
        public EconomicActorId Payee { get; }
        public int Amount { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class ServicePaymentTransactionLedger
    {
        private readonly List<ServicePaymentTransaction> _transactions = new List<ServicePaymentTransaction>();
        private readonly HashSet<Guid> _transactionIds = new HashSet<Guid>();
        private readonly HashSet<Guid> _orderIds = new HashSet<Guid>();
        private readonly IReadOnlyList<ServicePaymentTransaction> _readOnlyTransactions;

        public ServicePaymentTransactionLedger()
        {
            _readOnlyTransactions = _transactions.AsReadOnly();
        }

        public IReadOnlyList<ServicePaymentTransaction> Transactions => _readOnlyTransactions;
        public int Count => _transactions.Count;
        public bool Contains(Guid transactionId) => _transactionIds.Contains(transactionId);

        public bool TryRecord(ServicePaymentTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                transaction.OrderId == Guid.Empty || transaction.ItemInstanceId == Guid.Empty ||
                !transaction.Payer.IsValid || !transaction.Payee.IsValid ||
                transaction.Payer == transaction.Payee || transaction.Amount < 1 ||
                transaction.Day < 1 || transaction.MinuteOfDay < 0 || transaction.MinuteOfDay >= 1440 ||
                _transactionIds.Contains(transaction.TransactionId) || _orderIds.Contains(transaction.OrderId))
                return false;
            _transactions.Add(transaction);
            _transactionIds.Add(transaction.TransactionId);
            _orderIds.Add(transaction.OrderId);
            return true;
        }

        public bool TryRemove(Guid transactionId)
        {
            if (transactionId == Guid.Empty || !_transactionIds.Contains(transactionId)) return false;
            for (int i = 0; i < _transactions.Count; i++)
            {
                if (_transactions[i].TransactionId != transactionId) continue;
                _transactions.RemoveAt(i);
                _transactionIds.Remove(transactionId);
                _orderIds.Remove(_transactions[i].OrderId);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _transactions.Clear();
            _transactionIds.Clear();
            _orderIds.Clear();
        }
    }
}