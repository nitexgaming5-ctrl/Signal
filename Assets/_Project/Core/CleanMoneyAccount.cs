using System;
namespace Signal.Core
{
    /// <summary>Clean bank/card balance. Physical dirty-money items are not represented here.</summary>
    public sealed class CleanMoneyAccount
    {
        private int _balance;
        public CleanMoneyAccount(int startingBalance = 0) { _balance = Math.Max(0, startingBalance); }
        public int Balance => _balance;
        public event Action<CleanMoneyAccount> Changed;
        public bool TryCredit(int amount)
        {
            if (amount <= 0 || amount > int.MaxValue - _balance) return false;
            _balance += amount;
            Changed?.Invoke(this);
            return true;
        }
        public bool TryDebit(int amount)
        {
            if (amount <= 0 || amount > _balance) return false;
            _balance -= amount;
            Changed?.Invoke(this);
            return true;
        }
        public void Restore(int balance)
        {
            if (RestoreSilently(Math.Max(0, balance))) Changed?.Invoke(this);
        }
        internal bool RestoreSilently(int balance)
        {
            if (_balance == balance) return false;
            _balance = balance;
            return true;
        }
    }
}
