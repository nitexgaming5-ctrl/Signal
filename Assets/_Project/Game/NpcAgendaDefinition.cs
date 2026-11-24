using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public enum NpcAgendaChoiceAction
    {
        DialogueOnly = 0,
        RecordDiscovery = 1,
        AdjustRelationship = 2,
        OpenWorkshopUpgradeOffer = 3
    }

    public enum NpcAgendaActivityRequirement
    {
        Any = 0,
        Work = 1,
        Rest = 2,
        Socialize = 3,
        Home = 4
    }

    [Serializable]
    public sealed class NpcAgendaChoiceDefinition
    {
        public string ChoiceId;
        public string Label;
        [TextArea(2, 5)] public string Response;
        public NpcAgendaChoiceAction Action;
        [Tooltip("False keeps a revisitable offer/dialogue choice from consuming its stage.")]
        public bool CompleteStageOnChoice = true;

        [Header("Discovery outcome")]
        public string DiscoveryId;
        public string DiscoverySubjectId;
        public DiscoverySignalType DiscoveryType = DiscoverySignalType.Conversation;
        [TextArea] public string DiscoveryDescription;
        [Range(0f, 1f)] public float DiscoveryConfidence = 1f;

        [Header("Relationship outcome")]
        [Range(-1f, 1f)] public float FamiliarityDelta;
        [Range(-1f, 1f)] public float TrustDelta;
        [Range(-1f, 1f)] public float AffinityDelta;

        [Header("Workshop offer outcome")]
        public WorkshopUpgradeDefinition WorkshopUpgradeOffer;
    }

    [Serializable]
    public sealed class NpcAgendaStageDefinition
    {
        public string StageId;
        public string Title;
        [TextArea(3, 8)] public string Dialogue;
        public bool RequireSocializingActivity;
        public NpcAgendaActivityRequirement RequiredActivity = NpcAgendaActivityRequirement.Any;
        public string RequiredPlace;
        public string RequiredDiscoveryId;

        [Header("Relationship gates")]
        public bool RequireRelationshipThresholds;
        [Range(0f, 1f)] public float MinimumFamiliarity;
        [Range(0f, 1f)] public float MinimumTrust;
        [Range(-1f, 1f)] public float MinimumAffinity = -1f;

        [Header("Opportunity gate")]
        public bool RequireOpportunityPhase;
        public string RequiredOpportunityId;
        public OpportunityPhase RequiredOpportunityPhase;

        [Header("Contract and upgrade gates")]
        [Tooltip("Optional completed customer contract ID required before this stage is available.")]
        public string RequiredCompletedContractId;
        public CustomerContractDefinition RequiredContract;
        [Tooltip("Optional acquired workshop upgrade required before this stage is available.")]
        public string RequiredUpgradeId;
        public WorkshopUpgradeDefinition RequiredUpgrade;
        public bool RequireUpgradeNotAcquired;

        [Header("Legacy discovery migration")]
        public bool CompleteIfLegacyDiscoveryExists;
        public string LegacyDiscoverySubjectId;
        public string LegacyDiscoverySourceId;
        public DiscoverySignalType LegacyDiscoveryType = DiscoverySignalType.Rumour;

        public NpcAgendaChoiceDefinition[] Choices = Array.Empty<NpcAgendaChoiceDefinition>();
    }

    [Serializable]
    public sealed class NpcAgendaThreadDefinition
    {
        public string ThreadId;
        public string DisplayName;
        [TextArea] public string Summary;
        [Min(0)] public int Priority;
        public NpcAgendaStageDefinition[] Stages = Array.Empty<NpcAgendaStageDefinition>();
    }

    /// <summary>
    /// Authored agenda content for one resident. Progress is stored separately by stable
    /// NPC/thread/stage/choice IDs; this asset contains no runtime state.
    /// </summary>
    [CreateAssetMenu(menuName = "Signal/NPC Agenda Definition", fileName = "NPC_Agenda")]
    public sealed class NpcAgendaDefinition : ScriptableObject
    {
        public string NpcId;
        public NpcAgendaThreadDefinition[] Threads = Array.Empty<NpcAgendaThreadDefinition>();

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(NpcId))
            {
                error = "Agenda NPC ID is required.";
                return false;
            }
            if (Threads == null || Threads.Length == 0)
            {
                error = "An NPC agenda must contain at least one thread.";
                return false;
            }

            var threadIds = new HashSet<string>(StringComparer.Ordinal);
            for (int t = 0; t < Threads.Length; t++)
            {
                NpcAgendaThreadDefinition thread = Threads[t];
                if (thread == null || string.IsNullOrWhiteSpace(thread.ThreadId) ||
                    string.IsNullOrWhiteSpace(thread.DisplayName) || thread.Priority < 0 ||
                    !threadIds.Add(thread.ThreadId))
                {
                    error = "Agenda thread IDs, display names and priorities must be valid and unique for NPC " + NpcId + ".";
                    return false;
                }
                if (thread.Stages == null || thread.Stages.Length == 0)
                {
                    error = "Thread " + thread.ThreadId + " must contain at least one stage.";
                    return false;
                }

                var stageIds = new HashSet<string>(StringComparer.Ordinal);
                for (int s = 0; s < thread.Stages.Length; s++)
                {
                    NpcAgendaStageDefinition stage = thread.Stages[s];
                    if (stage == null || string.IsNullOrWhiteSpace(stage.StageId) ||
                        !stageIds.Add(stage.StageId) || string.IsNullOrWhiteSpace(stage.Dialogue))
                    {
                        error = "Stage IDs and dialogue must be present and stage IDs unique in thread " +
                            thread.ThreadId + ".";
                        return false;
                    }
                    if (!Enum.IsDefined(typeof(NpcAgendaActivityRequirement), stage.RequiredActivity))
                    {
                        error = "Stage " + stage.StageId + " has an invalid activity requirement.";
                        return false;
                    }
                    if (float.IsNaN(stage.MinimumFamiliarity) || float.IsInfinity(stage.MinimumFamiliarity) ||
                        stage.MinimumFamiliarity < 0f || stage.MinimumFamiliarity > 1f ||
                        float.IsNaN(stage.MinimumTrust) || float.IsInfinity(stage.MinimumTrust) ||
                        stage.MinimumTrust < 0f || stage.MinimumTrust > 1f ||
                        float.IsNaN(stage.MinimumAffinity) || float.IsInfinity(stage.MinimumAffinity) ||
                        stage.MinimumAffinity < -1f || stage.MinimumAffinity > 1f)
                    {
                        error = "Stage " + stage.StageId + " has invalid relationship thresholds.";
                        return false;
                    }
                    if (stage.RequireOpportunityPhase &&
                        (string.IsNullOrWhiteSpace(stage.RequiredOpportunityId) ||
                         !Enum.IsDefined(typeof(OpportunityPhase), stage.RequiredOpportunityPhase)))
                    {
                        error = "Stage " + stage.StageId + " has an invalid opportunity phase requirement.";
                        return false;
                    }
                    if (stage.RequiredContract != null && !stage.RequiredContract.TryValidate(out _))
                    {
                        error = "Stage " + stage.StageId + " references an invalid customer contract.";
                        return false;
                    }
                    if (stage.RequiredUpgrade != null && !stage.RequiredUpgrade.TryValidate(out _))
                    {
                        error = "Stage " + stage.StageId + " references an invalid workshop upgrade.";
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(stage.RequiredCompletedContractId) && stage.RequiredContract != null &&
                        stage.RequiredCompletedContractId != stage.RequiredContract.Id.ToString())
                    {
                        error = "Stage " + stage.StageId + " has conflicting contract identity requirements.";
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(stage.RequiredUpgradeId) && stage.RequiredUpgrade != null &&
                        stage.RequiredUpgradeId != stage.RequiredUpgrade.Id.ToString())
                    {
                        error = "Stage " + stage.StageId + " has conflicting workshop upgrade identity requirements.";
                        return false;
                    }
                    if (stage.RequireUpgradeNotAcquired &&
                        string.IsNullOrWhiteSpace(stage.RequiredUpgradeId) && stage.RequiredUpgrade == null)
                    {
                        error = "Stage " + stage.StageId + " requires an upgrade ID for its not-acquired condition.";
                        return false;
                    }
                    if (stage.CompleteIfLegacyDiscoveryExists &&
                        (string.IsNullOrWhiteSpace(stage.LegacyDiscoverySubjectId) ||
                         string.IsNullOrWhiteSpace(stage.LegacyDiscoverySourceId) ||
                         !Enum.IsDefined(typeof(DiscoverySignalType), stage.LegacyDiscoveryType)))
                    {
                        error = "Stage " + stage.StageId + " has an invalid legacy discovery migration rule.";
                        return false;
                    }

                    if (stage.Choices == null || stage.Choices.Length == 0)
                    {
                        error = "Stage " + stage.StageId + " must contain at least one choice.";
                        return false;
                    }

                    var choiceIds = new HashSet<string>(StringComparer.Ordinal);
                    for (int c = 0; c < stage.Choices.Length; c++)
                    {
                        NpcAgendaChoiceDefinition choice = stage.Choices[c];
                        if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId) ||
                            !choiceIds.Add(choice.ChoiceId) || string.IsNullOrWhiteSpace(choice.Label) ||
                            string.IsNullOrWhiteSpace(choice.Response) ||
                            !Enum.IsDefined(typeof(NpcAgendaChoiceAction), choice.Action))
                        {
                            error = "Choice IDs, labels, responses and actions must be valid and unique in stage " +
                                stage.StageId + ".";
                            return false;
                        }
                        if (choice.Action == NpcAgendaChoiceAction.RecordDiscovery &&
                            (string.IsNullOrWhiteSpace(choice.DiscoveryId) ||
                             string.IsNullOrWhiteSpace(choice.DiscoverySubjectId) ||
                             string.IsNullOrWhiteSpace(choice.DiscoveryDescription) ||
                             !Enum.IsDefined(typeof(DiscoverySignalType), choice.DiscoveryType) ||
                             float.IsNaN(choice.DiscoveryConfidence) || float.IsInfinity(choice.DiscoveryConfidence) ||
                             choice.DiscoveryConfidence < 0f || choice.DiscoveryConfidence > 1f))
                        {
                            error = "Discovery choice " + choice.ChoiceId + " is missing valid authored discovery data.";
                            return false;
                        }
                        if (choice.Action == NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer &&
                            (choice.WorkshopUpgradeOffer == null ||
                             !choice.WorkshopUpgradeOffer.TryValidate(out _)))
                        {
                            error = "Workshop offer choice " + choice.ChoiceId + " must reference a valid upgrade.";
                            return false;
                        }
                        if (float.IsNaN(choice.FamiliarityDelta) || float.IsInfinity(choice.FamiliarityDelta) ||
                            float.IsNaN(choice.TrustDelta) || float.IsInfinity(choice.TrustDelta) ||
                            float.IsNaN(choice.AffinityDelta) || float.IsInfinity(choice.AffinityDelta))
                        {
                            error = "Relationship outcomes must be finite in choice " + choice.ChoiceId + ".";
                            return false;
                        }
                        if (!choice.CompleteStageOnChoice &&
                            choice.Action != NpcAgendaChoiceAction.DialogueOnly &&
                            choice.Action != NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer)
                        {
                            error = "Repeatable choice " + choice.ChoiceId +
                                " cannot apply a one-time discovery or relationship outcome.";
                            return false;
                        }
                    }
                }
            }

            error = null;
            return true;
        }
    }
}
