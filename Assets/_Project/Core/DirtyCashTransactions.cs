using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum IllicitExchangeStatus
    {
        Completed, InvalidRequest, ItemNotFound, ItemNotStolen, OwnershipMismatch,
        ContactStorageFull, RewardInventoryFull, TransferFailed
    }

    public sealed class IllicitExchangeTransaction
    {
        internal IllicitExchangeTransaction(Guid transactionId, Guid itemInstanceId, Guid itemDefinitionId,
            int quantity, EconomicActorId seller, EconomicActorId buyer, int dirtyCashUnits, int day, int minuteOfDay)
        {
            TransactionId = transactionId; ItemInstanceId = itemInstanceId; ItemDefinitionId = itemDefinitionId;
            Quantity = quantity; Seller = seller; Buyer = buyer; DirtyCashUnits = dirtyCashUnits;
            Day = day; MinuteOfDay = minuteOfDay;
        }
        public Guid TransactionId { get; }
        public Guid ItemInstanceId { get; }
        public Guid ItemDefinitionId { get; }
        public int Quantity { get; }
        public EconomicActorId Seller { get; }
        public EconomicActorId Buyer { get; }
        public int DirtyCashUnits { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class IllicitExchangeTransactionLedger
    {
        private readonly List<IllicitExchangeTransaction> _transactions = new List<IllicitExchangeTransaction>();
        private readonly HashSet<Guid> _ids = new HashSet<Guid>();
        private readonly IReadOnlyList<IllicitExchangeTransaction> _readOnly;
        public IllicitExchangeTransactionLedger() { _readOnly = _transactions.AsReadOnly(); }
        public IReadOnlyList<IllicitExchangeTransaction> Transactions => _readOnly;
        public int Count => _transactions.Count;
        public bool TryRecord(IllicitExchangeTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                transaction.ItemInstanceId == Guid.Empty || transaction.ItemDefinitionId == Guid.Empty ||
                transaction.Quantity < 1 || !transaction.Seller.IsValid || !transaction.Buyer.IsValid ||
                transaction.Seller == transaction.Buyer || transaction.DirtyCashUnits < 1 ||
                transaction.Day < 1 || transaction.MinuteOfDay < 0 || transaction.MinuteOfDay >= 1440 ||
                !_ids.Add(transaction.TransactionId)) return false;
            _transactions.Add(transaction);
            return true;
        }
        public void Clear() { _transactions.Clear(); _ids.Clear(); }
    }

    public enum DirtyCashCleaningStatus
    {
        Completed, InvalidRequest, ItemNotFound, OwnershipMismatch, InvalidAmount, AccountOverflow, TransactionFailed
    }

    public sealed class DirtyCashCleaningTransaction
    {
        internal DirtyCashCleaningTransaction(Guid transactionId, Guid cashInstanceId, int dirtyUnits,
            int faceValuePerUnit, int feePercent, int cleanPayout, int feeAmount,
            EconomicActorId cleaner, EconomicActorId feeRecipient, int day, int minuteOfDay)
        {
            TransactionId = transactionId; CashInstanceId = cashInstanceId; DirtyUnits = dirtyUnits;
            FaceValuePerUnit = faceValuePerUnit; FeePercent = feePercent; CleanPayout = cleanPayout;
            FeeAmount = feeAmount; Cleaner = cleaner; FeeRecipient = feeRecipient;
            Day = day; MinuteOfDay = minuteOfDay;
        }
        public Guid TransactionId { get; }
        public Guid CashInstanceId { get; }
        public int DirtyUnits { get; }
        public int FaceValuePerUnit { get; }
        public int FeePercent { get; }
        public int CleanPayout { get; }
        public int FeeAmount { get; }
        public EconomicActorId Cleaner { get; }
        public EconomicActorId FeeRecipient { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
    }

    public sealed class DirtyCashCleaningTransactionLedger
    {
        private readonly List<DirtyCashCleaningTransaction> _transactions = new List<DirtyCashCleaningTransaction>();
        private readonly HashSet<Guid> _ids = new HashSet<Guid>();
        private readonly IReadOnlyList<DirtyCashCleaningTransaction> _readOnly;
        public DirtyCashCleaningTransactionLedger() { _readOnly = _transactions.AsReadOnly(); }
        public IReadOnlyList<DirtyCashCleaningTransaction> Transactions => _readOnly;
        public int Count => _transactions.Count;
        public bool TryRecord(DirtyCashCleaningTransaction transaction)
        {
            if (transaction == null || transaction.TransactionId == Guid.Empty ||
                transaction.CashInstanceId == Guid.Empty || transaction.DirtyUnits < 1 ||
                transaction.FaceValuePerUnit < 1 || transaction.FeePercent < 0 || transaction.FeePercent >= 100 ||
                transaction.CleanPayout < 1 || transaction.FeeAmount < 0 ||
                decimal.Floor((decimal)transaction.DirtyUnits * transaction.FaceValuePerUnit *
                    (100 - transaction.FeePercent) / 100m) != transaction.CleanPayout ||
                (decimal)transaction.DirtyUnits * transaction.FaceValuePerUnit - transaction.CleanPayout != transaction.FeeAmount ||
                !transaction.Cleaner.IsValid || !transaction.FeeRecipient.IsValid ||
                transaction.Cleaner == transaction.FeeRecipient || transaction.Day < 1 || transaction.MinuteOfDay < 0 ||
                transaction.MinuteOfDay >= 1440 || !_ids.Add(transaction.TransactionId)) return false;
            _transactions.Add(transaction);
            return true;
        }
        public void Clear() { _transactions.Clear(); _ids.Clear(); }
    }

    public static class DirtyCashRules
    {
        public const int FaceValuePerUnit = 10;

        public static bool TryGetCleaningAmounts(int dirtyUnits, int feePercent,
            out int payout, out int feeAmount)
        {
            payout = 0;
            feeAmount = 0;
            if (dirtyUnits < 1 || feePercent < 0 || feePercent >= 100) return false;
            long gross = (long)dirtyUnits * FaceValuePerUnit;
            long clean = gross * (100 - feePercent) / 100;
            long fee = gross - clean;
            if (clean < 1 || clean > int.MaxValue || fee < 0 || fee > int.MaxValue) return false;
            payout = (int)clean;
            feeAmount = (int)fee;
            return true;
        }
    }

    public readonly struct IllicitExchangeResult
    {
        internal IllicitExchangeResult(IllicitExchangeStatus status, IllicitExchangeTransaction transaction)
        { Status = status; Transaction = transaction; }
        public IllicitExchangeStatus Status { get; }
        public IllicitExchangeTransaction Transaction { get; }
        public bool Success => Status == IllicitExchangeStatus.Completed;
    }

    public static class IllicitExchangeService
    {
        public static IllicitExchangeResult TryExchange(GridInventory playerInventory, GridInventory contactInventory,
            Guid itemInstanceId, ItemType acceptedItem, int acceptedQuantity, ItemType dirtyCash,
            int dirtyCashUnits, EconomicActorId contactActor, AssetOwnershipBook ownership,
            EconomicTransactionLedger economicLedger, IllicitExchangeTransactionLedger ledger,
            int day, int minuteOfDay)
        {
            if (playerInventory == null || contactInventory == null || ownership == null ||
                economicLedger == null || ledger == null ||
                ReferenceEquals(playerInventory, contactInventory) || itemInstanceId == Guid.Empty ||
                acceptedItem.Id == Guid.Empty || dirtyCash.Id == Guid.Empty || acceptedItem == dirtyCash ||
                acceptedQuantity < 1 || acceptedQuantity > acceptedItem.MaxStack ||
                dirtyCashUnits < 1 || dirtyCashUnits > dirtyCash.MaxStack || !contactActor.IsValid ||
                contactActor == EconomicActorId.Player || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new IllicitExchangeResult(IllicitExchangeStatus.InvalidRequest, null);
            if (!playerInventory.TryGetItemInstance(itemInstanceId, out PlacedItem item))
                return new IllicitExchangeResult(IllicitExchangeStatus.ItemNotFound, null);
            if (item.Type != acceptedItem || item.Count != acceptedQuantity ||
                item.State.Ownership != ItemOwnershipStatus.Stolen)
                return new IllicitExchangeResult(IllicitExchangeStatus.ItemNotStolen, null);
            if (!ownership.TryGetOwner(itemInstanceId, out EconomicActorId currentOwner) ||
                currentOwner != EconomicActorId.Player)
                return new IllicitExchangeResult(IllicitExchangeStatus.OwnershipMismatch, null);

            InventorySnapshot playerBefore = playerInventory.CreateSnapshot();
            InventorySnapshot contactBefore = contactInventory.CreateSnapshot();
            InventoryTransferResult moved = InventoryTransfer.MoveStackSilently(playerInventory, contactInventory, itemInstanceId);
            if (!moved.Success)
                return new IllicitExchangeResult(IllicitExchangeStatus.TransferFailed, null);
            if (moved.Moved != acceptedQuantity)
                return new IllicitExchangeResult(IllicitExchangeStatus.ContactStorageFull, null);
            if (!ownership.TryTransferOwnership(itemInstanceId, EconomicActorId.Player, contactActor))
            {
                playerInventory.RestoreSilently(playerBefore); contactInventory.RestoreSilently(contactBefore);
                return new IllicitExchangeResult(IllicitExchangeStatus.OwnershipMismatch, null);
            }

            int? rewardLocalId = null;
            for (int y = 0; y < playerInventory.Height && !rewardLocalId.HasValue; y++)
                for (int x = 0; x < playerInventory.Width && !rewardLocalId.HasValue; x++)
                    if (playerInventory.CanPlace(dirtyCash, x, y, false))
                        rewardLocalId = playerInventory.PlaceSilently(dirtyCash, x, y, false, dirtyCashUnits);
            if (!rewardLocalId.HasValue ||
                !playerInventory.TryGet(rewardLocalId.Value, out PlacedItem reward) ||
                !ownership.TryRegister(reward.ItemInstanceId, EconomicActorId.Player))
            {
                playerInventory.RestoreSilently(playerBefore); contactInventory.RestoreSilently(contactBefore);
                ownership.TryTransferOwnership(itemInstanceId, contactActor, EconomicActorId.Player);
                return new IllicitExchangeResult(IllicitExchangeStatus.RewardInventoryFull, null);
            }

            var economicTransaction = new EconomicTransaction(Guid.NewGuid(), item.ItemInstanceId, item.Type.Id,
                item.Count, EconomicActorId.Player, contactActor, AssetTransferReason.IllicitExchange, day, minuteOfDay);
            if (!economicLedger.TryRecord(economicTransaction))
            {
                ownership.TryRemove(reward.ItemInstanceId, EconomicActorId.Player);
                ownership.TryTransferOwnership(itemInstanceId, contactActor, EconomicActorId.Player);
                playerInventory.RestoreSilently(playerBefore); contactInventory.RestoreSilently(contactBefore);
                return new IllicitExchangeResult(IllicitExchangeStatus.TransferFailed, null);
            }

            var transaction = new IllicitExchangeTransaction(Guid.NewGuid(), item.ItemInstanceId, item.Type.Id,
                item.Count, EconomicActorId.Player, contactActor, dirtyCashUnits, day, minuteOfDay);
            if (!ledger.TryRecord(transaction))
            {
                economicLedger.TryRemove(economicTransaction.TransactionId);
                ownership.TryRemove(reward.ItemInstanceId, EconomicActorId.Player);
                ownership.TryTransferOwnership(itemInstanceId, contactActor, EconomicActorId.Player);
                playerInventory.RestoreSilently(playerBefore); contactInventory.RestoreSilently(contactBefore);
                return new IllicitExchangeResult(IllicitExchangeStatus.TransferFailed, null);
            }
            playerInventory.NotifyChanged();
            contactInventory.NotifyChanged();
            return new IllicitExchangeResult(IllicitExchangeStatus.Completed, transaction);
        }
    }

    public readonly struct DirtyCashCleaningResult
    {
        internal DirtyCashCleaningResult(DirtyCashCleaningStatus status, DirtyCashCleaningTransaction transaction)
        { Status = status; Transaction = transaction; }
        public DirtyCashCleaningStatus Status { get; }
        public DirtyCashCleaningTransaction Transaction { get; }
        public bool Success => Status == DirtyCashCleaningStatus.Completed;
    }

    public static class DirtyCashCleaningService
    {
        public static DirtyCashCleaningResult TryClean(GridInventory inventory, Guid cashInstanceId,
            ItemType dirtyCash, EconomicActorId cleaner, AssetOwnershipBook ownership,
            CleanMoneyAccount cleanAccount, CleanMoneyAccount feeAccount, EconomicActorId feeRecipient,
            int feePercent, DirtyCashCleaningTransactionLedger ledger, int day, int minuteOfDay)
        {
            if (inventory == null || ownership == null || cleanAccount == null || feeAccount == null ||
                ReferenceEquals(cleanAccount, feeAccount) || ledger == null || feePercent < 0 || feePercent >= 100 ||
                cashInstanceId == Guid.Empty || dirtyCash.Id == Guid.Empty || !cleaner.IsValid ||
                cleaner != EconomicActorId.Player || !feeRecipient.IsValid || feeRecipient == cleaner ||
                day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.InvalidRequest, null);
            if (!inventory.TryGetItemInstance(cashInstanceId, out PlacedItem item) || item.Type != dirtyCash)
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.ItemNotFound, null);
            if (!ownership.TryGetOwner(cashInstanceId, out EconomicActorId owner) || owner != cleaner)
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.OwnershipMismatch, null);
            if (!DirtyCashRules.TryGetCleaningAmounts(item.Count, feePercent, out int payout, out int feeAmount))
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.InvalidAmount, null);

            InventorySnapshot before = inventory.CreateSnapshot();
            int balanceBefore = cleanAccount.Balance;
            int feeBalanceBefore = feeAccount.Balance;
            if (!inventory.TryRemoveItemInstanceSilently(cashInstanceId, item.Count) ||
                !ownership.TryRemove(cashInstanceId, cleaner))
            {
                inventory.RestoreSilently(before);
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.TransactionFailed, null);
            }
            if (!cleanAccount.TryCredit(payout) || (feeAmount > 0 && !feeAccount.TryCredit(feeAmount)))
            {
                inventory.RestoreSilently(before);
                cleanAccount.Restore(balanceBefore);
                feeAccount.Restore(feeBalanceBefore);
                ownership.TryRegister(cashInstanceId, cleaner);
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.AccountOverflow, null);
            }

            var transaction = new DirtyCashCleaningTransaction(Guid.NewGuid(), cashInstanceId, item.Count,
                DirtyCashRules.FaceValuePerUnit, feePercent, payout, feeAmount,
                cleaner, feeRecipient, day, minuteOfDay);
            if (!ledger.TryRecord(transaction))
            {
                inventory.RestoreSilently(before); cleanAccount.Restore(balanceBefore);
                feeAccount.Restore(feeBalanceBefore);
                ownership.TryRegister(cashInstanceId, cleaner);
                return new DirtyCashCleaningResult(DirtyCashCleaningStatus.TransactionFailed, null);
            }
            inventory.NotifyChanged();
            return new DirtyCashCleaningResult(DirtyCashCleaningStatus.Completed, transaction);
        }
    }
}
