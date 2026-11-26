using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum CustomerContractPhase
    {
        Offered = 0,
        Active = 1,
        Completed = 2
    }

    public sealed class CustomerContractState
    {
        internal CustomerContractState(Guid contractId)
        {
            ContractId = contractId;
            Phase = CustomerContractPhase.Offered;
        }

        internal CustomerContractState(Guid contractId, CustomerContractPhase phase, int acceptedDay,
            int acceptedMinute, int completedDay, int completedMinute, Guid deliveryTransactionId,
            int reward, EconomicActorId payer)
        {
            ContractId = contractId;
            Phase = phase;
            AcceptedDay = acceptedDay;
            AcceptedMinuteOfDay = acceptedMinute;
            CompletedDay = completedDay;
            CompletedMinuteOfDay = completedMinute;
            DeliveryTransactionId = deliveryTransactionId;
            Reward = reward;
            Payer = payer;
        }

        public Guid ContractId { get; }
        public CustomerContractPhase Phase { get; internal set; }
        public int AcceptedDay { get; internal set; }
        public int AcceptedMinuteOfDay { get; internal set; }
        public int CompletedDay { get; internal set; }
        public int CompletedMinuteOfDay { get; internal set; }
        public Guid DeliveryTransactionId { get; internal set; }
        public int Reward { get; internal set; }
        public EconomicActorId Payer { get; internal set; }
    }

    /// <summary>Tracks authored customer offers and enforces a single accepted contract globally.</summary>
    public sealed class CustomerContractBook
    {
        private readonly Dictionary<Guid, CustomerContractState> _states =
            new Dictionary<Guid, CustomerContractState>();

        public int Count => _states.Count;
        public Guid ActiveContractId { get; private set; }
        public IEnumerable<CustomerContractState> States => _states.Values;
        public bool HasActiveContract => ActiveContractId != Guid.Empty;

        public bool TryRegisterOffer(Guid contractId)
        {
            if (contractId == Guid.Empty || _states.ContainsKey(contractId))
                return false;
            _states.Add(contractId, new CustomerContractState(contractId));
            return true;
        }

        public bool TryGetState(Guid contractId, out CustomerContractState state)
        {
            return _states.TryGetValue(contractId, out state);
        }

        public bool TryAccept(Guid contractId, int day, int minuteOfDay)
        {
            if (!ValidTime(day, minuteOfDay) || HasActiveContract ||
                !_states.TryGetValue(contractId, out CustomerContractState state) ||
                state.Phase != CustomerContractPhase.Offered)
                return false;

            state.Phase = CustomerContractPhase.Active;
            state.AcceptedDay = day;
            state.AcceptedMinuteOfDay = minuteOfDay;
            ActiveContractId = contractId;
            return true;
        }

        public bool TryComplete(Guid contractId, Guid deliveryTransactionId, int reward,
            EconomicActorId payer, int day, int minuteOfDay)
        {
            if (!ValidTime(day, minuteOfDay) || deliveryTransactionId == Guid.Empty ||
                reward < 1 || !payer.IsValid || ActiveContractId != contractId ||
                !_states.TryGetValue(contractId, out CustomerContractState state) ||
                state.Phase != CustomerContractPhase.Active)
                return false;

            state.Phase = CustomerContractPhase.Completed;
            state.CompletedDay = day;
            state.CompletedMinuteOfDay = minuteOfDay;
            state.DeliveryTransactionId = deliveryTransactionId;
            state.Reward = reward;
            state.Payer = payer;
            ActiveContractId = Guid.Empty;
            return true;
        }

        public bool RestoreState(Guid contractId, CustomerContractPhase phase, int acceptedDay,
            int acceptedMinuteOfDay, int completedDay, int completedMinuteOfDay,
            Guid deliveryTransactionId, int reward, EconomicActorId payer)
        {
            if (contractId == Guid.Empty || !_states.ContainsKey(contractId) ||
                !Enum.IsDefined(typeof(CustomerContractPhase), phase) ||
                (phase == CustomerContractPhase.Active && HasActiveContract))
                return false;

            bool accepted = phase != CustomerContractPhase.Offered;
            bool completed = phase == CustomerContractPhase.Completed;
            if (accepted && !ValidTime(acceptedDay, acceptedMinuteOfDay))
                return false;
            if (!accepted && (acceptedDay != 0 || acceptedMinuteOfDay != 0))
                return false;
            if (completed)
            {
                if (!ValidTime(completedDay, completedMinuteOfDay) ||
                    completedDay < acceptedDay ||
                    (completedDay == acceptedDay && completedMinuteOfDay < acceptedMinuteOfDay) ||
                    deliveryTransactionId == Guid.Empty || reward < 1 || !payer.IsValid)
                    return false;
            }
            else if (completedDay != 0 || completedMinuteOfDay != 0 ||
                deliveryTransactionId != Guid.Empty || reward != 0 || payer.IsValid)
                return false;

            CustomerContractState state = _states[contractId];
            state.Phase = phase;
            state.AcceptedDay = acceptedDay;
            state.AcceptedMinuteOfDay = acceptedMinuteOfDay;
            state.CompletedDay = completedDay;
            state.CompletedMinuteOfDay = completedMinuteOfDay;
            state.DeliveryTransactionId = deliveryTransactionId;
            state.Reward = reward;
            state.Payer = payer;

            if (phase == CustomerContractPhase.Active)
                ActiveContractId = contractId;
            return true;
        }

        public void ResetToOffers()
        {
            foreach (CustomerContractState state in _states.Values)
            {
                state.Phase = CustomerContractPhase.Offered;
                state.AcceptedDay = 0;
                state.AcceptedMinuteOfDay = 0;
                state.CompletedDay = 0;
                state.CompletedMinuteOfDay = 0;
                state.DeliveryTransactionId = Guid.Empty;
                state.Reward = 0;
                state.Payer = default;
            }
            ActiveContractId = Guid.Empty;
        }

        private static bool ValidTime(int day, int minuteOfDay)
        {
            return day >= 1 && minuteOfDay >= 0 && minuteOfDay < 1440;
        }

    }

    public enum CustomerContractFulfilmentStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        ContractNotActive = 2,
        ItemNotFound = 3,
        WrongItem = 4,
        WrongQuantity = 5,
        OwnershipMismatch = 6,
        StolenGoodsNotAccepted = 7,
        InsufficientFunds = 8,
        AccountOverflow = 9,
        ReceiverFull = 10,
        TransferFailed = 11,
        ContractStateConflict = 12
    }

    public readonly struct CustomerContractFulfilmentResult
    {
        internal CustomerContractFulfilmentResult(CustomerContractFulfilmentStatus status,
            EconomicTransaction delivery, int reward)
        {
            Status = status;
            Delivery = delivery;
            Reward = reward;
        }

        public CustomerContractFulfilmentStatus Status { get; }
        public EconomicTransaction Delivery { get; }
        public int Reward { get; }
        public bool Success => Status == CustomerContractFulfilmentStatus.Completed;
    }

    /// <summary>Moves the exact contracted stack to the customer and pays the player atomically.</summary>
    public static class CustomerContractService
    {
        public static CustomerContractFulfilmentResult TryFulfil(Guid contractId, Guid itemInstanceId,
            ItemType requiredItem, int requiredQuantity, bool acceptStolenGoods, int reward,
            EconomicActorId receiverActor, EconomicActorId payerActor, GridInventory playerInventory,
            GridInventory receiverInventory, AssetOwnershipBook ownership, AssetTransferService transfers,
            EconomicTransactionLedger economicTransactions, CleanMoneyAccount playerAccount,
            CleanMoneyAccount payerAccount, CustomerContractBook contracts, int day, int minuteOfDay,
            PlayerRelationshipBook relationships = null, string relationshipNpcId = null,
            float familiarityDelta = 0f, float trustDelta = 0f, float affinityDelta = 0f,
            NpcSocialWorld npcSocialWorld = null, string npcOpinionNpcId = null,
            float npcFamiliarityDelta = 0f, float npcTrustDelta = 0f, float npcAffinityDelta = 0f)
        {
            if (contractId == Guid.Empty || itemInstanceId == Guid.Empty || requiredQuantity < 1 ||
                reward < 1 || !receiverActor.IsValid || !payerActor.IsValid ||
                receiverActor != payerActor || payerActor == EconomicActorId.Player ||
                requiredItem.MaxStack < 1 || playerInventory == null || receiverInventory == null ||
                ownership == null || transfers == null || economicTransactions == null ||
                playerAccount == null || payerAccount == null || ReferenceEquals(playerAccount, payerAccount) ||
                contracts == null || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440 ||
                !ValidRelationshipDelta(familiarityDelta) || !ValidRelationshipDelta(trustDelta) ||
                !ValidRelationshipDelta(affinityDelta) ||
                !ValidRelationshipDelta(npcFamiliarityDelta) || !ValidRelationshipDelta(npcTrustDelta) ||
                !ValidRelationshipDelta(npcAffinityDelta) ||
                ((npcFamiliarityDelta != 0f || npcTrustDelta != 0f || npcAffinityDelta != 0f) &&
                 (npcSocialWorld == null || string.IsNullOrWhiteSpace(npcOpinionNpcId) ||
                  payerActor.Value != "resident:" + npcOpinionNpcId ||
                  !npcSocialWorld.TryGetNpc(npcOpinionNpcId, out NpcAgentState _))) ||
                ((familiarityDelta != 0f || trustDelta != 0f || affinityDelta != 0f) &&
                 (relationships == null || string.IsNullOrWhiteSpace(relationshipNpcId) ||
                  payerActor.Value != "resident:" + relationshipNpcId)))
                return Result(CustomerContractFulfilmentStatus.InvalidRequest);

            if (!contracts.TryGetState(contractId, out CustomerContractState contract) ||
                contract.Phase != CustomerContractPhase.Active || contracts.ActiveContractId != contractId)
                return Result(CustomerContractFulfilmentStatus.ContractNotActive);

            if (!playerInventory.TryGetItemInstance(itemInstanceId, out PlacedItem item))
                return Result(CustomerContractFulfilmentStatus.ItemNotFound);
            if (item.Type != requiredItem)
                return Result(CustomerContractFulfilmentStatus.WrongItem);
            if (item.Count != requiredQuantity)
                return Result(CustomerContractFulfilmentStatus.WrongQuantity);
            if (!ownership.TryGetOwner(itemInstanceId, out EconomicActorId owner) || owner != EconomicActorId.Player)
                return Result(CustomerContractFulfilmentStatus.OwnershipMismatch);
            if (item.State.Ownership == ItemOwnershipStatus.Stolen && !acceptStolenGoods)
                return Result(CustomerContractFulfilmentStatus.StolenGoodsNotAccepted);
            if (payerAccount.Balance < reward)
                return Result(CustomerContractFulfilmentStatus.InsufficientFunds);
            if (playerAccount.Balance > int.MaxValue - reward)
                return Result(CustomerContractFulfilmentStatus.AccountOverflow);

            InventorySnapshot playerBefore = playerInventory.CreateSnapshot();
            InventorySnapshot receiverBefore = receiverInventory.CreateSnapshot();
            int playerBalanceBefore = playerAccount.Balance;
            int payerBalanceBefore = payerAccount.Balance;

            AssetTransferResult transfer = transfers.TransferStackDeferred(playerInventory, receiverInventory,
                itemInstanceId, EconomicActorId.Player, receiverActor, AssetTransferReason.ContractDelivery,
                day, minuteOfDay);
            if (!transfer.Success)
            {
                return Result(transfer.Status == AssetTransferStatus.DestinationFull
                    ? CustomerContractFulfilmentStatus.ReceiverFull
                    : CustomerContractFulfilmentStatus.TransferFailed);
            }

            if (!payerAccount.TryDebit(reward) || !playerAccount.TryCredit(reward) ||
                !contracts.TryComplete(contractId, transfer.Transaction.TransactionId, reward, payerActor, day, minuteOfDay))
            {
                payerAccount.RestoreSilently(payerBalanceBefore);
                playerAccount.RestoreSilently(playerBalanceBefore);
                ownership.TryTransferOwnership(itemInstanceId, receiverActor, EconomicActorId.Player);
                economicTransactions.TryRemove(transfer.Transaction.TransactionId);
                playerInventory.RestoreSilently(playerBefore);
                receiverInventory.RestoreSilently(receiverBefore);
                return Result(CustomerContractFulfilmentStatus.ContractStateConflict);
            }

            if (familiarityDelta != 0f || trustDelta != 0f || affinityDelta != 0f)
                relationships.Adjust(relationshipNpcId, familiarityDelta, trustDelta, affinityDelta);
            if (npcFamiliarityDelta != 0f || npcTrustDelta != 0f || npcAffinityDelta != 0f)
                npcSocialWorld.AdjustPlayerRelationship(npcOpinionNpcId,
                    npcFamiliarityDelta, npcTrustDelta, npcAffinityDelta);

            // The transfer, payment, contract state and relationship outcomes are now committed.
            playerInventory.NotifyChanged();
            receiverInventory.NotifyChanged();
            return new CustomerContractFulfilmentResult(CustomerContractFulfilmentStatus.Completed,
                transfer.Transaction, reward);
        }

        private static bool ValidRelationshipDelta(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= -1f && value <= 1f;
        }

        private static CustomerContractFulfilmentResult Result(CustomerContractFulfilmentStatus status)
        {
            return new CustomerContractFulfilmentResult(status, null, 0);
        }
    }
}
