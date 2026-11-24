using System;
using System.Collections.Generic;
using Signal.Core;

namespace Signal.Game
{
    public readonly struct NpcAgendaInteraction
    {
        public string NpcId { get; }
        public string ThreadId { get; }
        public NpcAgendaStageDefinition Stage { get; }

        public NpcAgendaInteraction(string npcId, string threadId, NpcAgendaStageDefinition stage)
        {
            NpcId = npcId;
            ThreadId = threadId;
            Stage = stage;
        }
    }

    public readonly struct NpcAgendaChoiceResult
    {
        public string Response { get; }
        public WorkshopUpgradeDefinition WorkshopOffer { get; }

        public NpcAgendaChoiceResult(string response, WorkshopUpgradeDefinition workshopOffer)
        {
            Response = response;
            WorkshopOffer = workshopOffer;
        }
    }

    /// <summary>
    /// Selects authored NPC agenda stages and applies their typed, one-time outcomes.
    /// Economy transactions remain in their existing services and panels.
    /// </summary>
    public sealed class NpcAgendaDirector
    {
        private readonly Dictionary<string, NpcAgendaDefinition> _agendas =
            new Dictionary<string, NpcAgendaDefinition>(StringComparer.Ordinal);
        private NpcAgendaProgressBook _progress;
        private PlayerDiscoveryLedger _discoveries;
        private PlayerRelationshipBook _relationships;
        private GameClock _clock;
        private Func<string, bool> _contractCompleted;
        private Func<string, bool> _upgradeAcquired;

        public void Bind(NpcAgendaDefinition[] definitions, NpcAgendaProgressBook progress,
            PlayerDiscoveryLedger discoveries, PlayerRelationshipBook relationships, GameClock clock,
            Func<string, bool> contractCompleted, Func<string, bool> upgradeAcquired)
        {
            if (progress == null || discoveries == null || relationships == null || clock == null ||
                contractCompleted == null || upgradeAcquired == null)
                throw new ArgumentNullException("NPC agenda dependencies must all be provided.");

            _agendas.Clear();
            _progress = progress;
            _discoveries = discoveries;
            _relationships = relationships;
            _clock = clock;
            _contractCompleted = contractCompleted;
            _upgradeAcquired = upgradeAcquired;

            if (definitions == null) return;
            foreach (NpcAgendaDefinition definition in definitions)
            {
                if (definition == null) continue;
                if (!definition.TryValidate(out string error))
                    throw new ArgumentException("Invalid NPC agenda '" + definition.name + "': " + error);
                if (_agendas.ContainsKey(definition.NpcId))
                    throw new ArgumentException("Duplicate NPC agenda for resident '" + definition.NpcId + "'.");
                _agendas.Add(definition.NpcId, definition);
            }
        }

        public bool TryGetAvailableStage(string npcId, NpcActivity activity, string place,
            out NpcAgendaInteraction interaction)
        {
            interaction = default;
            if (_progress == null ||
                !_agendas.TryGetValue(npcId ?? string.Empty, out NpcAgendaDefinition agenda))
                return false;

            NpcAgendaThreadDefinition selectedThread = null;
            NpcAgendaStageDefinition selectedStage = null;
            foreach (NpcAgendaThreadDefinition thread in agenda.Threads)
            {
                foreach (NpcAgendaStageDefinition stage in thread.Stages)
                {
                    if (!IsEligible(agenda.NpcId, thread.ThreadId, stage, activity, place))
                        continue;

                    if (selectedThread == null || thread.Priority > selectedThread.Priority ||
                        (thread.Priority == selectedThread.Priority &&
                         string.CompareOrdinal(thread.ThreadId, selectedThread.ThreadId) < 0))
                    {
                        selectedThread = thread;
                        selectedStage = stage;
                    }
                }
            }

            if (selectedStage == null) return false;
            interaction = new NpcAgendaInteraction(agenda.NpcId, selectedThread.ThreadId, selectedStage);
            return true;
        }

