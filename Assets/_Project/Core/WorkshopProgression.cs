using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Persistent record of practical workshop improvements the player has acquired.</summary>
    public sealed class WorkshopProgressionBook
    {
        private readonly HashSet<Guid> _acquired = new HashSet<Guid>();
        public int Count => _acquired.Count;
        public IEnumerable<Guid> AcquiredUpgradeIds => _acquired;
        public bool HasAcquired(Guid upgradeId) => upgradeId != Guid.Empty && _acquired.Contains(upgradeId);
        public bool TryAcquire(Guid upgradeId) => upgradeId != Guid.Empty && _acquired.Add(upgradeId);
        public void Clear() => _acquired.Clear();
    }

    public enum WorkshopUpgradeStatus
    {
        Acquired = 0, InvalidRequest = 1, AlreadyAcquired = 2, ContractPrerequisiteNotMet = 3,
        InsufficientFunds = 4, AccountOverflow = 5, AcquisitionConflict = 6
    }

    public readonly struct WorkshopUpgradeResult
    {
        internal WorkshopUpgradeResult(WorkshopUpgradeStatus status) { Status = status; }
        public WorkshopUpgradeStatus Status { get; }
        public bool Success => Status == WorkshopUpgradeStatus.Acquired;
    }

    public static class WorkshopUpgradeService
    {
        public static WorkshopUpgradeResult TryAcquire(Guid upgradeId, Guid requiredContractId,
            int cost, EconomicActorId recipientActor, CleanMoneyAccount playerAccount,
            CleanMoneyAccount recipientAccount, CustomerContractBook contracts,
            WorkshopProgressionBook progression)
        {
            if (upgradeId == Guid.Empty || cost < 1 ||
                !recipientActor.IsValid || recipientActor == EconomicActorId.Player ||
                playerAccount == null || recipientAccount == null ||
                ReferenceEquals(playerAccount, recipientAccount) || progression == null ||
                (requiredContractId != Guid.Empty && contracts == null))
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.InvalidRequest);

            if (progression.HasAcquired(upgradeId))
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.AlreadyAcquired);
            if (requiredContractId != Guid.Empty &&
                (contracts == null || !contracts.TryGetState(requiredContractId, out CustomerContractState contract) ||
                 contract.Phase != CustomerContractPhase.Completed))
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.ContractPrerequisiteNotMet);
            if (playerAccount.Balance < cost)
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.InsufficientFunds);
            if (recipientAccount.Balance > int.MaxValue - cost)
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.AccountOverflow);

            int playerBefore = playerAccount.Balance;
            int recipientBefore = recipientAccount.Balance;
            if (!playerAccount.TryDebit(cost) || !recipientAccount.TryCredit(cost))
            {
                playerAccount.RestoreSilently(playerBefore);
                recipientAccount.RestoreSilently(recipientBefore);
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.AccountOverflow);
            }
            if (!progression.TryAcquire(upgradeId))
            {
                playerAccount.RestoreSilently(playerBefore);
                recipientAccount.RestoreSilently(recipientBefore);
                return new WorkshopUpgradeResult(WorkshopUpgradeStatus.AcquisitionConflict);
            }
            return new WorkshopUpgradeResult(WorkshopUpgradeStatus.Acquired);
        }
    }
}
