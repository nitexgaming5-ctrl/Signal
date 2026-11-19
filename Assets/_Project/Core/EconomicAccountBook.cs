using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Resolves the one clean-money account belonging to each economic actor.</summary>
    public sealed class EconomicAccountBook
    {
        private readonly Dictionary<EconomicActorId, CleanMoneyAccount> _accounts =
            new Dictionary<EconomicActorId, CleanMoneyAccount>();

        public int Count => _accounts.Count;

        public bool TryRegister(EconomicActorId actor, CleanMoneyAccount account)
        {
            if (!actor.IsValid || account == null || _accounts.ContainsKey(actor))
                return false;
            _accounts.Add(actor, account);
            return true;
        }

        /// <summary>Returns an existing account or creates it once with the authored starting balance.</summary>
        public CleanMoneyAccount GetOrCreate(EconomicActorId actor, int startingBalance = 0)
        {
            if (!actor.IsValid)
                throw new ArgumentException("Economic actor ID must be valid.", nameof(actor));
            if (_accounts.TryGetValue(actor, out CleanMoneyAccount existing))
                return existing;

            var account = new CleanMoneyAccount(startingBalance);
            _accounts.Add(actor, account);
            return account;
        }

        public bool TryGetAccount(EconomicActorId actor, out CleanMoneyAccount account)
        {
            if (!actor.IsValid)
            {
                account = null;
                return false;
            }
            return _accounts.TryGetValue(actor, out account);
        }

        /// <summary>Restores a saved balance, creating an account only for a valid saved actor.</summary>
        public bool RestoreBalance(EconomicActorId actor, int balance)
        {
            if (!actor.IsValid || balance < 0)
                return false;

            CleanMoneyAccount account = GetOrCreate(actor);
            account.Restore(balance);
            return true;
        }

        public IEnumerable<KeyValuePair<EconomicActorId, CleanMoneyAccount>> Accounts => _accounts;
    }
}