        public bool TrySelectChoice(NpcAgendaInteraction interaction, string choiceId,
            out NpcAgendaChoiceResult result)
        {
            result = default;
            if (_progress == null ||
                !_agendas.TryGetValue(interaction.NpcId ?? string.Empty, out NpcAgendaDefinition agenda) ||
                interaction.Stage == null)
                return false;

            NpcAgendaThreadDefinition thread = FindThread(agenda, interaction.ThreadId);
            if (thread == null) return false;
            NpcAgendaStageDefinition stage = FindStage(thread, interaction.Stage.StageId);
            if (stage == null || _progress.IsStageCompleted(agenda.NpcId, thread.ThreadId, stage.StageId))
                return false;

            NpcAgendaChoiceDefinition choice = null;
            foreach (NpcAgendaChoiceDefinition candidate in stage.Choices)
                if (candidate != null && candidate.ChoiceId == choiceId)
                {
                    choice = candidate;
                    break;
                }
            if (choice == null) return false;

            WorkshopUpgradeDefinition offer = null;
            switch (choice.Action)
            {
                case NpcAgendaChoiceAction.RecordDiscovery:
                    _discoveries.Record(new PlayerDiscoverySignal(choice.DiscoveryId,
                        choice.DiscoverySubjectId, choice.DiscoveryType, agenda.NpcId,
                        choice.DiscoveryDescription, choice.DiscoveryConfidence,
                        _clock.Day, _clock.MinuteOfDay));
                    AdjustRelationship(agenda.NpcId, choice);
                    break;
                case NpcAgendaChoiceAction.AdjustRelationship:
                    AdjustRelationship(agenda.NpcId, choice);
                    break;
                case NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer:
                    offer = choice.WorkshopUpgradeOffer;
                    break;
                case NpcAgendaChoiceAction.DialogueOnly:
                    break;
                default:
                    return false;
            }

            if (choice.CompleteStageOnSelect &&
                !_progress.TryCompleteStage(agenda.NpcId, thread.ThreadId, stage.StageId, choice.ChoiceId))
                return false;

            result = new NpcAgendaChoiceResult(choice.Response, offer);
            return true;
        }

        private bool IsEligible(string npcId, string threadId, NpcAgendaStageDefinition stage,
            NpcActivity activity, string place)
        {
            if (stage == null || _progress.IsStageCompleted(npcId, threadId, stage.StageId))
                return false;

            if (stage.RequireSocializingActivity && activity != NpcActivity.Socialize)
                return false;
            switch (stage.RequiredActivity)
            {
                case NpcAgendaActivityRequirement.Any:
                    break;
                case NpcAgendaActivityRequirement.Work:
                    if (activity != NpcActivity.Work) return false;
                    break;
                case NpcAgendaActivityRequirement.Rest:
                    if (activity != NpcActivity.Rest) return false;
                    break;
                case NpcAgendaActivityRequirement.Socialize:
                    if (activity != NpcActivity.Socialize) return false;
                    break;
                case NpcAgendaActivityRequirement.Home:
                    if (activity != NpcActivity.Home) return false;
                    break;
                default:
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(stage.RequiredPlace) && stage.RequiredPlace != place)
                return false;

            string contractId = !string.IsNullOrWhiteSpace(stage.RequiredCompletedContractId)
                ? stage.RequiredCompletedContractId
                : stage.RequiredContract != null ? stage.RequiredContract.Id.ToString() : null;
            if (!string.IsNullOrWhiteSpace(contractId) && !_contractCompleted(contractId))
                return false;

            string upgradeId = !string.IsNullOrWhiteSpace(stage.RequiredUpgradeId)
                ? stage.RequiredUpgradeId
                : stage.RequiredUpgrade != null ? stage.RequiredUpgrade.Id.ToString() : null;
            if (!string.IsNullOrWhiteSpace(upgradeId))
            {
                bool acquired = _upgradeAcquired(upgradeId);
                if (stage.RequireUpgradeNotAcquired && acquired) return false;
                if (!stage.RequireUpgradeNotAcquired && !acquired) return false;
            }
            return string.IsNullOrWhiteSpace(stage.RequiredDiscoveryId) ||
                _discoveries.TryGet(stage.RequiredDiscoveryId, out _);
        }

        private void AdjustRelationship(string npcId, NpcAgendaChoiceDefinition choice)
        {
            if (choice.FamiliarityDelta == 0f && choice.TrustDelta == 0f && choice.AffinityDelta == 0f)
                return;
            _relationships.Adjust(npcId, choice.FamiliarityDelta, choice.TrustDelta, choice.AffinityDelta);
        }

        private static NpcAgendaThreadDefinition FindThread(NpcAgendaDefinition agenda, string threadId)
        {
            foreach (NpcAgendaThreadDefinition thread in agenda.Threads)
                if (thread != null && thread.ThreadId == threadId)
                    return thread;
            return null;
        }

        private static NpcAgendaStageDefinition FindStage(NpcAgendaThreadDefinition thread, string stageId)
        {
            foreach (NpcAgendaStageDefinition stage in thread.Stages)
                if (stage != null && stage.StageId == stageId)
                    return stage;
            return null;
        }
    }
}
