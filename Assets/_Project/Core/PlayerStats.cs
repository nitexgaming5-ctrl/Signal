using System;

namespace Signal.Core
{
    public sealed class PlayerStats
    {
        public const int MaxHealth = 100;

        private int _money;
        private int _health;

        public PlayerStats(int money = 0, int health = MaxHealth)
        {
            _money = Math.Max(0, money);
            _health = Clamp(health);
        }

        public int Money => _money;

        public int Health => _health;

        public event Action<PlayerStats> Changed;

        public void Earn(int amount)
        {
            if (amount <= 0)
                return;

            _money += amount;
            Raise();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > _money)
                return false;

            _money -= amount;
            Raise();
            return true;
        }

        public void Damage(int amount)
        {
            if (amount <= 0 || _health == 0)
                return;

            _health = Math.Max(0, _health - amount);
            Raise();
        }

        /// <summary>Sets both values at once (used when loading a save). Fires Changed only if something differs.</summary>
        public void Restore(int money, int health)
        {
            int newMoney = Math.Max(0, money);
            int newHealth = Clamp(health);

            if (newMoney == _money && newHealth == _health)
                return;

            _money = newMoney;
            _health = newHealth;
            Raise();
        }

        private static int Clamp(int health) => Math.Min(MaxHealth, Math.Max(0, health));

        private void Raise() => Changed?.Invoke(this);
    }
}
