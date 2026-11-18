using System;
using System.Collections.Generic;

namespace Signal.Core
{
    [Serializable]
    public sealed class SavedItem
    {
        public string typeId;
        public string instanceId;
        public int x;
        public int y;
        public bool rotated;
        public int count;
        public int quality;
        public int condition;
        public int authenticity;
        public int ownership;
    }

    [Serializable]
    public sealed class SavedItemKnowledge
    {
        public string instanceId;
        public int aspect;
        public int value;
        public int source;
        public float confidence;
        public int learnedDay;
        public int learnedMinuteOfDay;
    }

    [Serializable]
    public sealed class SavedDiscoverySignal
    {
        public string signalId;
        public string subjectId;
        public int type;
        public string sourceId;
        public string description;
        public float confidence;
        public int learnedDay;
        public int learnedMinuteOfDay;
    }

    [Serializable]
    public sealed class SavedOpportunityState
    {
        public string opportunityId;
        public int phase;
        public int resolution;
        public string parcelInstanceId;
        public int acceptanceCount;
        public int declineCount;
        public int lastAcceptedDay;
        public int lastAcceptedMinuteOfDay;
        public int lastDeclinedDay;
        public int lastDeclinedMinuteOfDay;
        public int resolvedDay;
        public int resolvedMinuteOfDay;
        public bool hasDeliveryAssessment;
        public bool deliveredOnTime;
        public bool deliveredIntact;
        public bool deliveryWitnessed;
    }

    [Serializable]
    public sealed class SavedPlayerRelationship
    {
        public string npcId;
        public float familiarity;
        public float trust;
        public float affinity;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = SaveMigrator.CurrentVersion;
        public int money;
        public int health;
        public int day = 1;
        public int minuteOfDay;
        public int heat;
        public int missedRent;
        public int inventoryWidth;
        public int inventoryHeight;
        public string worldPlace = "Hideout";
        public float playerX;
        public float playerY;
        public List<SavedItem> items = new List<SavedItem>();
        public List<SavedItem> storageItems = new List<SavedItem>();
        public List<SavedItemKnowledge> itemKnowledge = new List<SavedItemKnowledge>();
        public List<SavedDiscoverySignal> playerDiscoveries = new List<SavedDiscoverySignal>();
        public SavedOpportunityState opportunity;
        public List<SavedPlayerRelationship> playerRelationships = new List<SavedPlayerRelationship>();
    }

