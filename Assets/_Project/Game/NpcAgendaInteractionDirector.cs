using System;
using System.Collections.Generic;
using Signal.Core;

namespace Signal.Game
{
    /// <summary>
    /// Deterministic runtime adapter for authored agenda content. It evaluates only the
    /// resident's own definition and explicit player/world predicates supplied by the session.
    /// </summary>
    public sealed class NpcAgendaInteractionDirector
    {
        private readonly Dictionary<string, NpcAgendaDefinition> _definitions =
            new Dictionary<string, NpcAgendaDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, NpcAgendaThreadDefinition[]> _orderedThreads =
            new Dictionary<string, NpcAgendaThreadDefinition[]>(StringComparer.Ordinal);
        private readonly NpcAgendaProgressBook _progress;
        private readonly PlayerDiscoveryLedger _discoveries;
        private readonly PlayerRelationshipBook _relationships;
        private readonly Func<string, bool> _isContractCompleted;
        private readonly Func<string, bool> _isUpgradeAcquired;
        private readonly Func<string, OpportunityPhase, bool> _isOpportunityInPhase;
        private readonly Func<WorkshopUpgradeDefinition, bool> _openWorkshopUpgradeOffer;
        private readonly Func<int> _getDay;
        private readonly Func<int> _getMinute;

        public NpcAgendaInteractionDirector(IEnumerable<NpcAgendaDefinition> definitions,
            NpcAgendaProgressBook progress, PlayerDiscoveryLedger discoveries,
            PlayerRelationshipBook relationships, Func<string, bool> isContractCompleted,
            Func<string, bool> isUpgradeAcquired,
            Func<string, OpportunityPhase, bool> isOpportunityInPhase,
            Func<WorkshopUpgradeDefinition, bool> openWorkshopUpgradeOffer,
            Func<int> getDay, Func<int> getMinute)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _discoveries = discoveries ?? throw new ArgumentNullException(nameof(discoveries));
            _relationships = relationships ?? throw new ArgumentNullException(nameof(relationships));
            _isContractCompleted = isContractCompleted ?? (_ => false);
            _isUpgradeAcquired = isUpgradeAcquired ?? (_ => false);
            _isOpportunityInPhase = isOpportunityInPhase ?? ((_, __) => false);
            _openWorkshopUpgradeOffer = openWorkshopUpgradeOffer ?? (_ => false);
            _getDay = getDay ?? (() => 1);
            _getMinute = getMinute ?? (() => 0);

            if (definitions == null) return;
            foreach (NpcAgendaDefinition definition in definitions)
            {
                if (definition == null) continue;
                if (!definition.TryValidate(out string error))
                    throw new ArgumentException("Invalid NPC agenda: " + error, nameof(definitions));
                if (_definitions.ContainsKey(definition.NpcId))
                    throw new ArgumentException("Duplicate NPC agenda for " + definition.NpcId + ".", nameof(definitions));
                _definitions.Add(definition.NpcId, definition);
                var orderedThreads = new List<NpcAgendaThreadDefinition>(definition.Threads);
                orderedThreads.Sort(CompareThreadPriority);
                _orderedThreads.Add(definition.NpcId, orderedThreads.ToArray());
            }
        }

        public bool HasAgenda(string npcId)
        {
            return !string.IsNullOrWhiteSpace(npcId) && _definitions.ContainsKey(npcId);
        }

        public bool TryGetNextStage(string npcId, NpcActivity activity,
            out NpcAgendaThreadDefinition thread, out NpcAgendaStageDefinition stage)
        {
            return TryGetNextStage(npcId, activity, null, out thread, out stage);
        }

        public bool TryGetNextStage(string npcId, NpcActivity activity, string place,
            out NpcAgendaThreadDefinition thread, out NpcAgendaStageDefinition stage)
        {
            thread = null;
            stage = null;
            if (string.IsNullOrWhiteSpace(npcId) ||
                !_orderedThreads.TryGetValue(npcId, out NpcAgendaThreadDefinition[] orderedThreads))
                return false;

            for (int t = 0; t < orderedThreads.Length; t++)
            {
                NpcAgendaThreadDefinition candidateThread = orderedThreads[t];
                for (int s = 0; s < candidateThread.Stages.Length; s++)
                {
                    NpcAgendaStageDefinition candidateStage = candidateThread.Stages[s];
                    if (_progress.IsStageCompleted(npcId, candidateThread.ThreadId, candidateStage.StageId))
                        continue;
                    if (!IsEligible(npcId, candidateStage, activity, place))
                        break;

                    thread = candidateThread;
                    stage = candidateStage;
                    return true;
                }
            }
            return false;
        }

        private static int CompareThreadPriority(NpcAgendaThreadDefinition left,
            NpcAgendaThreadDefinition right)
        {
            int priority = right.Priority.CompareTo(left.Priority);
            return priority != 0 ? priority : string.CompareOrdinal(left.ThreadId, right.ThreadId);
        }

