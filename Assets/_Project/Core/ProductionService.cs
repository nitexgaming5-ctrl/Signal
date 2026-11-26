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
            public Removal(GridInventory inventory, Guid instanceId, int quantity, int originalCount)
            {
                Inventory = inventory;
                InstanceId = instanceId;
                Quantity = quantity;
                OriginalCount = originalCount;
            }
            public GridInventory Inventory { get; }
            public Guid InstanceId { get; }
            public int Quantity { get; }
            public int OriginalCount { get; }
        }

        public static ProductionResult TryProduce(Guid recipeId, Recipe recipe, GridInventory inventory,
            EconomicActorId operatorActor, EconomicContainerRegistry containers,
            AssetOwnershipBook ownership, ProductionTransactionLedger history, int day, int minuteOfDay,
            ItemInstanceState? outputState = null)
        {
            if (inventory == null)
                return new ProductionResult(ProductionStatus.InvalidRequest, null);
            return TryProduce(recipeId, recipe, new[] { inventory }, inventory, operatorActor,
                containers, ownership, history, day, minuteOfDay, outputState);
        }

        /// <summary>Consumes ingredients across accessible carried grids and places output in one capacity-approved grid.</summary>
        public static ProductionResult TryProduce(Guid recipeId, Recipe recipe,
            IReadOnlyList<GridInventory> inventories, GridInventory outputInventory,
            EconomicActorId operatorActor, EconomicContainerRegistry containers,
            AssetOwnershipBook ownership, ProductionTransactionLedger history, int day, int minuteOfDay,
            ItemInstanceState? outputState = null)
        {
            if (recipeId == Guid.Empty || recipe == null || inventories == null || inventories.Count == 0 ||
                outputInventory == null || !operatorActor.IsValid || containers == null || ownership == null ||
                history == null || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return new ProductionResult(ProductionStatus.InvalidRequest, null);

            var accessible = new List<GridInventory>();
            for (int i = 0; i < inventories.Count; i++)
            {
                GridInventory candidate = inventories[i];
                if (candidate == null || accessible.Contains(candidate)) continue;
                if (!containers.TryGetCustodian(candidate, out EconomicActorId custodian) || custodian != operatorActor)
                    return new ProductionResult(ProductionStatus.OwnershipMismatch, null);
                accessible.Add(candidate);
            }
            if (accessible.Count == 0 || !accessible.Contains(outputInventory) ||
                !containers.TryGetCustodian(outputInventory, out EconomicActorId outputCustodian) ||
                outputCustodian != operatorActor)
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
                for (int inventoryIndex = 0; inventoryIndex < accessible.Count; inventoryIndex++)
                    foreach (PlacedItem item in accessible[inventoryIndex].Items)
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
                for (int inventoryIndex = 0; inventoryIndex < accessible.Count && remaining > 0; inventoryIndex++)
                    foreach (PlacedItem item in accessible[inventoryIndex].Items)
                    {
                        if (remaining == 0) break;
                        if (item.Type != type ||
                            !ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) ||
                            owner != operatorActor)
                            continue;

                        int take = Math.Min(item.Count, remaining);
                        removals.Add(new Removal(accessible[inventoryIndex], item.ItemInstanceId, take, item.Count));
                        remaining -= take;
                    }
                if (remaining != 0)
                    return new ProductionResult(ProductionStatus.MissingInputs, null);
            }

            ItemInstanceState producedState = outputState ?? ItemInstanceState.Default;
            foreach (PlacedItem item in outputInventory.Items)
            {
                if (item.Type != recipe.Output.Type || item.State != producedState) continue;
                if (!ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) || owner != operatorActor)
                    return new ProductionResult(ProductionStatus.OwnershipMismatch, null);
            }

            var snapshots = new Dictionary<GridInventory, InventorySnapshot>();
            var removedOwners = new List<Guid>();
            var addedOwners = new List<Guid>();
            var changedInventories = new HashSet<GridInventory>();
            for (int i = 0; i < accessible.Count; i++)
                snapshots.Add(accessible[i], accessible[i].CreateSnapshot());
            var priorOutputIds = new HashSet<Guid>();
            foreach (PlacedItem item in snapshots[outputInventory].Items)
                priorOutputIds.Add(item.ItemInstanceId);

            foreach (Removal removal in removals)
            {
                if (!removal.Inventory.TryRemoveItemInstanceSilently(removal.InstanceId, removal.Quantity))
                    return RollBack(ProductionStatus.InvalidRequest, snapshots, ownership,
                        operatorActor, removedOwners, addedOwners);

                changedInventories.Add(removal.Inventory);

                if (removal.Quantity == removal.OriginalCount)
                {
                    if (!ownership.TryRemove(removal.InstanceId, operatorActor))
                        return RollBack(ProductionStatus.OwnershipMismatch, snapshots, ownership,
                            operatorActor, removedOwners, addedOwners);
                    removedOwners.Add(removal.InstanceId);
                }
            }

            int outputLeftover = outputInventory.AddSilently(recipe.Output.Type, recipe.Output.Count, producedState, out bool outputChanged);
            if (outputLeftover > 0)
                return RollBack(ProductionStatus.NoSpaceForOutput, snapshots, ownership,
                    operatorActor, removedOwners, addedOwners);

            foreach (PlacedItem item in outputInventory.Items)
            {
                if (priorOutputIds.Contains(item.ItemInstanceId)) continue;
                if (!ownership.TryRegister(item.ItemInstanceId, operatorActor))
                    return RollBack(ProductionStatus.OwnershipMismatch, snapshots, ownership,
                        operatorActor, removedOwners, addedOwners);
                addedOwners.Add(item.ItemInstanceId);
            }

            if (outputChanged)
                changedInventories.Add(outputInventory);

            var transaction = new ProductionTransaction(Guid.NewGuid(), recipeId,
                recipe.Output.Type.Id, recipe.Output.Count, operatorActor, day, minuteOfDay);
            if (!history.TryRecord(transaction))
                return RollBack(ProductionStatus.HistoryConflict, snapshots, ownership,
                    operatorActor, removedOwners, addedOwners);

            // Publish only after ingredients, output ownership and production history commit.
            foreach (GridInventory changedInventory in changedInventories)
                changedInventory.NotifyChanged();
            return new ProductionResult(ProductionStatus.Completed, transaction);
        }

        private static ProductionResult RollBack(ProductionStatus status,
            Dictionary<GridInventory, InventorySnapshot> snapshots, AssetOwnershipBook ownership,
            EconomicActorId operatorActor, List<Guid> removedOwners, List<Guid> addedOwners)
        {
            for (int i = addedOwners.Count - 1; i >= 0; i--)
                ownership.TryRemove(addedOwners[i], operatorActor);
            foreach (KeyValuePair<GridInventory, InventorySnapshot> snapshot in snapshots)
                snapshot.Key.RestoreSilently(snapshot.Value);
            for (int i = 0; i < removedOwners.Count; i++)
                ownership.TryRegister(removedOwners[i], operatorActor);
            return new ProductionResult(status, null);
        }
    }
}