    public sealed class LoadReport
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int SkippedItems { get; set; }
        public int SkippedStorageItems { get; set; }
        public int SkippedKnowledgeFacts { get; set; }
        public int SkippedDiscoverySignals { get; set; }
        public int SkippedPlayerRelationships { get; set; }
        public OpportunityState RestoredOpportunity { get; set; }
    }

    public static class SaveMigrator
    {
        public const int CurrentVersion = 7;

        /// <summary>Accepts save formats v1 through v7 and migrates them to the current inventory and player-knowledge format.</summary>
        public static bool TryMigrate(SaveData data, out string error)
        {
            error = null;
            if (data == null)
            {
                error = "No save data.";
                return false;
            }

            if (data.version < 1)
            {
                error = "Save has an invalid version (" + data.version + ").";
                return false;
            }

            if (data.version > CurrentVersion)
            {
                error = "Save was made by a newer version of the game (" + data.version + ").";
                return false;
            }

            // Version 1 did not contain a separate home store or a world location.
            if (data.version == 1)
            {
                data.worldPlace = "Hideout";
                data.storageItems = new List<SavedItem>();
            }

            // Versions 1 and 2 had no stable identity for an inventory stack.
            // Assign identities once during migration, across both containers.
            var usedIds = new HashSet<Guid>();
            EnsureInstanceIds(data.items, usedIds);
            EnsureInstanceIds(data.storageItems, usedIds);
            // Versions 1-3 had no per-stack state. Enum fields default to neutral values.
            // Version 4 introduced per-stack state but no player-specific item knowledge.
            if (data.itemKnowledge == null)
                data.itemKnowledge = new List<SavedItemKnowledge>();
            // Version 5 introduced player item knowledge. Version 6 adds discovery signals and one authored opportunity.
            if (data.playerDiscoveries == null)
                data.playerDiscoveries = new List<SavedDiscoverySignal>();
            // Version 7 adds player-to-NPC relationship impressions.
            if (data.playerRelationships == null)
                data.playerRelationships = new List<SavedPlayerRelationship>();

            data.version = CurrentVersion;

            return true;
        }

        private static void EnsureInstanceIds(List<SavedItem> items, HashSet<Guid> usedIds)
        {
            if (items == null)
                return;

            foreach (SavedItem item in items)
            {
                if (item == null)
                    continue;

                if (!Guid.TryParse(item.instanceId, out Guid id) || id == Guid.Empty || !usedIds.Add(id))
                {
                    do { id = Guid.NewGuid(); }
                    while (!usedIds.Add(id));

                    item.instanceId = id.ToString();
                }
            }
        }
    }

    public static class SaveMapper
    {
        public static SaveData Capture(PlayerStats stats, GridInventory inventory, GameClock clock, HeatMeter heat,
                                       RentLedger rent, float playerX, float playerY)
        {
            return Capture(stats, inventory, null, clock, heat, rent, "Hideout", playerX, playerY);
        }

        public static SaveData Capture(PlayerStats stats, GridInventory inventory, GridInventory storage,
                                       GameClock clock, HeatMeter heat, RentLedger rent, string worldPlace,
                                       float playerX, float playerY, ItemKnowledgeBook itemKnowledge = null,
                                       PlayerDiscoveryLedger discoveries = null, OpportunityState opportunity = null,
                                       PlayerRelationshipBook relationships = null)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));

            var data = new SaveData
            {
                money = stats.Money,
                health = stats.Health,
                day = clock.Day,
                minuteOfDay = clock.MinuteOfDay,
                heat = heat.Heat,
                missedRent = rent.MissedPayments,
                inventoryWidth = inventory.Width,
                inventoryHeight = inventory.Height,
                worldPlace = string.IsNullOrWhiteSpace(worldPlace) ? "Hideout" : worldPlace,
                playerX = playerX,
                playerY = playerY
            };

            CaptureItems(inventory, data.items);
            if (storage != null)
                CaptureItems(storage, data.storageItems);
            CaptureKnowledge(itemKnowledge, data.itemKnowledge);
            CaptureDiscoveries(discoveries, data.playerDiscoveries);
            data.opportunity = CaptureOpportunity(opportunity);
            CapturePlayerRelationships(relationships, data.playerRelationships);

            return data;
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GameClock clock,
                                       HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve)
        {
            return Apply(data, stats, inventory, null, clock, heat, rent, resolve, null);
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GridInventory storage,
                                       GameClock clock, HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve,
                                       ItemKnowledgeBook itemKnowledge = null,
                                       PlayerDiscoveryLedger discoveries = null,
                                       PlayerRelationshipBook relationships = null)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            if (!SaveMigrator.TryMigrate(data, out string error))
                return new LoadReport { Success = false, Error = error };

            stats.Restore(data.money, data.health);
            clock.Restore(data.day, data.minuteOfDay);
            heat.Restore(data.heat);
            rent.Restore(data.missedRent);

            inventory.Clear();
            int skipped = RestoreItems(data.items, inventory, resolve);

            int skippedStorage = 0;
            if (storage != null)
            {
                storage.Clear();
                skippedStorage = RestoreItems(data.storageItems, storage, resolve);
            }

            int skippedKnowledge = RestoreKnowledge(data.itemKnowledge, itemKnowledge);
            int skippedDiscoveries = RestoreDiscoveries(data.playerDiscoveries, discoveries);
            int skippedRelationships = RestorePlayerRelationships(data.playerRelationships, relationships);
            OpportunityState restoredOpportunity = RestoreOpportunity(data.opportunity);

            return new LoadReport
            {
                Success = true,
                SkippedItems = skipped,
                SkippedStorageItems = skippedStorage,
                SkippedKnowledgeFacts = skippedKnowledge,
                SkippedDiscoverySignals = skippedDiscoveries,
                SkippedPlayerRelationships = skippedRelationships,
                RestoredOpportunity = restoredOpportunity
            };
        }

        private static void CaptureItems(GridInventory inventory, List<SavedItem> destination)
        {
            foreach (var item in inventory.Items)
            {
                destination.Add(new SavedItem
                {
                    typeId = item.Type.Id.ToString(),
                    instanceId = item.ItemInstanceId.ToString(),
                    x = item.X,
                    y = item.Y,
                    rotated = item.Rotated,
                    count = item.Count,
                    quality = (int)item.State.Quality,
                    condition = (int)item.State.Condition,
                    authenticity = (int)item.State.Authenticity,
                    ownership = (int)item.State.Ownership
                });
            }
        }


        private static void CaptureKnowledge(ItemKnowledgeBook knowledge, List<SavedItemKnowledge> destination)
        {
            if (knowledge == null)
                return;

            foreach (ItemKnowledgeFact fact in knowledge.Facts)
            {
                destination.Add(new SavedItemKnowledge
                {
                    instanceId = fact.ItemInstanceId.ToString(),
                    aspect = (int)fact.Aspect,
                    value = fact.Value,
                    source = (int)fact.Source,
                    confidence = fact.Confidence,
                    learnedDay = fact.LearnedDay,
                    learnedMinuteOfDay = fact.LearnedMinuteOfDay
                });
            }
        }

        private static void CaptureDiscoveries(PlayerDiscoveryLedger discoveries,
            List<SavedDiscoverySignal> destination)
        {
            if (discoveries == null)
                return;

            foreach (PlayerDiscoverySignal signal in discoveries.Signals)
            {
                destination.Add(new SavedDiscoverySignal
                {
                    signalId = signal.SignalId,
                    subjectId = signal.SubjectId,
                    type = (int)signal.Type,
                    sourceId = signal.SourceId,
                    description = signal.Description,
                    confidence = signal.Confidence,
                    learnedDay = signal.LearnedDay,
                    learnedMinuteOfDay = signal.LearnedMinuteOfDay
                });
            }
        }

        private static int RestoreDiscoveries(List<SavedDiscoverySignal> savedSignals,
            PlayerDiscoveryLedger destination)
        {
            if (destination == null)
                return 0;

            destination.Clear();
            int skipped = 0;
            if (savedSignals == null)
                return skipped;

            foreach (SavedDiscoverySignal saved in savedSignals)
            {
                if (saved == null)
                {
                    skipped++;
                    continue;
                }

                try
                {
                    destination.Record(new PlayerDiscoverySignal(saved.signalId, saved.subjectId,
                        (DiscoverySignalType)saved.type, saved.sourceId, saved.description,
                        saved.confidence, saved.learnedDay, saved.learnedMinuteOfDay));
                }
                catch (ArgumentException)
                {
                    skipped++;
                }
            }

            return skipped;
        }

        private static void CapturePlayerRelationships(PlayerRelationshipBook relationships,
            List<SavedPlayerRelationship> destination)
        {
            if (relationships == null)
                return;

            foreach (PlayerNpcRelationshipEntry entry in relationships.Entries)
            {
                destination.Add(new SavedPlayerRelationship
                {
                    npcId = entry.NpcId,
                    familiarity = entry.Relationship.Familiarity,
                    trust = entry.Relationship.Trust,
                    affinity = entry.Relationship.Affinity
                });
            }
        }

        private static int RestorePlayerRelationships(List<SavedPlayerRelationship> savedRelationships,
            PlayerRelationshipBook destination)
        {
            if (destination == null)
                return 0;

            destination.Clear();
            int skipped = 0;
            if (savedRelationships == null)
                return skipped;

            foreach (SavedPlayerRelationship saved in savedRelationships)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.npcId) ||
                    float.IsNaN(saved.familiarity) || float.IsInfinity(saved.familiarity) ||
                    float.IsNaN(saved.trust) || float.IsInfinity(saved.trust) ||
                    float.IsNaN(saved.affinity) || float.IsInfinity(saved.affinity) ||
                    saved.familiarity < 0f || saved.familiarity > 1f ||
                    saved.trust < 0f || saved.trust > 1f ||
                    saved.affinity < -1f || saved.affinity > 1f)
                {
                    skipped++;
                    continue;
                }

                destination.Set(saved.npcId, saved.familiarity, saved.trust, saved.affinity);
            }

            return skipped;
        }

        private static SavedOpportunityState CaptureOpportunity(OpportunityState opportunity)
        {
            if (opportunity == null)
                return null;

            return new SavedOpportunityState
            {
                opportunityId = opportunity.OpportunityId,
                phase = (int)opportunity.Phase,
                resolution = (int)opportunity.Resolution,
                parcelInstanceId = opportunity.ParcelInstanceId.ToString(),
                acceptanceCount = opportunity.AcceptanceCount,
                declineCount = opportunity.DeclineCount,
                lastAcceptedDay = opportunity.LastAcceptedDay,
                lastAcceptedMinuteOfDay = opportunity.LastAcceptedMinuteOfDay,
                lastDeclinedDay = opportunity.LastDeclinedDay,
                lastDeclinedMinuteOfDay = opportunity.LastDeclinedMinuteOfDay,
                resolvedDay = opportunity.ResolvedDay,
                resolvedMinuteOfDay = opportunity.ResolvedMinuteOfDay,
                hasDeliveryAssessment = opportunity.HasDeliveryAssessment,
                deliveredOnTime = opportunity.DeliveredOnTime,
                deliveredIntact = opportunity.DeliveredIntact,
                deliveryWitnessed = opportunity.DeliveryWitnessed
            };
        }

        private static OpportunityState RestoreOpportunity(SavedOpportunityState saved)
        {
            if (saved == null)
                return null;

            Guid.TryParse(saved.parcelInstanceId, out Guid parcelId);
            return OpportunityState.TryRestore(saved.opportunityId,
                (OpportunityPhase)saved.phase, (OpportunityResolution)saved.resolution, parcelId,
                saved.acceptanceCount, saved.declineCount, saved.lastAcceptedDay,
                saved.lastAcceptedMinuteOfDay, saved.lastDeclinedDay, saved.lastDeclinedMinuteOfDay,
                saved.resolvedDay, saved.resolvedMinuteOfDay, saved.hasDeliveryAssessment,
                saved.deliveredOnTime, saved.deliveredIntact, saved.deliveryWitnessed,
                out OpportunityState restored)
                ? restored
                : null;
        }

        private static int RestoreKnowledge(List<SavedItemKnowledge> savedFacts, ItemKnowledgeBook destination)
        {
            if (destination == null)
                return 0;

            destination.Clear();
            int skipped = 0;
            if (savedFacts == null)
                return skipped;

            foreach (SavedItemKnowledge saved in savedFacts)
            {
                if (saved == null || !Guid.TryParse(saved.instanceId, out Guid instanceId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    destination.RecordFact(instanceId, (ItemKnowledgeAspect)saved.aspect, saved.value,
                        (ItemKnowledgeSource)saved.source, saved.confidence, saved.learnedDay, saved.learnedMinuteOfDay);
                }
                catch (ArgumentException)
                {
                    skipped++;
                }
            }

            return skipped;
        }

        private static int RestoreItems(List<SavedItem> savedItems, GridInventory destination, Func<Guid, ItemType?> resolve)
        {
            int skipped = 0;
            if (savedItems == null)
                return skipped;

            foreach (var saved in savedItems)
            {
                if (saved == null || !Guid.TryParse(saved.typeId, out Guid id))
                {
                    skipped++;
                    continue;
                }

                ItemType? type = resolve(id);
                ItemInstanceState state;
                try
                {
                    state = new ItemInstanceState((ItemQuality)saved.quality, (ItemCondition)saved.condition,
                        (ItemAuthenticity)saved.authenticity, (ItemOwnershipStatus)saved.ownership);
                }
                catch (ArgumentOutOfRangeException)
                {
                    skipped++;
                    continue;
                }

                if (!type.HasValue || !destination.Place(type.Value, saved.x, saved.y, saved.rotated, saved.count,
                    Guid.TryParse(saved.instanceId, out Guid instanceId) ? instanceId : Guid.Empty, state).HasValue)
                    skipped++;
            }

            return skipped;
        }
    }
}
