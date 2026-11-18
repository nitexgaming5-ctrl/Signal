using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Authored first-opportunity vertical slice. It coordinates one small scenario only;
    /// reusable discovery, relationship, inventory and lifecycle rules remain in Core.
    /// </summary>
    public sealed class MiloParcelOpportunityController : MonoBehaviour
    {
        private enum PendingChoice { None, Offer }

        private static readonly Vector2 ParcelCounterPosition = new Vector2(997.35f, 1001.05f);
        private static readonly Vector2 EvidencePosition = new Vector2(1002.75f, 1001.05f);
        private static readonly Vector2 DropOffPosition = new Vector2(20.5f, -16.5f);
        private const float InteractionRadius = 1.85f;
        private const string MiloId = "milo";
        private const string RheaId = "rhea";
        private const string OpportunityId = "milo-parcel";

        private GridInventory _inventory;
        private ItemType _parcelType;
        private PlayerDiscoveryLedger _discoveries;
        private PlayerRelationshipBook _relationships;
        private ItemKnowledgeBook _itemKnowledge;
        private GameClock _clock;
        private CityBlockBootstrap _city;
        private OpportunityState _state;
        private OpportunityDefinition _definition;
        private Action<string> _say;
        private PendingChoice _pendingChoice;
        private GameObject _counterParcel;
        private GameObject _evidenceSlip;
        private GameObject _dropMarker;
        private GameObject _carriedParcelMarker;
        private int _lastRoutineObservationDay;
        private int _lastRumourDay;
        private bool _bound;

        public bool HasPendingChoice => _pendingChoice != PendingChoice.None;

        public void RestoreOpportunityState(OpportunityState state)
        {
            if (state == null || state.OpportunityId != OpportunityId)
                throw new ArgumentException("A matching Milo parcel opportunity state is required.", nameof(state));
            _state = state;
            if (_state.ParcelInstanceId == Guid.Empty)
                _state.SetParcelInstanceId(Guid.NewGuid());
            _pendingChoice = PendingChoice.None;
            RefreshWorldProps();
        }

        public void Bind(Transform player, GridInventory inventory, ItemType parcelType,
            PlayerDiscoveryLedger discoveries, PlayerRelationshipBook relationships,
            ItemKnowledgeBook itemKnowledge, GameClock clock, CityBlockBootstrap city,
            OpportunityState state, Action<string> say)
        {
            if (player == null || inventory == null || discoveries == null || relationships == null ||
                itemKnowledge == null || clock == null || city == null || state == null || say == null)
                throw new ArgumentNullException("Milo parcel scenario dependencies must all be provided.");

            _inventory = inventory;
            _parcelType = parcelType;
            _discoveries = discoveries;
            _relationships = relationships;
            _itemKnowledge = itemKnowledge;
            _clock = clock;
            _city = city;
            _state = state;
            _say = say;
            _definition = new OpportunityDefinition(OpportunityId, MiloId,
                new InteractionConditionSet(NpcActivity.Socialize, minimumFamiliarity: 0.2f,
                    minimumDistinctSignalTypes: 2));

            if (_state.ParcelInstanceId == Guid.Empty)
                _state.SetParcelInstanceId(Guid.NewGuid());

            BuildWorldProps();
            RefreshWorldProps();
            _bound = true;
        }

        public string GetPrompt(Vector2 playerPosition, string place)
        {
            if (!_bound)
                return string.Empty;
            if (_pendingChoice == PendingChoice.Offer)
                return "1 Accept   2 Ask why   3 Inspect   4 Decline";

            if (place == "CornerShop")
            {
                if (Vector2.Distance(playerPosition, EvidencePosition) <= InteractionRadius)
                    return "E  Inspect delivery slip";
                if (IsParcelOnCounter() && Vector2.Distance(playerPosition, ParcelCounterPosition) <= InteractionRadius)
                    return "E  Inspect wrapped parcel";
                if (_city.HasResident(MiloId) &&
                    Vector2.Distance(playerPosition, _city.GetResidentPosition(MiloId)) <= InteractionRadius)
                    return "E  Talk to Milo";
            }

            if (place == "Street")
            {
                if (_state.Phase == OpportunityPhase.Accepted &&
                    Vector2.Distance(playerPosition, DropOffPosition) <= InteractionRadius)
                    return "E  Use discreet drop-off";

                if (_city.HasResident(MiloId) &&
                    Vector2.Distance(playerPosition, _city.GetResidentPosition(MiloId)) <= InteractionRadius)
                    return _state.Phase == OpportunityPhase.Accepted
                        ? "E  Return parcel to Milo"
                        : "E  Talk to Milo";

                if (_city.HasResident(RheaId) &&
                    Vector2.Distance(playerPosition, _city.GetResidentPosition(RheaId)) <= InteractionRadius)
                    return "E  Talk to Rhea";
            }

            return string.Empty;
        }

        public void Tick(Vector2 playerPosition, string place)
        {
            if (!_bound)
                return;

            RefreshWorldProps();
            if (place != "Street" || !_city.HasResident(MiloId) ||
                _city.GetResidentActivity(MiloId) != NpcActivity.Socialize)
                return;

            Vector2 miloPosition = _city.GetResidentPosition(MiloId);
            if (Vector2.Distance(playerPosition, miloPosition) > 5.5f ||
                _lastRoutineObservationDay == _clock.Day)
                return;

            _lastRoutineObservationDay = _clock.Day;
            RecordSignal("milo-routine-" + _clock.Day, DiscoverySignalType.RoutineObservation,
                "milo-after-hours", "Milo stepped outside after closing with a wrapped parcel.",
                1f);
            _say("You notice Milo carrying a wrapped parcel after the shop has closed.");
        }

        public bool TryInteract(Vector2 playerPosition, string place, out string message)
        {
            message = null;
            if (!_bound)
                return false;

            if (place == "CornerShop")
            {
                if (Vector2.Distance(playerPosition, EvidencePosition) <= InteractionRadius)
                {
                    InspectDeliverySlip();
                    message = "The slip has Milo's flour-marked initials and a scratched-out recipient line. It looks like a private delivery.";
                    return true;
                }

                if (IsParcelOnCounter() &&
                    Vector2.Distance(playerPosition, ParcelCounterPosition) <= InteractionRadius)
                {
                    InspectParcel();
                    message = "The wrapping is intact. The label has been removed; the marks underneath suggest the parcel may not belong to Milo.";
                    return true;
                }

                if (_city.HasResident(MiloId) &&
                    Vector2.Distance(playerPosition, _city.GetResidentPosition(MiloId)) <= InteractionRadius)
                {
                    message = TalkToMilo();
                    return true;
                }
            }

            if (place != "Street")
                return false;

            if (_state.Phase == OpportunityPhase.Accepted &&
                Vector2.Distance(playerPosition, DropOffPosition) <= InteractionRadius)
            {
                message = DeliverParcel();
                return true;
            }

            if (_city.HasResident(MiloId) &&
                Vector2.Distance(playerPosition, _city.GetResidentPosition(MiloId)) <= InteractionRadius)
            {
                if (_state.Phase == OpportunityPhase.Accepted)
                {
                    message = ReturnParcel();
                    return true;
                }

                message = TalkToMilo();
                return true;
            }

            if (_city.HasResident(RheaId) &&
                Vector2.Distance(playerPosition, _city.GetResidentPosition(RheaId)) <= InteractionRadius)
            {
                message = TalkToRhea();
                return true;
            }

            return false;
        }

        public bool HandleChoice(int choice, out string message)
        {
            message = null;
            if (!_bound || _pendingChoice != PendingChoice.Offer)
                return false;

            _pendingChoice = PendingChoice.None;
            switch (choice)
            {
                case 1:
                    if (!TryAcceptParcel())
                    {
                        _pendingChoice = PendingChoice.Offer;
                        message = "There isn't enough room in your carried inventory for the parcel. Free some space and choose again.";
                        return true;
                    }
                    message = "Milo hands you the parcel. Take it to the discreet drop-off point marked on the street.";
                    return true;

                case 2:
                    if (RecordSignal("milo-explained-favour", DiscoverySignalType.Conversation,
                        MiloId, "Milo says he is helping someone he knows who is in a difficult position.", 0.9f))
                        _relationships.Adjust(MiloId, trustDelta: 0.05f);
                    _pendingChoice = PendingChoice.Offer;
                    message = "Milo: It's for someone I know. They're in a bind, and I promised I'd help. That's all I can tell you.\\n\\n[1] Accept   [2] Ask why   [3] Inspect first   [4] Decline";
                    return true;

                case 3:
                    InspectParcel();
                    _pendingChoice = PendingChoice.Offer;
                    message = "You inspect the parcel: the wrapping is intact, but the removed label and delivery slip suggest it was taken from someone else.\\n\\n[1] Accept   [2] Ask why   [3] Inspect again   [4] Decline";
                    return true;

                case 4:
                    _state.RecordDecline(_clock.Day, _clock.MinuteOfDay);
                    message = "Milo nods. He doesn't press you. The parcel stays where it is.";
                    return true;

                default:
                    message = "Choose 1, 2, 3 or 4.";
                    _pendingChoice = PendingChoice.Offer;
                    return true;
            }
        }

        private string TalkToMilo()
        {
            if (_city.GetResidentActivity(MiloId) != NpcActivity.Socialize)
            {
                _relationships.Adjust(MiloId, familiarityDelta: 0.1f);
                RecordConversation("milo-everyday",
                    "Milo chatted about the bakery and the neighbourhood.", 1f);
                return "Milo: Long day. The ovens never seem to cool down.";
            }

            InteractionContext context = new InteractionContext(_city.GetResidentActivity(MiloId),
                _relationships.Get(MiloId), _discoveries);

            if (_state.Phase == OpportunityPhase.Dormant &&
                new InteractionConditionSet(minimumDistinctSignalTypes: 2)
                    .IsSatisfiedBy(context, MiloId) &&
                _definition.OfferConditions.IsSatisfiedBy(context, MiloId))
                _state.MakeAvailable();

            if (_definition.CanOffer(_state, context))
            {
                _pendingChoice = PendingChoice.Offer;
                RecordConversation("milo-private-offer",
                    "Milo asks the player to move a parcel for someone he knows.", 0.9f);
                return "Milo: Listen, could you do me a small favour? There's a parcel I need moved. Just take it to the drop-off point. I'd rather not leave the shop.\n\n[1] Accept   [2] Ask why   [3] Inspect first   [4] Decline";
            }

            _relationships.Adjust(MiloId, familiarityDelta: 0.1f);
            RecordConversation("milo-evening",
                "Milo made brief conversation after closing.", 1f);
            return "Milo: Some people around here need a hand now and then. You learn not to ask too many questions.";
        }

        private string TalkToRhea()
        {
            if (_city.GetResidentActivity(RheaId) == NpcActivity.Socialize &&
                _lastRumourDay != _clock.Day)
            {
                _lastRumourDay = _clock.Day;
                RecordSignal("milo-rumour-rhea-" + _clock.Day, DiscoverySignalType.Rumour,
                    RheaId, "Rhea heard that Milo has been carrying parcels after closing, but does not know why.",
                    0.4f);
                _relationships.Adjust(RheaId, familiarityDelta: 0.05f);
                return "Rhea: I heard Milo has been carrying parcels after closing. Could be nothing. I don't know the whole story.";
            }

            _relationships.Adjust(RheaId, familiarityDelta: 0.05f);
            return "Rhea: You hear things around this block, but half of it is probably just talk.";
        }

        private void InspectDeliverySlip()
        {
            RecordSignal("milo-delivery-slip", DiscoverySignalType.PhysicalEvidence,
                "shop-counter-slip", "A delivery slip bears Milo's flour-marked initials and a scratched-out recipient line.",
                1f);
        }

        private void InspectParcel()
        {
            _itemKnowledge.RecordFact(_state.ParcelInstanceId, ItemKnowledgeAspect.Condition,
                (int)ItemCondition.Good, ItemKnowledgeSource.DirectInspection, 1f,
                _clock.Day, _clock.MinuteOfDay);
            _itemKnowledge.RecordFact(_state.ParcelInstanceId, ItemKnowledgeAspect.Ownership,
                (int)ItemOwnershipStatus.Stolen, ItemKnowledgeSource.DirectInspection, 0.7f,
                _clock.Day, _clock.MinuteOfDay);
            RecordSignal("milo-parcel-inspection", DiscoverySignalType.PhysicalEvidence,
                "wrapped-parcel", "The parcel's original label has been deliberately removed.",
                1f);
        }

        private void RecordConversation(string suffix, string description, float confidence)
        {
            string id = "milo-conversation-" + suffix + "-" + _clock.Day + "-" + _clock.MinuteOfDay + "-" + _discoveries.Count;
            RecordSignal(id, DiscoverySignalType.Conversation, MiloId, description, confidence);
        }

        private bool RecordSignal(string id, DiscoverySignalType type, string source,
            string description, float confidence)
        {
            return _discoveries.Record(new PlayerDiscoverySignal(id, MiloId, type, source, description,
                confidence, _clock.Day, _clock.MinuteOfDay));
        }

        private bool TryAcceptParcel()
        {
            if (_state.Phase != OpportunityPhase.Available ||
                _state.ParcelInstanceId == Guid.Empty ||
                !IsParcelOnCounter())
                return false;

            var parcelState = new ItemInstanceState(ItemQuality.Standard, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);

            for (int y = 0; y < _inventory.Height; y++)
            {
                for (int x = 0; x < _inventory.Width; x++)
                {
                    int? localId = _inventory.Place(_parcelType, x, y, false, 1,
                        _state.ParcelInstanceId, parcelState);
                    if (!localId.HasValue)
                        continue;

                    if (_state.TryAccept(_state.ParcelInstanceId, _clock.Day, _clock.MinuteOfDay))
                    {
                        RefreshWorldProps();
                        return true;
                    }

                    _inventory.Remove(localId.Value);
                    return false;
                }
            }

            return false;
        }

        private string ReturnParcel()
        {
            if (!_inventory.TryGetItemInstance(_state.ParcelInstanceId, out PlacedItem parcel))
                return "You no longer have the parcel to return.";

            if (!_inventory.Remove(parcel.InstanceId))
                return "You couldn't return the parcel.";

            if (!_state.TryReturnParcel(_clock.Day, _clock.MinuteOfDay))
            {
                RestoreParcelToInventory(parcel);
                return "The parcel could not be returned just now.";
            }

            RefreshWorldProps();
            return "Milo takes the parcel back. You can reconsider later; he doesn't treat it as a betrayal.";
        }

        private string DeliverParcel()
        {
            if (!_inventory.TryGetItemInstance(_state.ParcelInstanceId, out PlacedItem parcel))
                return "You don't have the parcel with you.";

            bool onTime = _clock.Day == _state.LastAcceptedDay;
            bool intact = parcel.State.Condition != ItemCondition.Damaged;
            bool witnessed = _city.IsAnyResidentWithin(DropOffPosition, 6.5f, MiloId);

            if (!_inventory.Remove(parcel.InstanceId))
                return "You couldn't leave the parcel here.";

            if (!_state.TryResolveDelivered(_clock.Day, _clock.MinuteOfDay,
                onTime, intact, witnessed))
            {
                RestoreParcelToInventory(parcel);
                return "The drop-off could not be completed.";
            }

            if (onTime && intact && !witnessed)
            {
                _relationships.Adjust(MiloId, familiarityDelta: 0.05f, trustDelta: 0.1f);
                RefreshWorldProps();
                return "You leave the parcel unseen and on time. It is still intact.\nMilo: Thank you. You helped someone who really needed it.";
            }

            _relationships.Adjust(MiloId, trustDelta: -0.05f);
            RefreshWorldProps();
            var outcomes = new List<string>();
            if (!onTime) outcomes.Add("the delivery was late");
            if (!intact) outcomes.Add("the parcel was damaged");
            if (witnessed) outcomes.Add("someone was nearby and may have seen you");
            return "The parcel is delivered, but " + string.Join(", ", outcomes) + ".\nMilo: I appreciate you seeing it through. I'll have to deal with the complications.";
        }

        private bool RestoreParcelToInventory(PlacedItem parcel)
        {
            for (int y = 0; y < _inventory.Height; y++)
                for (int x = 0; x < _inventory.Width; x++)
                    if (_inventory.Place(parcel.Type, x, y, parcel.Rotated, parcel.Count,
                        parcel.ItemInstanceId, parcel.State).HasValue)
                        return true;
            return false;
        }

        private bool IsParcelOnCounter()
        {
            return _state.Phase == OpportunityPhase.Dormant || _state.Phase == OpportunityPhase.Available;
        }

        private void BuildWorldProps()
        {
            _counterParcel = CreateParcelVisual("CounterParcel", new Vector2(ParcelCounterPosition.x, ParcelCounterPosition.y), 22);
            _evidenceSlip = CreateMarker("DeliverySlip", EvidencePosition, new Color32(218, 207, 170, 255), new Vector2(0.45f, 0.22f), 23);
            _dropMarker = CreateMarker("DiscreetDropOff", DropOffPosition, new Color32(174, 146, 103, 255), new Vector2(0.85f, 0.22f), 12);
            _carriedParcelMarker = CreateParcelVisual("MiloAfterHoursParcel", Vector2.zero, 18);
        }

        private void RefreshWorldProps()
        {
            if (_counterParcel != null)
                _counterParcel.SetActive(IsParcelOnCounter());
            if (_evidenceSlip != null)
                _evidenceSlip.SetActive(_state.Phase != OpportunityPhase.Resolved);
            if (_dropMarker != null)
                _dropMarker.SetActive(_state.Phase == OpportunityPhase.Accepted ||
                    _state.Phase == OpportunityPhase.Resolved);
            if (_carriedParcelMarker != null)
            {
                bool show = _state.Phase != OpportunityPhase.Accepted &&
                    _state.Phase != OpportunityPhase.Resolved && _city.CurrentPlace == "Street" &&
                    _city.HasResident(MiloId) && _city.GetResidentActivity(MiloId) == NpcActivity.Socialize;
                _carriedParcelMarker.SetActive(show);
                if (show)
                {
                    Vector2 position = _city.GetResidentPosition(MiloId) + new Vector2(0.55f, 0.25f);
                    _carriedParcelMarker.transform.position = new Vector3(position.x, position.y, 0f);
                }
            }
        }

        private GameObject CreateParcelVisual(string name, Vector2 position, int sortingOrder)
        {
            var root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(position.x, position.y, 0f);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(new Color32(139, 103, 69, 255));
            renderer.sortingOrder = sortingOrder;
            root.transform.localScale = new Vector3(0.75f, 0.48f, 1f);

            var band = new GameObject("ParcelBand");
            band.transform.SetParent(root.transform, false);
            band.transform.localScale = new Vector3(0.16f, 1.05f, 1f);
            var bandRenderer = band.AddComponent<SpriteRenderer>();
            bandRenderer.sprite = PixelSprites.Solid(new Color32(213, 190, 146, 255));
            bandRenderer.sortingOrder = sortingOrder + 1;
            return root;
        }

        private GameObject CreateMarker(string name, Vector2 position, Color color, Vector2 size, int sortingOrder)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(transform, false);
            marker.transform.position = new Vector3(position.x, position.y, 0f);
            marker.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;
            return marker;
        }

        private void OnDestroy()
        {
            if (_counterParcel != null) Destroy(_counterParcel);
            if (_evidenceSlip != null) Destroy(_evidenceSlip);
            if (_dropMarker != null) Destroy(_dropMarker);
            if (_carriedParcelMarker != null) Destroy(_carriedParcelMarker);
        }
    }
}
