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
    public sealed class SavedAssetOwner
    {
        public string instanceId;
        public string actorId;
    }

    [Serializable]
    public sealed class SavedEconomicTransaction
    {
        public string transactionId;
        public string itemInstanceId;
        public string itemDefinitionId;
        public int quantity;
        public string fromActorId;
        public string toActorId;
        public int reason;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedEconomicAccount
    {
        public string actorId;
        public int balance;
    }

    [Serializable]
    public sealed class SavedRetailTransaction
    {
        public string transactionId;
        public string businessId;
        public string customerActorId;
        public string ownerActorId;
        public string itemDefinitionId;
        public int quantity;
        public int totalAmount;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedShopSaleTransaction
    {
        public string transactionId;
        public string businessId;
        public string sellerActorId;
        public string itemInstanceId;
        public string itemDefinitionId;
        public int quantity;
        public int unitPrice;
        public int totalAmount;
        public int quality;
        public int condition;
        public int authenticity;
        public int ownership;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedSearchableContainer
    {
        public string containerId;
        public bool generated;
        public bool searchStarted;
        public int discoveredCount;
        public List<string> searchOrder = new List<string>();
        public List<SavedItem> items = new List<SavedItem>();
    }

    [Serializable]
    public sealed class SavedCrimeOutcome
    {
        public string outcomeId;
        public string actionId;
        public string actorId;
        public string assetInstanceId;
        public string locationId;
        public string witnessNpcId;
        public string witnessEventId;
        public string witnessDescription;
        public float witnessX;
        public float witnessY;
        public float witnessRadius;
        public int day;
        public int minuteOfDay;
        public int heatAward;
        public int progressionAward;
    }

    [Serializable]
    public sealed class SavedIllicitExchangeTransaction
    {
        public string transactionId;
        public string itemInstanceId;
        public string itemDefinitionId;
        public int quantity;
        public string sellerActorId;
        public string buyerActorId;
        public int dirtyCashUnits;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedProductionTransaction
    {
        public string transactionId;
        public string recipeId;
        public string outputDefinitionId;
        public string operatorActorId;
        public int quantity;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedCustomerContract
    {
        public string contractId;
        public int phase;
        public int acceptedDay;
        public int acceptedMinuteOfDay;
        public int completedDay;
        public int completedMinuteOfDay;
        public string deliveryTransactionId;
        public int reward;
        public string payerActorId;
    }

    [Serializable]
    public sealed class SavedDirtyCashCleaningTransaction
    {
        public string transactionId;
        public string cashInstanceId;
        public int dirtyUnits;
        public int faceValuePerUnit;
        public int feePercent;
        public int cleanPayout;
        public int feeAmount;
        public string cleanerActorId;
        public string feeRecipientActorId;
        public int day;
        public int minuteOfDay;
    }

    [Serializable]
    public sealed class SavedNpcAgendaStageProgress
    {
        public string npcId;
        public string threadId;
        public string stageId;
        public string choiceId;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int money;
        public int cleanBalance;
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
        public List<SavedAssetOwner> assetOwners = new List<SavedAssetOwner>();
        public List<SavedEconomicTransaction> economicTransactions = new List<SavedEconomicTransaction>();
        public List<SavedEconomicAccount> residentAccounts = new List<SavedEconomicAccount>();
        public List<SavedRetailTransaction> retailTransactions = new List<SavedRetailTransaction>();
        public List<SavedShopSaleTransaction> shopSaleTransactions = new List<SavedShopSaleTransaction>();
        public List<SavedSearchableContainer> searchableContainers = new List<SavedSearchableContainer>();
        public int criminalProgressionPoints;
        public List<SavedCrimeOutcome> crimeOutcomes = new List<SavedCrimeOutcome>();
        public List<SavedItem> contactItems = new List<SavedItem>();
        public List<SavedIllicitExchangeTransaction> illicitExchangeTransactions = new List<SavedIllicitExchangeTransaction>();
        public List<SavedDirtyCashCleaningTransaction> dirtyCashCleaningTransactions = new List<SavedDirtyCashCleaningTransaction>();
        public List<SavedProductionTransaction> productionTransactions = new List<SavedProductionTransaction>();
        public List<SavedCustomerContract> customerContracts = new List<SavedCustomerContract>();
        public List<string> acquiredWorkshopUpgradeIds = new List<string>();
        public List<SavedItem> contractReceiverItems = new List<SavedItem>();
        public string contractReceiverActorId;
        public List<SavedNpcAgendaStageProgress> npcAgendaProgress = new List<SavedNpcAgendaStageProgress>();
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
        public int SkippedAssetOwners { get; set; }
        public int SkippedEconomicTransactions { get; set; }
        public int SkippedResidentAccounts { get; set; }
        public int SkippedRetailTransactions { get; set; }
        public int SkippedShopSaleTransactions { get; set; }
        public int SkippedCrimeOutcomes { get; set; }
        public int SkippedContactItems { get; set; }
        public int SkippedIllicitExchangeTransactions { get; set; }
        public int SkippedDirtyCashCleaningTransactions { get; set; }
        public int SkippedProductionTransactions { get; set; }
        public int SkippedCustomerContracts { get; set; }
        public int SkippedWorkshopUpgradeIds { get; set; }
        public int SkippedContractReceiverItems { get; set; }
        public int SkippedNpcAgendaProgress { get; set; }
        public OpportunityState RestoredOpportunity { get; set; }
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
                                       PlayerRelationshipBook relationships = null,
                                       AssetOwnershipBook assetOwnership = null,
                                       EconomicTransactionLedger economicTransactions = null,
                                       EconomicAccountBook economicAccounts = null,
                                       RetailTransactionLedger retailTransactions = null,
                                       ShopSaleTransactionLedger shopSaleTransactions = null,
                                       CriminalProgressionTrack criminalProgression = null,
                                       CrimeOutcomeLedger crimeOutcomes = null,
                                       GridInventory contactInventory = null,
                                       IllicitExchangeTransactionLedger illicitExchanges = null,
                                       DirtyCashCleaningTransactionLedger dirtyCashCleanings = null,
                                       ProductionTransactionLedger productionTransactions = null,
                                       CustomerContractBook customerContracts = null,
                                       WorkshopProgressionBook workshopProgression = null,
                                       GridInventory contractReceiverInventory = null,
                                       string contractReceiverActorId = null,
                                       NpcAgendaProgressBook npcAgendaProgress = null)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));

            var data = new SaveData
            {
                money = stats.Money,
                cleanBalance = stats.CleanMoney.Balance,
                health = stats.Health,
                day = clock.Day,
                minuteOfDay = clock.MinuteOfDay,
                heat = heat.Heat,
                missedRent = rent.MissedPayments,
                inventoryWidth = inventory.Width,
                inventoryHeight = inventory.Height,
                worldPlace = string.IsNullOrWhiteSpace(worldPlace) ? "Hideout" : worldPlace,
                playerX = playerX,
                playerY = playerY,
                criminalProgressionPoints = criminalProgression != null ? criminalProgression.Points : 0,
                contractReceiverActorId = contractReceiverActorId
            };

            CaptureItems(inventory, data.items);
            if (storage != null)
                CaptureItems(storage, data.storageItems);
            CaptureKnowledge(itemKnowledge, data.itemKnowledge);
            CaptureDiscoveries(discoveries, data.playerDiscoveries);
            data.opportunity = CaptureOpportunity(opportunity);
            CapturePlayerRelationships(relationships, data.playerRelationships);
            if (contactInventory != null) CaptureItems(contactInventory, data.contactItems);
            if (contractReceiverInventory != null) CaptureItems(contractReceiverInventory, data.contractReceiverItems);
            CaptureEconomicState(inventory, storage, contactInventory, contractReceiverInventory,
                contractReceiverActorId, assetOwnership, economicTransactions, data);
            CaptureResidentAccounts(economicAccounts, data.residentAccounts);
            CaptureRetailTransactions(retailTransactions, data.retailTransactions);
            CaptureShopSaleTransactions(shopSaleTransactions, data.shopSaleTransactions);
            CaptureCrimeOutcomes(crimeOutcomes, data.crimeOutcomes);
            CaptureIllicitExchanges(illicitExchanges, data.illicitExchangeTransactions);
            CaptureDirtyCashCleanings(dirtyCashCleanings, data.dirtyCashCleaningTransactions);
            CaptureProductionTransactions(productionTransactions, data.productionTransactions);
            CaptureCustomerContracts(customerContracts, data.customerContracts);
            CaptureWorkshopProgression(workshopProgression, data.acquiredWorkshopUpgradeIds);
            CaptureNpcAgendaProgress(npcAgendaProgress, data.npcAgendaProgress);

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
                                       PlayerRelationshipBook relationships = null,
                                       AssetOwnershipBook assetOwnership = null,
                                       EconomicTransactionLedger economicTransactions = null,
                                       EconomicAccountBook economicAccounts = null,
                                       RetailTransactionLedger retailTransactions = null,
                                       ShopSaleTransactionLedger shopSaleTransactions = null,
                                       CriminalProgressionTrack criminalProgression = null,
                                       CrimeOutcomeLedger crimeOutcomes = null,
                                       GridInventory contactInventory = null,
                                       IllicitExchangeTransactionLedger illicitExchanges = null,
                                       DirtyCashCleaningTransactionLedger dirtyCashCleanings = null,
                                       ProductionTransactionLedger productionTransactions = null,
                                       CustomerContractBook customerContracts = null,
                                       WorkshopProgressionBook workshopProgression = null,
                                       GridInventory contractReceiverInventory = null,
                                       NpcAgendaProgressBook npcAgendaProgress = null)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            if (data == null)
                return new LoadReport { Success = false, Error = "No save data." };

            stats.Restore(data.cleanBalance, data.health);
            clock.Restore(data.day, data.minuteOfDay);
            heat.Restore(data.heat);
            rent.Restore(data.missedRent);
            if (criminalProgression != null)
                criminalProgression.Restore(data.criminalProgressionPoints);

            inventory.Clear();
            int skipped = RestoreItems(data.items, inventory, resolve);

            int skippedStorage = 0;
            if (storage != null)
            {
                storage.Clear();
                skippedStorage = RestoreItems(data.storageItems, storage, resolve);
            }
            int skippedContact = 0;
            if (contactInventory != null)
            {
                contactInventory.Clear();
                skippedContact = RestoreItems(data.contactItems, contactInventory, resolve);
            }
            int skippedContractReceiver = 0;
            if (contractReceiverInventory != null)
            {
                contractReceiverInventory.Clear();
                skippedContractReceiver = RestoreItems(data.contractReceiverItems, contractReceiverInventory, resolve);
            }

            int skippedKnowledge = RestoreKnowledge(data.itemKnowledge, itemKnowledge);
            int skippedDiscoveries = RestoreDiscoveries(data.playerDiscoveries, discoveries);
            int skippedRelationships = RestorePlayerRelationships(data.playerRelationships, relationships);
            int skippedOwners = RestoreAssetOwners(data.assetOwners, inventory, storage, contactInventory,
                contractReceiverInventory, data.contractReceiverActorId, assetOwnership);
            int skippedTransactions = RestoreEconomicTransactions(data.economicTransactions, economicTransactions);
            int skippedAccounts = RestoreResidentAccounts(data.residentAccounts, economicAccounts);
            int skippedRetail = RestoreRetailTransactions(data.retailTransactions, retailTransactions);
            int skippedShopSales = RestoreShopSaleTransactions(data.shopSaleTransactions, shopSaleTransactions);
            int skippedCrimeOutcomes = RestoreCrimeOutcomes(data.crimeOutcomes, crimeOutcomes);
            int skippedIllicitExchanges = RestoreIllicitExchanges(data.illicitExchangeTransactions, illicitExchanges);
            int skippedDirtyCashCleanings = RestoreDirtyCashCleanings(data.dirtyCashCleaningTransactions, dirtyCashCleanings);
            int skippedProduction = RestoreProductionTransactions(data.productionTransactions, productionTransactions);
            int skippedContracts = RestoreCustomerContracts(data.customerContracts, customerContracts);
            int skippedUpgrades = RestoreWorkshopProgression(data.acquiredWorkshopUpgradeIds, workshopProgression);
            int skippedAgendaProgress = RestoreNpcAgendaProgress(data.npcAgendaProgress, npcAgendaProgress);
            OpportunityState restoredOpportunity = RestoreOpportunity(data.opportunity);

            return new LoadReport
            {
                Success = true,
                SkippedItems = skipped,
                SkippedStorageItems = skippedStorage,
                SkippedKnowledgeFacts = skippedKnowledge,
                SkippedDiscoverySignals = skippedDiscoveries,
                SkippedPlayerRelationships = skippedRelationships,
                SkippedAssetOwners = skippedOwners,
                SkippedEconomicTransactions = skippedTransactions,
                SkippedResidentAccounts = skippedAccounts,
                SkippedRetailTransactions = skippedRetail,
                SkippedShopSaleTransactions = skippedShopSales,
                SkippedCrimeOutcomes = skippedCrimeOutcomes,
                SkippedContactItems = skippedContact,
                SkippedIllicitExchangeTransactions = skippedIllicitExchanges,
                SkippedDirtyCashCleaningTransactions = skippedDirtyCashCleanings,
                SkippedProductionTransactions = skippedProduction,
                SkippedCustomerContracts = skippedContracts,
                SkippedWorkshopUpgradeIds = skippedUpgrades,
                SkippedContractReceiverItems = skippedContractReceiver,
                SkippedNpcAgendaProgress = skippedAgendaProgress,
                RestoredOpportunity = restoredOpportunity
            };
        }



        private static void CaptureNpcAgendaProgress(NpcAgendaProgressBook book,
            List<SavedNpcAgendaStageProgress> destination)
        {
            if (book == null || destination == null) return;
            foreach (NpcAgendaStageProgress progress in book.CompletedStages)
                destination.Add(new SavedNpcAgendaStageProgress
                {
                    npcId = progress.NpcId,
                    threadId = progress.ThreadId,
                    stageId = progress.StageId,
                    choiceId = progress.ChoiceId
                });
        }

        private static int RestoreNpcAgendaProgress(List<SavedNpcAgendaStageProgress> saved,
            NpcAgendaProgressBook destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (SavedNpcAgendaStageProgress entry in saved)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.npcId) ||
                    string.IsNullOrWhiteSpace(entry.threadId) ||
                    string.IsNullOrWhiteSpace(entry.stageId) ||
                    string.IsNullOrWhiteSpace(entry.choiceId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    if (!destination.TryRestore(new NpcAgendaStageProgress(
                        entry.npcId, entry.threadId, entry.stageId, entry.choiceId)))
                        skipped++;
                }
                catch (ArgumentException)
                {
                    skipped++;
                }
            }
            return skipped;
        }

        private static void CaptureIllicitExchanges(IllicitExchangeTransactionLedger ledger,
            List<SavedIllicitExchangeTransaction> destination)
        {
            if (ledger == null) return;
            foreach (IllicitExchangeTransaction transaction in ledger.Transactions)
                destination.Add(new SavedIllicitExchangeTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    itemInstanceId = transaction.ItemInstanceId.ToString(),
                    itemDefinitionId = transaction.ItemDefinitionId.ToString(),
                    quantity = transaction.Quantity,
                    sellerActorId = transaction.Seller.Value,
                    buyerActorId = transaction.Buyer.Value,
                    dirtyCashUnits = transaction.DirtyCashUnits,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static int RestoreIllicitExchanges(List<SavedIllicitExchangeTransaction> saved,
            IllicitExchangeTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (SavedIllicitExchangeTransaction entry in saved)
            {
                if (entry == null || !Guid.TryParse(entry.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(entry.itemInstanceId, out Guid itemId) ||
                    !Guid.TryParse(entry.itemDefinitionId, out Guid definitionId) ||
                    string.IsNullOrWhiteSpace(entry.sellerActorId) || string.IsNullOrWhiteSpace(entry.buyerActorId))
                { skipped++; continue; }
                try
                {
                    var transaction = new IllicitExchangeTransaction(transactionId, itemId, definitionId,
                        entry.quantity, new EconomicActorId(entry.sellerActorId), new EconomicActorId(entry.buyerActorId),
                        entry.dirtyCashUnits, entry.day, entry.minuteOfDay);
                    if (!destination.TryRecord(transaction)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureDirtyCashCleanings(DirtyCashCleaningTransactionLedger ledger,
            List<SavedDirtyCashCleaningTransaction> destination)
        {
            if (ledger == null) return;
            foreach (DirtyCashCleaningTransaction transaction in ledger.Transactions)
                destination.Add(new SavedDirtyCashCleaningTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    cashInstanceId = transaction.CashInstanceId.ToString(),
                    dirtyUnits = transaction.DirtyUnits,
                    faceValuePerUnit = transaction.FaceValuePerUnit,
                    feePercent = transaction.FeePercent,
                    cleanPayout = transaction.CleanPayout,
                    feeAmount = transaction.FeeAmount,
                    cleanerActorId = transaction.Cleaner.Value,
                    feeRecipientActorId = transaction.FeeRecipient.Value,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static int RestoreDirtyCashCleanings(List<SavedDirtyCashCleaningTransaction> saved,
            DirtyCashCleaningTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (SavedDirtyCashCleaningTransaction entry in saved)
            {
                if (entry == null || !Guid.TryParse(entry.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(entry.cashInstanceId, out Guid cashId) ||
                    string.IsNullOrWhiteSpace(entry.cleanerActorId) ||
                    string.IsNullOrWhiteSpace(entry.feeRecipientActorId))
                { skipped++; continue; }
                try
                {
                    var transaction = new DirtyCashCleaningTransaction(transactionId, cashId, entry.dirtyUnits,
                        entry.faceValuePerUnit, entry.feePercent, entry.cleanPayout, entry.feeAmount,
                        new EconomicActorId(entry.cleanerActorId), new EconomicActorId(entry.feeRecipientActorId),
                        entry.day, entry.minuteOfDay);
                    if (!destination.TryRecord(transaction)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureCrimeOutcomes(CrimeOutcomeLedger ledger, List<SavedCrimeOutcome> destination)
        {
            if (ledger == null) return;
            foreach (CrimeOutcome outcome in ledger.Outcomes)
                destination.Add(new SavedCrimeOutcome
                {
                    outcomeId = outcome.OutcomeId.ToString(),
                    actionId = outcome.ActionId,
                    actorId = outcome.ActorId,
                    assetInstanceId = outcome.AssetInstanceId.ToString(),
                    locationId = outcome.LocationId,
                    witnessNpcId = outcome.WitnessNpcId,
                    witnessEventId = outcome.WitnessEventId,
                    witnessDescription = outcome.WitnessDescription,
                    witnessX = outcome.WitnessX,
                    witnessY = outcome.WitnessY,
                    witnessRadius = outcome.WitnessRadius,
                    day = outcome.Day,
                    minuteOfDay = outcome.MinuteOfDay,
                    heatAward = outcome.HeatAward,
                    progressionAward = outcome.ProgressionAward
                });
        }

        private static int RestoreCrimeOutcomes(List<SavedCrimeOutcome> savedOutcomes, CrimeOutcomeLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (savedOutcomes == null) return skipped;
            foreach (SavedCrimeOutcome saved in savedOutcomes)
            {
                if (saved == null || !Guid.TryParse(saved.outcomeId, out Guid outcomeId) ||
                    !Guid.TryParse(saved.assetInstanceId, out Guid assetInstanceId) ||
                    string.IsNullOrWhiteSpace(saved.actionId) || string.IsNullOrWhiteSpace(saved.actorId) ||
                    string.IsNullOrWhiteSpace(saved.locationId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var outcome = new CrimeOutcome(outcomeId, saved.actionId, saved.actorId, assetInstanceId,
                        saved.locationId, saved.witnessNpcId, saved.witnessEventId,
                        saved.witnessDescription, saved.witnessX, saved.witnessY, saved.witnessRadius,
                        saved.day, saved.minuteOfDay, saved.heatAward, saved.progressionAward);
                    if (!destination.TryRecord(outcome)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureResidentAccounts(EconomicAccountBook accounts, List<SavedEconomicAccount> destination)
        {
            if (accounts == null) return;
            foreach (KeyValuePair<EconomicActorId, CleanMoneyAccount> entry in accounts.Accounts)
            {
                // The player's account already has its canonical cleanBalance save field.
                if (entry.Key == EconomicActorId.Player || entry.Value == null) continue;
                destination.Add(new SavedEconomicAccount { actorId = entry.Key.Value, balance = entry.Value.Balance });
            }
        }

        private static int RestoreResidentAccounts(List<SavedEconomicAccount> savedAccounts, EconomicAccountBook destination)
        {
            if (destination == null) return 0;
            destination.ResetNonPlayerAccountsToInitialBalances();
            int skipped = 0;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (savedAccounts == null) return skipped;
            foreach (SavedEconomicAccount saved in savedAccounts)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.actorId) ||
                    string.Equals(saved.actorId, EconomicActorId.Player.Value, StringComparison.Ordinal) ||
                    saved.balance < 0 || !seen.Add(saved.actorId))
                { skipped++; continue; }
                try
                {
                    if (!destination.RestoreBalance(new EconomicActorId(saved.actorId), saved.balance))
                        skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureRetailTransactions(RetailTransactionLedger ledger, List<SavedRetailTransaction> destination)
        {
            if (ledger == null) return;
            foreach (RetailTransaction transaction in ledger.Transactions)
                destination.Add(new SavedRetailTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    businessId = transaction.BusinessId,
                    customerActorId = transaction.Customer.Value,
                    ownerActorId = transaction.Owner.Value,
                    itemDefinitionId = transaction.ItemDefinitionId.ToString(),
                    quantity = transaction.Quantity,
                    totalAmount = transaction.TotalAmount,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static int RestoreRetailTransactions(List<SavedRetailTransaction> savedTransactions, RetailTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (savedTransactions == null) return skipped;
            foreach (SavedRetailTransaction saved in savedTransactions)
            {
                if (saved == null || !Guid.TryParse(saved.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(saved.itemDefinitionId, out Guid itemDefinitionId) ||
                    string.IsNullOrWhiteSpace(saved.businessId) ||
                    string.IsNullOrWhiteSpace(saved.customerActorId) || string.IsNullOrWhiteSpace(saved.ownerActorId))
                { skipped++; continue; }
                try
                {
                    var transaction = new RetailTransaction(transactionId, saved.businessId,
                        new EconomicActorId(saved.customerActorId), new EconomicActorId(saved.ownerActorId),
                        itemDefinitionId, saved.quantity, saved.totalAmount, saved.day, saved.minuteOfDay);
                    if (!destination.TryRecord(transaction)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureShopSaleTransactions(ShopSaleTransactionLedger ledger,
            List<SavedShopSaleTransaction> destination)
        {
            if (ledger == null) return;
            foreach (ShopSaleTransaction transaction in ledger.Transactions)
                destination.Add(new SavedShopSaleTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    businessId = transaction.BusinessId,
                    sellerActorId = transaction.Seller.Value,
                    itemInstanceId = transaction.ItemInstanceId.ToString(),
                    itemDefinitionId = transaction.ItemDefinitionId.ToString(),
                    quantity = transaction.Quantity,
                    unitPrice = transaction.UnitPrice,
                    totalAmount = transaction.TotalAmount,
                    quality = (int)transaction.ItemState.Quality,
                    condition = (int)transaction.ItemState.Condition,
                    authenticity = (int)transaction.ItemState.Authenticity,
                    ownership = (int)transaction.ItemState.Ownership,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static int RestoreShopSaleTransactions(List<SavedShopSaleTransaction> savedTransactions,
            ShopSaleTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (savedTransactions == null) return skipped;
            foreach (SavedShopSaleTransaction saved in savedTransactions)
            {
                if (saved == null || !Guid.TryParse(saved.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(saved.itemInstanceId, out Guid itemInstanceId) ||
                    !Guid.TryParse(saved.itemDefinitionId, out Guid itemDefinitionId) ||
                    string.IsNullOrWhiteSpace(saved.businessId) || string.IsNullOrWhiteSpace(saved.sellerActorId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var state = new ItemInstanceState((ItemQuality)saved.quality, (ItemCondition)saved.condition,
                        (ItemAuthenticity)saved.authenticity, (ItemOwnershipStatus)saved.ownership);
                    var transaction = new ShopSaleTransaction(transactionId, saved.businessId,
                        new EconomicActorId(saved.sellerActorId), itemInstanceId, itemDefinitionId,
                        saved.quantity, saved.unitPrice, saved.totalAmount, state, saved.day, saved.minuteOfDay);
                    if (!destination.TryRecord(transaction)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureProductionTransactions(ProductionTransactionLedger ledger,
            List<SavedProductionTransaction> destination)
        {
            if (ledger == null) return;
            foreach (ProductionTransaction transaction in ledger.Transactions)
                destination.Add(new SavedProductionTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    recipeId = transaction.RecipeId.ToString(),
                    outputDefinitionId = transaction.OutputDefinitionId.ToString(),
                    operatorActorId = transaction.OperatorActor.Value,
                    quantity = transaction.Quantity,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static int RestoreProductionTransactions(List<SavedProductionTransaction> saved,
            ProductionTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (SavedProductionTransaction entry in saved)
            {
                if (entry == null || !Guid.TryParse(entry.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(entry.recipeId, out Guid recipeId) ||
                    !Guid.TryParse(entry.outputDefinitionId, out Guid outputId) ||
                    string.IsNullOrWhiteSpace(entry.operatorActorId))
                { skipped++; continue; }
                try
                {
                    if (!destination.TryRecord(new ProductionTransaction(transactionId, recipeId,
                        outputId, entry.quantity, new EconomicActorId(entry.operatorActorId),
                        entry.day, entry.minuteOfDay))) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
        }

        private static void CaptureCustomerContracts(CustomerContractBook contracts,
            List<SavedCustomerContract> destination)
        {
            if (contracts == null) return;
            foreach (CustomerContractState state in contracts.States)
                destination.Add(new SavedCustomerContract
                {
                    contractId = state.ContractId.ToString(),
                    phase = (int)state.Phase,
                    acceptedDay = state.AcceptedDay,
                    acceptedMinuteOfDay = state.AcceptedMinuteOfDay,
                    completedDay = state.CompletedDay,
                    completedMinuteOfDay = state.CompletedMinuteOfDay,
                    deliveryTransactionId = state.DeliveryTransactionId.ToString(),
                    reward = state.Reward,
                    payerActorId = state.Payer.Value
                });
        }

        private static int RestoreCustomerContracts(List<SavedCustomerContract> saved,
            CustomerContractBook destination)
        {
            if (destination == null) return 0;
            destination.ResetToOffers();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (SavedCustomerContract entry in saved)
            {
                if (entry == null || !Guid.TryParse(entry.contractId, out Guid contractId))
                { skipped++; continue; }
                Guid.TryParse(entry.deliveryTransactionId, out Guid deliveryId);
                EconomicActorId payer = string.IsNullOrWhiteSpace(entry.payerActorId)
                    ? default : new EconomicActorId(entry.payerActorId);
                if (!destination.RestoreState(contractId, (CustomerContractPhase)entry.phase,
                    entry.acceptedDay, entry.acceptedMinuteOfDay, entry.completedDay,
                    entry.completedMinuteOfDay, deliveryId, entry.reward, payer)) skipped++;
            }
            return skipped;
        }

        private static void CaptureWorkshopProgression(WorkshopProgressionBook progression,
            List<string> destination)
        {
            if (progression == null) return;
            foreach (Guid id in progression.AcquiredUpgradeIds)
                destination.Add(id.ToString());
        }

        private static int RestoreWorkshopProgression(List<string> saved, WorkshopProgressionBook destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (saved == null) return skipped;
            foreach (string value in saved)
            {
                if (!Guid.TryParse(value, out Guid id) || id == Guid.Empty || !destination.TryAcquire(id))
                    skipped++;
            }
            return skipped;
        }

        private static void CaptureEconomicState(GridInventory inventory, GridInventory storage, GridInventory contactInventory,
            GridInventory contractReceiverInventory, string contractReceiverActorId,
            AssetOwnershipBook ownership, EconomicTransactionLedger ledger, SaveData destination)
        {
            if (ownership != null)
            {
                CaptureAssetOwners(inventory, ownership, destination.assetOwners, EconomicActorId.Player);
                if (storage != null) CaptureAssetOwners(storage, ownership, destination.assetOwners, EconomicActorId.Player);
                if (contactInventory != null)
                    CaptureAssetOwners(contactInventory, ownership, destination.assetOwners,
                        new EconomicActorId("resident:Clean_Dirtycash"));
                if (contractReceiverInventory != null && !string.IsNullOrWhiteSpace(contractReceiverActorId))
                    CaptureAssetOwners(contractReceiverInventory, ownership, destination.assetOwners,
                        new EconomicActorId(contractReceiverActorId));
            }
            if (ledger == null) return;
            foreach (EconomicTransaction transaction in ledger.Transactions)
                destination.economicTransactions.Add(new SavedEconomicTransaction
                {
                    transactionId = transaction.TransactionId.ToString(),
                    itemInstanceId = transaction.ItemInstanceId.ToString(),
                    itemDefinitionId = transaction.ItemDefinitionId.ToString(),
                    quantity = transaction.Quantity,
                    fromActorId = transaction.FromActor.Value,
                    toActorId = transaction.ToActor.Value,
                    reason = (int)transaction.Reason,
                    day = transaction.Day,
                    minuteOfDay = transaction.MinuteOfDay
                });
        }

        private static void CaptureAssetOwners(GridInventory inventory, AssetOwnershipBook ownership,
            List<SavedAssetOwner> destination, EconomicActorId fallbackOwner)
        {
            foreach (PlacedItem item in inventory.Items)
            {
                EconomicActorId owner = ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId registered)
                    ? registered : fallbackOwner;
                destination.Add(new SavedAssetOwner { instanceId = item.ItemInstanceId.ToString(), actorId = owner.Value });
            }
        }

        private static int RestoreAssetOwners(List<SavedAssetOwner> savedOwners, GridInventory inventory,
            GridInventory storage, GridInventory contactInventory, GridInventory contractReceiverInventory,
            string contractReceiverActorId, AssetOwnershipBook destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (savedOwners != null)
                foreach (SavedAssetOwner saved in savedOwners)
                {
                    if (saved == null || !Guid.TryParse(saved.instanceId, out Guid id) ||
                        id == Guid.Empty || string.IsNullOrWhiteSpace(saved.actorId))
                    { skipped++; continue; }
                    try
                    {
                        if (!destination.TryRegister(id, new EconomicActorId(saved.actorId))) skipped++;
                    }
                    catch (ArgumentException) { skipped++; }
                }
            RegisterMissingOwners(inventory, destination, EconomicActorId.Player);
            if (storage != null) RegisterMissingOwners(storage, destination, EconomicActorId.Player);
            if (contactInventory != null)
                RegisterMissingOwners(contactInventory, destination, new EconomicActorId("resident:Clean_Dirtycash"));
            if (contractReceiverInventory != null && !string.IsNullOrWhiteSpace(contractReceiverActorId))
                RegisterMissingOwners(contractReceiverInventory, destination, new EconomicActorId(contractReceiverActorId));
            return skipped;
        }

        private static void RegisterMissingOwners(GridInventory inventory, AssetOwnershipBook ownership,
            EconomicActorId fallbackOwner)
        {
            foreach (PlacedItem item in inventory.Items)
                if (!ownership.TryGetOwner(item.ItemInstanceId, out _))
                    ownership.TryRegister(item.ItemInstanceId, fallbackOwner);
        }

        private static int RestoreEconomicTransactions(List<SavedEconomicTransaction> savedTransactions,
            EconomicTransactionLedger destination)
        {
            if (destination == null) return 0;
            destination.Clear();
            int skipped = 0;
            if (savedTransactions == null) return skipped;
            foreach (SavedEconomicTransaction saved in savedTransactions)
            {
                if (saved == null || !Guid.TryParse(saved.transactionId, out Guid transactionId) ||
                    !Guid.TryParse(saved.itemInstanceId, out Guid itemId) ||
                    !Guid.TryParse(saved.itemDefinitionId, out Guid definitionId))
                { skipped++; continue; }
                try
                {
                    var transaction = new EconomicTransaction(transactionId, itemId, definitionId, saved.quantity,
                        new EconomicActorId(saved.fromActorId), new EconomicActorId(saved.toActorId),
                        (AssetTransferReason)saved.reason, saved.day, saved.minuteOfDay);
                    if (!destination.TryRecord(transaction)) skipped++;
                }
                catch (ArgumentException) { skipped++; }
            }
            return skipped;
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
