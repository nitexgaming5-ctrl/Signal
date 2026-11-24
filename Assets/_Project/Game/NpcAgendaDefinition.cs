using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public enum NpcAgendaChoiceAction
    {
        DialogueOnly,
        RecordDiscovery,
        AdjustRelationship,
        OpenWorkshopUpgradeOffer
    }

    [Serializable]
    public sealed class NpcAgendaChoiceDefinition
    {
        public string ChoiceId;
        public string Label;
        [TextArea(2, 5)] public string Response;
        public NpcAgendaChoiceAction Action;
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
    }

    [Serializable]
    public sealed class NpcAgendaStageDefinition
    {
        public string StageId;
        public string Title;
        [TextArea(3, 8)] public string Dialogue;
        public bool RequireSocializingActivity;
        [Tooltip("Optional player discovery required before this stage is available.")]
        public string RequiredDiscoveryId;
        [Tooltip("Optional completed customer contract ID required before this stage is available.")]
        public string RequiredCompletedContractId;
        [Tooltip("Optional acquired workshop upgrade ID required before this stage is available.")]
        public string RequiredUpgradeId;
        public NpcAgendaChoiceDefinition[] Choices = Array.Empty<NpcAgendaChoiceDefinition>();
    }

    [Serializable]
    public sealed class NpcAgendaThreadDefinition
    {
        public string ThreadId;
        public string DisplayName;
        [TextArea] public string Summary;
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
            if (Threads == null)
            {
                error = "Agenda thread collection is missing.";
                return false;
            }

            var threadIds = new HashSet<string>(StringComparer.Ordinal);
            for (int t = 0; t < Threads.Length; t++)
            {
                NpcAgendaThreadDefinition thread = Threads[t];
                if (thread == null || string.IsNullOrWhiteSpace(thread.ThreadId) ||
                    !threadIds.Add(thread.ThreadId))
                {
                    error = "Agenda thread IDs must be present and unique for NPC " + NpcId + ".";
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
                            !choiceIds.Add(choice.ChoiceId) || string.IsNullOrWhiteSpace(choice.Label))
                        {
                            error = "Choice IDs and labels must be present and unique in stage " + stage.StageId + ".";
                            return false;
                        }
                        if (choice.Action == NpcAgendaChoiceAction.RecordDiscovery &&
                            (string.IsNullOrWhiteSpace(choice.DiscoveryId) ||
                             string.IsNullOrWhiteSpace(choice.DiscoverySubjectId) ||
                             string.IsNullOrWhiteSpace(choice.DiscoveryDescription)))
                        {
                            error = "Discovery choice " + choice.ChoiceId + " is missing its authored discovery data.";
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
