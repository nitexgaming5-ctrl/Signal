using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public static class StackLineageSaveMapper
    {
        public static void Capture(StackLineageBook source, SaveData destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (destination.stackMergeLineage == null) destination.stackMergeLineage = new List<SavedStackMergeRecord>();
            destination.stackMergeLineage.Clear();
            if (source == null) return;
            foreach (StackMergeRecord record in source.Merges)
            {
                if (string.IsNullOrWhiteSpace(record.OwnerActorId) ||
                    string.IsNullOrWhiteSpace(record.SourceCustodianId) ||
                    string.IsNullOrWhiteSpace(record.DestinationCustodianId))
                    continue;
                destination.stackMergeLineage.Add(new SavedStackMergeRecord
                {
                    operationId = record.OperationId.ToString(),
                    sourceStackId = record.SourceStackId.ToString(),
                    destinationStackId = record.DestinationStackId.ToString(),
                    itemDefinitionId = record.ItemDefinitionId.ToString(),
                    quantity = record.Quantity,
                    ownerActorId = record.OwnerActorId,
                    sourceCustodianId = record.SourceCustodianId,
                    destinationCustodianId = record.DestinationCustodianId,
                    operationKind = (int)record.OperationKind,
                    day = record.Day,
                    minuteOfDay = record.MinuteOfDay,
                    sourceRetired = record.SourceRetired
                });
            }
        }

        public static int Apply(SaveData source, StackLineageBook destination, AssetOwnershipBook ownership = null, params GridInventory[] inventories)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();
            if (source == null || source.stackMergeLineage == null) return 0;
            int skipped = 0;
            foreach (SavedStackMergeRecord saved in source.stackMergeLineage)
            {
                if (saved == null ||
                    !Guid.TryParse(saved.operationId, out Guid operationId) ||
                    !Guid.TryParse(saved.sourceStackId, out Guid sourceId) ||
                    !Guid.TryParse(saved.destinationStackId, out Guid destinationId) ||
                    !Guid.TryParse(saved.itemDefinitionId, out Guid definitionId) ||
                    string.IsNullOrWhiteSpace(saved.ownerActorId) ||
                    string.IsNullOrWhiteSpace(saved.sourceCustodianId) ||
                    string.IsNullOrWhiteSpace(saved.destinationCustodianId))
                { skipped++; continue; }

                try
                {
                    var record = new StackMergeRecord(operationId, sourceId, destinationId, definitionId,
                        saved.quantity, saved.ownerActorId, saved.sourceCustodianId,
                        saved.destinationCustodianId, (StackLineageOperationKind)saved.operationKind,
                        saved.day, saved.minuteOfDay, saved.sourceRetired);
                    if (!destination.TryRecordMerge(record)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            var retiredIds = new HashSet<Guid>();
            foreach (StackMergeRecord record in destination.Merges)
                if (record.SourceRetired) retiredIds.Add(record.SourceStackId);

            if (retiredIds.Count > 0 && inventories != null)
            {
                foreach (GridInventory inventory in inventories)
                {
                    if (inventory == null) continue;
                    bool changed = false;
                    foreach (Guid retiredId in retiredIds)
                    {
                        if (!inventory.TryGetItemInstance(retiredId, out _)) continue;
                        if (inventory.TryRemoveItemInstanceSilently(retiredId))
                            changed = true;
                        if (ownership != null && ownership.TryGetOwner(retiredId, out EconomicActorId owner))
                            ownership.TryRemove(retiredId, owner);
                    }
                    if (changed) inventory.NotifyChanged();
                }
            }
            return skipped;
        }
    }
}

