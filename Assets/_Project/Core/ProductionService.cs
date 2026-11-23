using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum ProductionStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        MissingInputs = 2,
        OwnershipMismatch = 3,
        NoSpaceForOutput = 4,
        HistoryConflict = 5
    }

    public readonly struct ProductionResult
    {
        internal ProductionResult(ProductionStatus status, ProductionTransaction transaction)
        {
            Status = status;
            Transaction = transaction;
        }
        public ProductionStatus Status { get; }
        public ProductionTransaction Transaction { get; }
        public bool Success => Status == ProductionStatus.Completed;
    }

    /// <summary>Executes an authored recipe against an actor-owned inventory as one reversible operation.</summary>
    public static class ProductionService
    {
        private readonly struct Removal
        {
            public Removal(Guid instanceId, int quantity, int originalCount)
            {
                InstanceId = instanceId;
                Quantity = quantity;
                OriginalCount = originalCount;
            }
            public Guid InstanceId { get; }
            public int Quantity { get; }
            public int OriginalCount { get; }
        }

        public static ProductionResult TryProduce(Guid recipeId, Recipe recipe, GridInventory inventory,
            EconomicActorId operatorActor, EconomicContainerRegistry containers,
            AssetOwnershipBook ownership, ProductionTransactionLedger history, int day, int minuteOfDay,
            ItemInstanceState? outputState = null)
        {
            if (recipeId == Guid.Empty || recipe == null || inventory == null || !operatorActor.IsValid ||
                containers == null || ownership == null || history == null ||
                day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new ProductionResult(ProductionStatus.InvalidRequest, null);

            if (!containers.TryGetCustodian(inventory, out EconomicActorId custodian) || custodian != operatorActor)
                return new ProductionResult(ProductionStatus.OwnershipMismatch, null);

            var required = new Dictionary<ItemType, int>();
            var order = new List<ItemType>();
            try
            {
                foreach (Ingredient ingredient in recipe.Inputs)
                {
                    if (!required.ContainsKey(ingredient.Type))
                    {
                        required.Add(ingredient.Type, ingredient.Count);
                        order.Add(ingredient.Type);
                    }
                    else
                        required[ingredient.Type] = checked(required[ingredient.Type] + ingredient.Count);
                }
            }
            catch (OverflowException)
            {
                return new ProductionResult(ProductionStatus.InvalidRequest, null);
            }

            var removals = new List<Removal>();
            foreach (ItemType type in order)
            {
                int needed = required[type];
                long carried = 0;
                long owned = 0;
                foreach (PlacedItem item in inventory.Items)
                {
                    if (item.Type != type) continue;
                    carried += item.Count;
                    if (ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) && owner == operatorActor)
                        owned += item.Count;
                }

                if (owned < needed)
                    return new ProductionResult(carried < needed
                        ? ProductionStatus.MissingInputs
                        : ProductionStatus.OwnershipMismatch, null);

                int remaining = needed;
                foreach (PlacedItem item in inventory.Items)
                {
                    if (remaining == 0) break;
                    if (item.Type != type ||
                        !ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) ||
                        owner != operatorActor)
                        continue;

                    int take = Math.Min(item.Count, remaining);
                    removals.Add(new Removal(item.ItemInstanceId, take, item.Count));
                    remaining -= take;
                }
                if (remaining != 0)
                    return new ProductionResult(ProductionStatus.MissingInputs, null);
            }

            ItemInstanceState producedState = outputState ?? ItemInstanceState.Default;
            // Add() tops up matching stacks first. Never merge into an asset owned by another actor.
            foreach (PlacedItem item in inventory.Items)
            {
                if (item.Type != recipe.Output.Type || item.State != producedState) continue;
                if (!ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) || owner != operatorActor)
                    return new ProductionResult(ProductionStatus.OwnershipMismatch, null);
            }

            InventorySnapshot before = inventory.CreateSnapshot();
            var removedOwners = new List<Guid>();
            var addedOwners = new List<Guid>();

            foreach (Removal removal in removals)
            {
                if (!inventory.TryRemoveItemInstance(removal.InstanceId, removal.Quantity))
                    return RollBack(ProductionStatus.InvalidRequest, inventory, before,
                        ownership, operatorActor, removedOwners, addedOwners);

                if (removal.Quantity == removal.OriginalCount)
                {
                    if (!ownership.TryRemove(removal.InstanceId, operatorActor))
                        return RollBack(ProductionStatus.OwnershipMismatch, inventory, before,
                            ownership, operatorActor, removedOwners, addedOwners);
                    removedOwners.Add(removal.InstanceId);
                }
            }

            int leftover = inventory.Add(recipe.Output.Type, recipe.Output.Count, producedState);
            if (leftover > 0)
                return RollBack(ProductionStatus.NoSpaceForOutput, inventory, before,
                    ownership, operatorActor, removedOwners, addedOwners);

            var priorIds = new HashSet<Guid>();
            foreach (PlacedItem item in before.Items) priorIds.Add(item.ItemInstanceId);

            foreach (PlacedItem item in inventory.Items)
            {
                if (priorIds.Contains(item.ItemInstanceId)) continue;
                if (!ownership.TryRegister(item.ItemInstanceId, operatorActor))
                    return RollBack(ProductionStatus.OwnershipMismatch, inventory, before,
                        ownership, operatorActor, removedOwners, addedOwners);
                addedOwners.Add(item.ItemInstanceId);
            }

            var transaction = new ProductionTransaction(Guid.NewGuid(), recipeId,
                recipe.Output.Type.Id, recipe.Output.Count, operatorActor, day, minuteOfDay);
            if (!history.TryRecord(transaction))
                return RollBack(ProductionStatus.HistoryConflict, inventory, before,
                    ownership, operatorActor, removedOwners, addedOwners);

            return new ProductionResult(ProductionStatus.Completed, transaction);
        }

        private static ProductionResult RollBack(ProductionStatus status, GridInventory inventory,
            InventorySnapshot before, AssetOwnershipBook ownership, EconomicActorId operatorActor,
            List<Guid> removedOwners, List<Guid> addedOwners)
        {
            for (int i = addedOwners.Count - 1; i >= 0; i--)
                ownership.TryRemove(addedOwners[i], operatorActor);
            inventory.Restore(before);
            for (int i = 0; i < removedOwners.Count; i++)
                ownership.TryRegister(removedOwners[i], operatorActor);
            return new ProductionResult(status, null);
        }
    }
}