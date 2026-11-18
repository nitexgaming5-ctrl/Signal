using System;

namespace Signal.Core
{
    /// <summary>
    /// Immutable identity and offer requirements for one authored opportunity.
    /// Character-specific dialogue and Unity presentation remain in Signal.Game.
    /// </summary>
    public sealed class OpportunityDefinition
    {
        public string Id { get; }
        public string NpcId { get; }
        public InteractionConditionSet OfferConditions { get; }

        public OpportunityDefinition(string id, string npcId, InteractionConditionSet offerConditions)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An opportunity id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException("An associated NPC id is required.", nameof(npcId));

            Id = id;
            NpcId = npcId;
            OfferConditions = offerConditions ?? new InteractionConditionSet();
        }

        public bool CanOffer(OpportunityState state, InteractionContext context)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return state.OpportunityId == Id && state.Phase == OpportunityPhase.Available &&
                OfferConditions.IsSatisfiedBy(context, NpcId);
        }
    }
}