        private bool IsEligible(string npcId, NpcAgendaStageDefinition stage, NpcActivity activity, string place)
        {
            if (stage.RequireSocializingActivity && activity != NpcActivity.Socialize)
                return false;
            if (stage.RequiredActivity != NpcAgendaActivityRequirement.Any &&
                !MatchesActivity(stage.RequiredActivity, activity))
                return false;
            if (!string.IsNullOrWhiteSpace(stage.RequiredPlace) && stage.RequiredPlace != place)
                return false;

            if (!string.IsNullOrWhiteSpace(stage.RequiredDiscoveryId) &&
                !_discoveries.HasSignal(stage.RequiredDiscoveryId))
                return false;

            if (stage.RequireRelationshipThresholds)
            {
                NpcRelationship relationship = _relationships.Get(npcId);
                if (relationship.Familiarity < stage.MinimumFamiliarity ||
                    relationship.Trust < stage.MinimumTrust ||
                    relationship.Affinity < stage.MinimumAffinity)
                    return false;
            }

            if (stage.RequireOpportunityPhase &&
                !_isOpportunityInPhase(stage.RequiredOpportunityId, stage.RequiredOpportunityPhase))
                return false;

            string contractId = stage.RequiredContract != null
                ? stage.RequiredContract.Id.ToString()
                : stage.RequiredCompletedContractId;
            if (!string.IsNullOrWhiteSpace(contractId) && !_isContractCompleted(contractId))
                return false;

            string upgradeId = stage.RequiredUpgrade != null
                ? stage.RequiredUpgrade.Id.ToString()
                : stage.RequiredUpgradeId;
            if (!string.IsNullOrWhiteSpace(upgradeId))
            {
                bool acquired = _isUpgradeAcquired(upgradeId);
                if (stage.RequireUpgradeNotAcquired ? acquired : !acquired)
                    return false;
            }
            return true;
        }

        private static bool MatchesActivity(NpcAgendaActivityRequirement requirement, NpcActivity activity)
        {
            switch (requirement)
            {
                case NpcAgendaActivityRequirement.Work: return activity == NpcActivity.Work;
                case NpcAgendaActivityRequirement.Rest: return activity == NpcActivity.Rest;
                case NpcAgendaActivityRequirement.Socialize: return activity == NpcActivity.Socialize;
                case NpcAgendaActivityRequirement.Home: return activity == NpcActivity.Home;
                default: return true;
            }
        }

        public bool TryChoose(string npcId, string threadId, string stageId, string choiceId,
            out string response, out bool stageCompleted)
        {
            response = null;
            stageCompleted = false;
            if (string.IsNullOrWhiteSpace(npcId) || string.IsNullOrWhiteSpace(threadId) ||
                string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(choiceId) ||
                !_definitions.TryGetValue(npcId, out NpcAgendaDefinition agenda))
                return false;

            NpcAgendaThreadDefinition thread = FindThread(agenda, threadId);
            NpcAgendaStageDefinition stage = FindStage(thread, stageId);
            NpcAgendaChoiceDefinition choice = FindChoice(stage, choiceId);
            if (choice == null || _progress.IsStageCompleted(npcId, threadId, stageId))
                return false;

            if (choice.Action == NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer &&
                !_openWorkshopUpgradeOffer(choice.WorkshopUpgradeOffer))
                return false;

            if (!choice.CompleteStageOnChoice)
            {
                response = choice.Response;
                return true;
            }

            if (!_progress.TryCompleteStage(npcId, threadId, stageId, choiceId))
                return false;

            if (choice.Action == NpcAgendaChoiceAction.RecordDiscovery)
                _discoveries.Record(new PlayerDiscoverySignal(choice.DiscoveryId,
                    choice.DiscoverySubjectId, choice.DiscoveryType, npcId,
                    choice.DiscoveryDescription, choice.DiscoveryConfidence, _getDay(), _getMinute()));

            if (choice.FamiliarityDelta != 0f || choice.TrustDelta != 0f || choice.AffinityDelta != 0f)
                _relationships.Adjust(npcId, choice.FamiliarityDelta,
                    choice.TrustDelta, choice.AffinityDelta);

            response = choice.Response;
            stageCompleted = true;
            return true;
        }

        private static NpcAgendaThreadDefinition FindThread(NpcAgendaDefinition agenda, string threadId)
        {
            if (agenda == null) return null;
            for (int i = 0; i < agenda.Threads.Length; i++)
                if (agenda.Threads[i].ThreadId == threadId) return agenda.Threads[i];
            return null;
        }

        private static NpcAgendaStageDefinition FindStage(NpcAgendaThreadDefinition thread, string stageId)
        {
            if (thread == null) return null;
            for (int i = 0; i < thread.Stages.Length; i++)
                if (thread.Stages[i].StageId == stageId) return thread.Stages[i];
            return null;
        }

        private static NpcAgendaChoiceDefinition FindChoice(NpcAgendaStageDefinition stage, string choiceId)
        {
            if (stage == null) return null;
            for (int i = 0; i < stage.Choices.Length; i++)
                if (stage.Choices[i].ChoiceId == choiceId) return stage.Choices[i];
            return null;
        }
    }
}
