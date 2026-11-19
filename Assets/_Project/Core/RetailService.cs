using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum RetailPurchaseStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        AccountNotFound = 2,
        InsufficientFunds = 3,
        InventoryFull = 4,
        OwnershipInvalid = 5,
        AccountOverflow = 6,
        CommitFailed = 7
    }

    public readonly struct RetailPurchaseResult
    {
        internal RetailPurchaseResult(RetailPurchaseStatus status, RetailTransaction transaction)
        {
            Status = status;
            Transaction = transaction;
        }

        public RetailPurchaseStatus Status { get; }
        public RetailTransaction Transaction { get; }
        public bool Success => Status == RetailPurchaseStatus.Completed;
    }

    /// <summary>Coordinates an atomic purchase from an unlimited authored business catalogue.</summary>
    public sealed class RetailService
    {
        private readonly EconomicAccountBook _accounts;
        private readonly AssetOwnershipBook _ownership;
        private readonly RetailTransactionLedger _ledger;

        public RetailService(EconomicAccountBook accounts, AssetOwnershipBook ownership, RetailTransactionLedger ledger)
        {
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }

        public RetailPurchaseResult TryPurchase(string businessId, GridInventory customerInventory,
            EconomicActorId customer, EconomicActorId owner, ItemType item, ItemInstanceState itemState,
            int quantity, int unitPrice, int day, int minuteOfDay)
        {
            if (string.IsNullOrWhiteSpace(businessId) || customerInventory == null ||
                !customer.IsValid || !owner.IsValid || customer == owner ||
                item.Id == Guid.Empty || quantity < 1 || unitPrice < 1 ||
                (long)quantity * unitPrice > int.MaxValue ||
                day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new RetailPurchaseResult(RetailPurchaseStatus.InvalidRequest, null);

            if (!_accounts.TryGetAccount(customer, out CleanMoneyAccount customerAccount) ||
                !_accounts.TryGetAccount(owner, out CleanMoneyAccount ownerAccount))
                return new RetailPurchaseResult(RetailPurchaseStatus.AccountNotFound, null);

            int total = quantity * unitPrice;
            if (customerAccount.Balance < total)
                return new RetailPurchaseResult(RetailPurchaseStatus.InsufficientFunds, null);

            // Any existing stack that Add may top up must already belong to the customer.
            foreach (PlacedItem existing in customerInventory.Items)
            {
                if (existing.Type != item || existing.State != itemState)
                    continue;
                if (!_ownership.TryGetOwner(existing.ItemInstanceId, out EconomicActorId existingOwner) ||
                    existingOwner != customer)
                    return new RetailPurchaseResult(RetailPurchaseStatus.OwnershipInvalid, null);
            }

            InventorySnapshot inventoryBefore = customerInventory.CreateSnapshot();
            int customerBalanceBefore = customerAccount.Balance;
            int ownerBalanceBefore = ownerAccount.Balance;
            var previousIds = new HashSet<Guid>();
            foreach (PlacedItem existing in customerInventory.Items)
                previousIds.Add(existing.ItemInstanceId);

            if (!customerAccount.TryDebit(total))
                return new RetailPurchaseResult(RetailPurchaseStatus.InsufficientFunds, null);
            if (!ownerAccount.TryCredit(total))
            {
                customerAccount.Restore(customerBalanceBefore);
                return new RetailPurchaseResult(RetailPurchaseStatus.AccountOverflow, null);
            }

            if (customerInventory.Add(item, quantity, itemState) != 0)
            {
                Rollback(customerInventory, inventoryBefore, customer, customerAccount, customerBalanceBefore,
                    ownerAccount, ownerBalanceBefore, null);
                return new RetailPurchaseResult(RetailPurchaseStatus.InventoryFull, null);
            }

            var registeredIds = new List<Guid>();
            foreach (PlacedItem placed in customerInventory.Items)
            {
                if (previousIds.Contains(placed.ItemInstanceId))
                    continue;
                if (!_ownership.TryRegister(placed.ItemInstanceId, customer))
                {
                    Rollback(customerInventory, inventoryBefore, customer, customerAccount, customerBalanceBefore,
                        ownerAccount, ownerBalanceBefore, registeredIds);
                    return new RetailPurchaseResult(RetailPurchaseStatus.OwnershipInvalid, null);
                }
                registeredIds.Add(placed.ItemInstanceId);
            }

            var transaction = new RetailTransaction(Guid.NewGuid(), businessId, customer, owner,
                item.Id, quantity, total, day, minuteOfDay);
            if (!_ledger.TryRecord(transaction))
            {
                Rollback(customerInventory, inventoryBefore, customerAccount, customerBalanceBefore,
                    ownerAccount, ownerBalanceBefore, registeredIds);
                return new RetailPurchaseResult(RetailPurchaseStatus.CommitFailed, null);
            }

            return new RetailPurchaseResult(RetailPurchaseStatus.Completed, transaction);
        }

        private void Rollback(GridInventory inventory, InventorySnapshot inventoryBefore,
            EconomicActorId customerActor, CleanMoneyAccount customer, int customerBalance, CleanMoneyAccount owner, int ownerBalance,
            List<Guid> registeredIds)
        {
            if (registeredIds != null)
                for (int i = registeredIds.Count - 1; i >= 0; i--)
                    _ownership.TryRemove(registeredIds[i], customerActor);
            inventory.Restore(inventoryBefore);
            customer.Restore(customerBalance);
            owner.Restore(ownerBalance);
        }
    }
}
