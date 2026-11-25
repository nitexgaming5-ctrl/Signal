using System;

namespace Signal.Core
{
    public enum RepairStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        ContainerNotOwned = 2,
        TargetNotFound = 3,
        UnsupportedTarget = 4,
        KitNotFound = 5,
        UnsupportedKit = 6,
        OwnershipMismatch = 7,
        ConditionNotRepairable = 8,
        KitCannotRepairCondition = 9,
        InventoryConflict = 10
    }

    public readonly struct RepairResult
    {
        public RepairResult(RepairStatus status, ItemCondition? resultingCondition = null)
        {
            Status = status;
            ResultingCondition = resultingCondition;
        }

        public RepairStatus Status { get; }
        public ItemCondition? ResultingCondition { get; }
        public bool Success => Status == RepairStatus.Completed;
    }

    /// <summary>
    /// Performs the first deterministic textile repair operation on one exact Work Jacket stack.
    /// The repair consumes one physical, player-owned patch kit and changes Condition only.
    /// </summary>
    public static class RepairService
    {
        public static RepairResult TryRepair(Guid targetInstanceId, Guid kitInstanceId,
            ItemType targetType, ItemType basicKitType, ItemType reinforcedKitType,
            GridInventory inventory, EconomicActorId operatorActor,
            EconomicContainerRegistry containers, AssetOwnershipBook ownership,
            ItemKnowledgeBook knowledge, int day, int minuteOfDay)
        {
            return TryRepairForOwner(targetInstanceId, kitInstanceId, targetType, basicKitType,
                reinforcedKitType, inventory, operatorActor, operatorActor, containers, ownership,
                knowledge, day, minuteOfDay);
        }

        /// <summary>Repairs a carried item while preserving its registered owner's identity.</summary>
        public static RepairResult TryRepairForOwner(Guid targetInstanceId, Guid kitInstanceId,
            ItemType targetType, ItemType basicKitType, ItemType reinforcedKitType,
            GridInventory inventory, EconomicActorId operatorActor, EconomicActorId targetOwner,
            EconomicContainerRegistry containers, AssetOwnershipBook ownership,
            ItemKnowledgeBook knowledge, int day, int minuteOfDay)
        {
            if (targetInstanceId == Guid.Empty || kitInstanceId == Guid.Empty || targetInstanceId == kitInstanceId ||
                !IsValid(targetType) || !IsValid(basicKitType) || !IsValid(reinforcedKitType) ||
                targetType.Id == basicKitType.Id || targetType.Id == reinforcedKitType.Id ||
                basicKitType.Id == reinforcedKitType.Id || targetType.MaxStack != 1 ||
                basicKitType.MaxStack != 1 || reinforcedKitType.MaxStack != 1 ||
                inventory == null || operatorActor != EconomicActorId.Player || !targetOwner.IsValid || containers == null ||
                ownership == null || knowledge == null || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new RepairResult(RepairStatus.InvalidRequest);

            if (!containers.TryGetCustodian(inventory, out EconomicActorId custodian) || custodian != operatorActor)
                return new RepairResult(RepairStatus.ContainerNotOwned);

            if (!inventory.TryGetItemInstance(targetInstanceId, out PlacedItem target))
                return new RepairResult(RepairStatus.TargetNotFound);
            if (target.Type != targetType || target.Count != 1 || target.Type.MaxStack != 1)
                return new RepairResult(RepairStatus.UnsupportedTarget);

            if (!inventory.TryGetItemInstance(kitInstanceId, out PlacedItem kit))
                return new RepairResult(RepairStatus.KitNotFound);
            bool isBasicKit = kit.Type == basicKitType;
            bool isReinforcedKit = kit.Type == reinforcedKitType;
            if ((!isBasicKit && !isReinforcedKit) || kit.Count != 1 || kit.Type.MaxStack != 1)
                return new RepairResult(RepairStatus.UnsupportedKit);

            if (!ownership.TryGetOwner(targetInstanceId, out EconomicActorId registeredTargetOwner) || registeredTargetOwner != targetOwner ||
                !ownership.TryGetOwner(kitInstanceId, out EconomicActorId kitOwner) || kitOwner != operatorActor)
                return new RepairResult(RepairStatus.OwnershipMismatch);

            if (!TryGetResultingCondition(target.State.Condition, isBasicKit, isReinforcedKit,
                out ItemCondition resultingCondition, out RepairStatus transitionStatus))
                return new RepairResult(transitionStatus);

            ItemInstanceState repairedState = new ItemInstanceState(target.State.Quality, resultingCondition,
                target.State.Authenticity, target.State.Ownership);
            InventorySnapshot before = inventory.CreateSnapshot();

            if (!inventory.TryRemoveItemInstance(kitInstanceId, 1))
                return new RepairResult(RepairStatus.InventoryConflict);

            if (!ownership.TryRemove(kitInstanceId, operatorActor))
            {
                inventory.Restore(before);
                return new RepairResult(RepairStatus.OwnershipMismatch);
            }

            if (!inventory.TryUpdateItemState(targetInstanceId, repairedState))
            {
                inventory.Restore(before);
                ownership.TryRegister(kitInstanceId, operatorActor);
                return new RepairResult(RepairStatus.InventoryConflict);
            }

            knowledge.RecordFact(targetInstanceId, ItemKnowledgeAspect.Condition,
                (int)resultingCondition, ItemKnowledgeSource.DirectInspection, 1f, day, minuteOfDay);
            return new RepairResult(RepairStatus.Completed, resultingCondition);
        }

        private static bool IsValid(ItemType type)
        {
            return type.Id != Guid.Empty && type.Width > 0 && type.Height > 0 && type.MaxStack > 0;
        }

        private static bool TryGetResultingCondition(ItemCondition current, bool isBasicKit, bool isReinforcedKit,
            out ItemCondition resulting, out RepairStatus failureStatus)
        {
            resulting = current;
            failureStatus = RepairStatus.ConditionNotRepairable;

            if (current == ItemCondition.Good || current == ItemCondition.Pristine)
                return false;

            if (isBasicKit)
            {
                if (current == ItemCondition.Damaged)
                {
                    resulting = ItemCondition.Worn;
                    return true;
                }
                if (current == ItemCondition.Worn)
                {
                    resulting = ItemCondition.Good;
                    return true;
                }
                return false;
            }

            if (isReinforcedKit)
            {
                if (current == ItemCondition.Damaged)
                {
                    resulting = ItemCondition.Good;
                    return true;
                }
                failureStatus = RepairStatus.KitCannotRepairCondition;
            }

            return false;
        }
    }
}
