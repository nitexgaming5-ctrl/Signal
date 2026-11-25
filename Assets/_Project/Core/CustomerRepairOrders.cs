using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum CustomerRepairOrderPhase
    {
        Offered = 0,
        Active = 1,
        Completed = 2
    }

    public enum CustomerRepairOrderStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        OfferUnavailable = 2,
        ItemNotFound = 3,
        WrongItem = 4,
        ItemNotRepairable = 5,
        OwnershipMismatch = 6,
        ContainerMismatch = 7,
        InventoryFull = 8,
        InsufficientFunds = 9,
        AccountOverflow = 10,
        TransferFailed = 11,
        TransactionConflict = 12,
        OrderConflict = 13,
        ItemNotRestored = 14,
        CustomerAccountUnavailable = 15,
        PlayerAccountUnavailable = 16
    }

    public sealed class CustomerRepairOrderState
    {
        internal CustomerRepairOrderState(Guid orderId, EconomicActorId issuer,
            Guid itemDefinitionId, int reward, ItemCondition requiredReturnCondition)
        {
            OrderId = orderId;
            Issuer = issuer;
            ItemDefinitionId = itemDefinitionId;
            Reward = reward;
            RequiredReturnCondition = requiredReturnCondition;
            Phase = CustomerRepairOrderPhase.Offered;
        }

        public Guid OrderId { get; }
        public EconomicActorId Issuer { get; }
        public Guid ItemDefinitionId { get; }
        public int Reward { get; }
        public ItemCondition RequiredReturnCondition { get; }
        public CustomerRepairOrderPhase Phase { get; internal set; }
        public Guid ItemInstanceId { get; internal set; }
        public int AcceptedDay { get; internal set; }
        public int AcceptedMinuteOfDay { get; internal set; }
        public int CompletedDay { get; internal set; }
        public int CompletedMinuteOfDay { get; internal set; }
        public Guid PaymentTransactionId { get; internal set; }
    }

    /// <summary>One authored repair order at a time; the entrusted item's owner never changes.</summary>
    public sealed class CustomerRepairOrderBook
    {
        private readonly Dictionary<Guid, CustomerRepairOrderState> _states =
            new Dictionary<Guid, CustomerRepairOrderState>();

        public int Count => _states.Count;
        public bool HasActiveOrder
        {
            get
            {
                foreach (CustomerRepairOrderState state in _states.Values)
                    if (state.Phase == CustomerRepairOrderPhase.Active) return true;
                return false;
            }
        }
        public IEnumerable<CustomerRepairOrderState> States => _states.Values;

        public bool TryRegisterOffer(Guid orderId, EconomicActorId issuer, Guid itemDefinitionId, int reward,
            ItemCondition requiredReturnCondition = ItemCondition.Good)
        {
            if (orderId == Guid.Empty || !issuer.IsValid || issuer == EconomicActorId.Player ||
                itemDefinitionId == Guid.Empty || reward < 1 ||
                requiredReturnCondition != ItemCondition.Good ||
                _states.ContainsKey(orderId))
                return false;
            _states.Add(orderId, new CustomerRepairOrderState(orderId, issuer, itemDefinitionId, reward, requiredReturnCondition));
            return true;
        }

        public bool TryGetState(Guid orderId, out CustomerRepairOrderState state) =>
            _states.TryGetValue(orderId, out state);

        internal bool TryAccept(Guid orderId, Guid itemInstanceId, int day, int minuteOfDay)
        {
            if (itemInstanceId == Guid.Empty || !ValidTime(day, minuteOfDay) || HasActiveOrder ||
                !_states.TryGetValue(orderId, out CustomerRepairOrderState state) ||
                state.Phase != CustomerRepairOrderPhase.Offered)
                return false;
            state.ItemInstanceId = itemInstanceId;
            state.AcceptedDay = day;
            state.AcceptedMinuteOfDay = minuteOfDay;
            state.Phase = CustomerRepairOrderPhase.Active;
            return true;
        }

        internal bool TryComplete(Guid orderId, Guid paymentTransactionId, int day, int minuteOfDay)
        {
            if (paymentTransactionId == Guid.Empty || !ValidTime(day, minuteOfDay) ||
                !_states.TryGetValue(orderId, out CustomerRepairOrderState state) ||
                state.Phase != CustomerRepairOrderPhase.Active ||
                day < state.AcceptedDay || (day == state.AcceptedDay && minuteOfDay < state.AcceptedMinuteOfDay))
                return false;
            state.PaymentTransactionId = paymentTransactionId;
            state.CompletedDay = day;
            state.CompletedMinuteOfDay = minuteOfDay;
            state.Phase = CustomerRepairOrderPhase.Completed;
            return true;
        }

        public void ResetToOffers()
        {
            foreach (CustomerRepairOrderState state in _states.Values)
            {
                state.Phase = CustomerRepairOrderPhase.Offered;
                state.ItemInstanceId = Guid.Empty;
                state.AcceptedDay = 0;
                state.AcceptedMinuteOfDay = 0;
                state.CompletedDay = 0;
                state.CompletedMinuteOfDay = 0;
                state.PaymentTransactionId = Guid.Empty;
            }
        }

        public bool RestoreState(Guid orderId, CustomerRepairOrderPhase phase, Guid itemInstanceId,
            int acceptedDay, int acceptedMinuteOfDay, int completedDay, int completedMinuteOfDay,
            Guid paymentTransactionId)
        {
            if (!_states.TryGetValue(orderId, out CustomerRepairOrderState state) ||
                !Enum.IsDefined(typeof(CustomerRepairOrderPhase), phase))
                return false;

            bool accepted = phase != CustomerRepairOrderPhase.Offered;
            bool completed = phase == CustomerRepairOrderPhase.Completed;
            if (accepted)
            {
                if (itemInstanceId == Guid.Empty || !ValidTime(acceptedDay, acceptedMinuteOfDay))
                    return false;
            }
            else if (itemInstanceId != Guid.Empty || acceptedDay != 0 || acceptedMinuteOfDay != 0)
                return false;

            if (completed)
            {
                if (!ValidTime(completedDay, completedMinuteOfDay) ||
                    completedDay < acceptedDay ||
                    (completedDay == acceptedDay && completedMinuteOfDay < acceptedMinuteOfDay) ||
                    paymentTransactionId == Guid.Empty)
                    return false;
            }
            else if (completedDay != 0 || completedMinuteOfDay != 0 || paymentTransactionId != Guid.Empty)
                return false;

            if (phase == CustomerRepairOrderPhase.Active && HasActiveOrder)
                return false;

            state.Phase = phase;
            state.ItemInstanceId = itemInstanceId;
            state.AcceptedDay = acceptedDay;
            state.AcceptedMinuteOfDay = acceptedMinuteOfDay;
            state.CompletedDay = completedDay;
            state.CompletedMinuteOfDay = completedMinuteOfDay;
            state.PaymentTransactionId = paymentTransactionId;
            return true;
        }

        private static bool ValidTime(int day, int minuteOfDay) =>
            day >= 1 && minuteOfDay >= 0 && minuteOfDay < 1440;
    }

    public readonly struct CustomerRepairOrderResult
    {
        internal CustomerRepairOrderResult(CustomerRepairOrderStatus status,
            ServicePaymentTransaction transaction = null, Guid itemInstanceId = default)
        {
            Status = status;
            Transaction = transaction;
            ItemInstanceId = itemInstanceId;
        }
        public CustomerRepairOrderStatus Status { get; }
        public ServicePaymentTransaction Transaction { get; }
        public Guid ItemInstanceId { get; }
        public bool Success => Status == CustomerRepairOrderStatus.Completed;
    }

    /// <summary>
    /// Moves an issuer-owned item into player custody, then returns it and pays the player
    /// as one transaction. Asset ownership remains with the issuer throughout.
    /// </summary>
    public static class CustomerRepairOrderService
    {
        public static CustomerRepairOrderResult TryAccept(Guid orderId, ItemType requiredItem,
            GridInventory issuerInventory, GridInventory playerInventory, EconomicContainerRegistry containers,
            AssetOwnershipBook ownership, CustomerRepairOrderBook orders, int day, int minuteOfDay)
        {
            if (orderId == Guid.Empty || !ValidItem(requiredItem) || issuerInventory == null ||
                playerInventory == null || ReferenceEquals(issuerInventory, playerInventory) ||
                containers == null || ownership == null || orders == null ||
                !ValidTime(day, minuteOfDay))
                return Result(CustomerRepairOrderStatus.InvalidRequest);

            if (!orders.TryGetState(orderId, out CustomerRepairOrderState order) ||
                order.Phase != CustomerRepairOrderPhase.Offered || orders.HasActiveOrder)
                return Result(CustomerRepairOrderStatus.OfferUnavailable);
            if (requiredItem.Id != order.ItemDefinitionId)
                return Result(CustomerRepairOrderStatus.WrongItem);
            if (!containers.TryGetCustodian(issuerInventory, out EconomicActorId issuerCustodian) ||
                issuerCustodian != order.Issuer ||
                !containers.TryGetCustodian(playerInventory, out EconomicActorId playerCustodian) ||
                playerCustodian != EconomicActorId.Player)
                return Result(CustomerRepairOrderStatus.ContainerMismatch);

            PlacedItem item = default;
            bool foundMatchingType = false;
            bool foundIssuerOwnedItem = false;
            bool foundRepairableItem = false;
            foreach (PlacedItem candidate in issuerInventory.Items)
            {
                if (candidate.Type != requiredItem) continue;
                foundMatchingType = true;
                if (candidate.Count != 1 || candidate.Type.MaxStack != 1) continue;
                if (!ownership.TryGetOwner(candidate.ItemInstanceId, out EconomicActorId owner) || owner != order.Issuer)
                    continue;
                foundIssuerOwnedItem = true;
                if (candidate.State.Condition != ItemCondition.Damaged && candidate.State.Condition != ItemCondition.Worn)
                    continue;
                foundRepairableItem = true;
                item = candidate;
                break;
            }
            if (!foundMatchingType) return Result(CustomerRepairOrderStatus.ItemNotFound);
            if (!foundIssuerOwnedItem) return Result(CustomerRepairOrderStatus.OwnershipMismatch);
            if (!foundRepairableItem) return Result(CustomerRepairOrderStatus.ItemNotRepairable);

            InventorySnapshot issuerBefore = issuerInventory.CreateSnapshot();
            InventorySnapshot playerBefore = playerInventory.CreateSnapshot();
            InventoryTransferResult movement = InventoryTransfer.MoveStack(issuerInventory, playerInventory, item.ItemInstanceId);
            if (!movement.Success || movement.Moved != 1)
            {
                issuerInventory.Restore(issuerBefore);
                playerInventory.Restore(playerBefore);
                return Result(movement.Success ? CustomerRepairOrderStatus.InventoryFull : CustomerRepairOrderStatus.TransferFailed);
            }
            if (!orders.TryAccept(orderId, item.ItemInstanceId, day, minuteOfDay))
            {
                issuerInventory.Restore(issuerBefore);
                playerInventory.Restore(playerBefore);
                return Result(CustomerRepairOrderStatus.OrderConflict);
            }
            return new CustomerRepairOrderResult(CustomerRepairOrderStatus.Completed, null, item.ItemInstanceId);
        }

        public static CustomerRepairOrderResult TryReturn(Guid orderId, ItemType requiredItem,
            GridInventory playerInventory, GridInventory issuerInventory, EconomicContainerRegistry containers,
            AssetOwnershipBook ownership, CleanMoneyAccount playerAccount, EconomicAccountBook accounts,
            ServicePaymentTransactionLedger transactions, CustomerRepairOrderBook orders, int day, int minuteOfDay)
        {
            if (orderId == Guid.Empty || !ValidItem(requiredItem) || playerInventory == null ||
                issuerInventory == null || ReferenceEquals(playerInventory, issuerInventory) || containers == null ||
                ownership == null || playerAccount == null || accounts == null || transactions == null || orders == null ||
                !ValidTime(day, minuteOfDay))
                return Result(CustomerRepairOrderStatus.InvalidRequest);

            if (!orders.TryGetState(orderId, out CustomerRepairOrderState order) ||
                order.Phase != CustomerRepairOrderPhase.Active || order.ItemInstanceId == Guid.Empty)
                return Result(CustomerRepairOrderStatus.OfferUnavailable);
            if (!accounts.TryGetAccount(EconomicActorId.Player, out CleanMoneyAccount registeredPlayerAccount) ||
                !ReferenceEquals(playerAccount, registeredPlayerAccount))
                return Result(CustomerRepairOrderStatus.PlayerAccountUnavailable);
            if (!accounts.TryGetAccount(order.Issuer, out CleanMoneyAccount issuerAccount) ||
                ReferenceEquals(playerAccount, issuerAccount))
                return Result(CustomerRepairOrderStatus.CustomerAccountUnavailable);
            if (!containers.TryGetCustodian(playerInventory, out EconomicActorId playerCustodian) ||
                playerCustodian != EconomicActorId.Player ||
                !containers.TryGetCustodian(issuerInventory, out EconomicActorId issuerCustodian) ||
                issuerCustodian != order.Issuer)
                return Result(CustomerRepairOrderStatus.ContainerMismatch);
            if (!playerInventory.TryGetItemInstance(order.ItemInstanceId, out PlacedItem item))
                return Result(CustomerRepairOrderStatus.ItemNotFound);
            if (item.Type != requiredItem || item.Count != 1 || item.Type.MaxStack != 1 ||
                item.Type.Id != order.ItemDefinitionId)
                return Result(CustomerRepairOrderStatus.WrongItem);
            if (!ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) || owner != order.Issuer)
                return Result(CustomerRepairOrderStatus.OwnershipMismatch);
            bool returnConditionMet = order.RequiredReturnCondition == ItemCondition.Good
                ? item.State.Condition == ItemCondition.Good || item.State.Condition == ItemCondition.Pristine
                : item.State.Condition == ItemCondition.Pristine;
            if (!returnConditionMet)
                return Result(CustomerRepairOrderStatus.ItemNotRestored);
            if (issuerAccount.Balance < order.Reward)
                return Result(CustomerRepairOrderStatus.InsufficientFunds);
            if (playerAccount.Balance > int.MaxValue - order.Reward)
                return Result(CustomerRepairOrderStatus.AccountOverflow);

            InventorySnapshot playerBefore = playerInventory.CreateSnapshot();
            InventorySnapshot issuerBefore = issuerInventory.CreateSnapshot();
            int playerBalanceBefore = playerAccount.Balance;
            int issuerBalanceBefore = issuerAccount.Balance;

            InventoryTransferResult movement = InventoryTransfer.MoveStack(playerInventory, issuerInventory, item.ItemInstanceId);
            if (!movement.Success || movement.Moved != 1)
            {
                playerInventory.Restore(playerBefore);
                issuerInventory.Restore(issuerBefore);
                return Result(movement.Success ? CustomerRepairOrderStatus.InventoryFull : CustomerRepairOrderStatus.TransferFailed);
            }

            if (!issuerAccount.TryDebit(order.Reward) || !playerAccount.TryCredit(order.Reward))
            {
                issuerAccount.RestoreSilently(issuerBalanceBefore);
                playerAccount.RestoreSilently(playerBalanceBefore);
                playerInventory.Restore(playerBefore);
                issuerInventory.Restore(issuerBefore);
                return Result(CustomerRepairOrderStatus.TransactionConflict);
            }

            var transaction = new ServicePaymentTransaction(Guid.NewGuid(), orderId,
                item.ItemInstanceId, order.Issuer, EconomicActorId.Player, order.Reward, day, minuteOfDay);
            bool recorded = transactions.TryRecord(transaction);
            if (!recorded || !orders.TryComplete(orderId, transaction.TransactionId, day, minuteOfDay))
            {
                if (recorded)
                    transactions.TryRemove(transaction.TransactionId);
                issuerAccount.RestoreSilently(issuerBalanceBefore);
                playerAccount.RestoreSilently(playerBalanceBefore);
                playerInventory.Restore(playerBefore);
                issuerInventory.Restore(issuerBefore);
                return Result(CustomerRepairOrderStatus.TransactionConflict);
            }

            return new CustomerRepairOrderResult(CustomerRepairOrderStatus.Completed,
                transaction, item.ItemInstanceId);
        }

        private static bool ValidItem(ItemType item) =>
            item.Id != Guid.Empty && item.Width > 0 && item.Height > 0 && item.MaxStack == 1;

        private static bool ValidTime(int day, int minuteOfDay) =>
            day >= 1 && minuteOfDay >= 0 && minuteOfDay < 1440;

        private static CustomerRepairOrderResult Result(CustomerRepairOrderStatus status) =>
            new CustomerRepairOrderResult(status);

    }
}