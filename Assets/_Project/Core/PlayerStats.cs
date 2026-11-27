using System;
namespace Signal.Core
{
    public sealed class PlayerStats
    {
        public const int MaxHealth = 100;
        private int _health;
        private int _maximumHealth = MaxHealth;
        private readonly CleanMoneyAccount _cleanMoney;
        public PlayerStats(int money = 0, int health = MaxHealth)
        {
            _cleanMoney = new CleanMoneyAccount(money);
            _cleanMoney.Changed += OnCleanMoneyChanged;
            _health = Clamp(health);
        }
        /// <summary>Compatibility view of the player's clean bank/card balance.</summary>
        public int Money => _cleanMoney.Balance;
        public CleanMoneyAccount CleanMoney => _cleanMoney;
        public int Health => _health;
        public int MaximumHealth => _maximumHealth;
        public event Action<PlayerStats> Changed;

        public bool TrySetMaximumHealth(int maximumHealth)
        {
            if (maximumHealth <= 0)
                return false;
            int clamped = Math.Min(int.MaxValue, maximumHealth);
            bool maxChanged = clamped != _maximumHealth;
            bool healthChanged = _health > clamped;
            if (!maxChanged && !healthChanged)
                return true;
            _maximumHealth = clamped;
            if (healthChanged)
                _health = clamped;
            Raise();
            return true;
        }
        public void Earn(int amount) => TryEarn(amount);
        public bool TryEarn(int amount) => _cleanMoney.TryCredit(amount);
        public bool TrySpend(int amount) => _cleanMoney.TryDebit(amount);
        public void Damage(int amount)
        {
            if (amount <= 0 || _health == 0) return;
            _health = Math.Max(0, _health - amount);
            Raise();
        }
        public void Restore(int money, int health)
        {
            bool moneyChanged = _cleanMoney.RestoreSilently(Math.Max(0, money));
            int newHealth = Clamp(health, _maximumHealth);
            bool healthChanged = newHealth != _health;
            if (!moneyChanged && !healthChanged) return;
            _health = newHealth;
            Raise();
        }
        private void OnCleanMoneyChanged(CleanMoneyAccount account) => Raise();
        private static int Clamp(int health, int maximumHealth) => Math.Min(maximumHealth, Math.Max(0, health));
        private void Raise() => Changed?.Invoke(this);
    }
}
