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
        private readonly NpcAgendaProgressBook _progress;
        private readonly PlayerDiscoveryLedger _discoveries;
        private readonly PlayerRelationshipBook _relationships;
        private readonly Func<string, bool> _isContractCompleted;
        private readonly Func<string, bool> _isUpgradeAcquired;
        private readonly Func<bool> _openWorkshopUpgradeOffer;
        private readonly Func<int> _getDay;
        private readonly Func<int> _getMinute;

        public NpcAgendaInteractionDirector(IEnumerable<NpcAgendaDefinition> definitions,
            NpcAgendaProgressBook progress, PlayerDiscoveryLedger discoveries,
            PlayerRelationshipBook relationships, Func<string, bool> isContractCompleted,
            Func<string, bool> isUpgradeAcquired, Func<bool> openWorkshopUpgradeOffer,
            Func<int> getDay, Func<int> getMinute)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _discoveries = discoveries ?? throw new ArgumentNullException(nameof(discoveries));
            _relationships = relationships ?? throw new ArgumentNullException(nameof(relationships));
            _isContractCompleted = isContractCompleted ?? (_ => false);
            _isUpgradeAcquired = isUpgradeAcquired ?? (_ => false);
            _openWorkshopUpgradeOffer = openWorkshopUpgradeOffer ?? (() => false);
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
            }
        }

        public bool TryGetNextStage(string npcId, NpcActivity activity,
            out NpcAgendaThreadDefinition thread, out NpcAgendaStageDefinition stage)
        {
            thread = null;
            stage = null;
            if (string.IsNullOrWhiteSpace(npcId) || !_definitions.TryGetValue(npcId, out NpcAgendaDefinition agenda))
                return false;

            for (int t = 0; t < agenda.Threads.Length; t++)
            {
                NpcAgendaThreadDefinition candidateThread = agenda.Threads[t];
                for (int s = 0; s < candidateThread.Stages.Length; s++)
                {
                    NpcAgendaStageDefinition candidateStage = candidateThread.Stages[s];
                    if (_progress.IsStageCompleted(npcId, candidateThread.ThreadId, candidateStage.StageId))
                        continue;
                    if (candidateStage.RequireSocializingActivity && activity != NpcActivity.Socialize)
                        break;
                    if (!string.IsNullOrWhiteSpace(candidateStage.RequiredDiscoveryId) &&
                        !_discoveries.HasSignal(candidateStage.RequiredDiscoveryId))
                        break;
                    if (!string.IsNullOrWhiteSpace(candidateStage.RequiredCompletedContractId) &&
                        !_isContractCompleted(candidateStage.RequiredCompletedContractId))
                        break;
                    if (!string.IsNullOrWhiteSpace(candidateStage.RequiredUpgradeId) &&
                        !_isUpgradeAcquired(candidateStage.RequiredUpgradeId))
                        break;

                    thread = candidateThread;
                    stage = candidateStage;
                    return true;
                }
            }
            return false;
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
                !_openWorkshopUpgradeOffer())
                return false;

            if (!choice.CompleteStageOnChoice)
            {
                response = choice.Response;
                return true;
            }

            if (!_progress.TryCompleteStage(npcId, threadId, stageId, choiceId))
                return false;

            switch (choice.Action)
            {
                case NpcAgendaChoiceAction.RecordDiscovery:
                    _discoveries.Record(new PlayerDiscoverySignal(choice.DiscoveryId,
                        choice.DiscoverySubjectId, choice.DiscoveryType, npcId,
                        choice.DiscoveryDescription, choice.DiscoveryConfidence, _getDay(), _getMinute()));
                    break;
                case NpcAgendaChoiceAction.AdjustRelationship:
                    _relationships.Adjust(npcId, choice.FamiliarityDelta,
                        choice.TrustDelta, choice.AffinityDelta);
                    break;
            }

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
