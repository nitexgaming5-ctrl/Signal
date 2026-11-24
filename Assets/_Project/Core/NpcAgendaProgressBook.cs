using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Durable player-facing progress through authored NPC agenda stages.
    /// Content definitions remain in Signal.Game; this book stores only stable IDs
    /// and the choice made, never Unity objects or presentation state.
    /// </summary>
    public sealed class NpcAgendaProgressBook
    {
        private readonly Dictionary<string, NpcAgendaStageProgress> _stages =
            new Dictionary<string, NpcAgendaStageProgress>(StringComparer.Ordinal);

        public event Action<NpcAgendaProgressBook> Changed;
        public int CompletedStageCount => _stages.Count;

        public IReadOnlyList<NpcAgendaStageProgress> CompletedStages
        {
            get
            {
                var result = new List<NpcAgendaStageProgress>(_stages.Values);
                result.Sort((left, right) => string.CompareOrdinal(left.ProgressId, right.ProgressId));
                return result.AsReadOnly();
            }
        }

        public bool IsStageCompleted(string npcId, string threadId, string stageId)
        {
            return _stages.ContainsKey(BuildProgressId(npcId, threadId, stageId));
        }

        public bool TryGetChoice(string npcId, string threadId, string stageId, out string choiceId)
        {
            if (_stages.TryGetValue(BuildProgressId(npcId, threadId, stageId),
                out NpcAgendaStageProgress progress))
            {
                choiceId = progress.ChoiceId;
                return true;
            }

            choiceId = null;
            return false;
        }

        /// <summary>
        /// Completes a stage exactly once. Replaying the same interaction after closing
        /// a panel or loading a save cannot apply its authored outcome a second time.
        /// A new authored occurrence requires a new stable stage ID.
        /// </summary>
        public bool TryCompleteStage(string npcId, string threadId, string stageId, string choiceId)
        {
            string progressId = BuildProgressId(npcId, threadId, stageId);
            if (string.IsNullOrWhiteSpace(choiceId))
                throw new ArgumentException("A choice ID is required to complete an agenda stage.", nameof(choiceId));
            if (_stages.ContainsKey(progressId))
                return false;

            _stages.Add(progressId, new NpcAgendaStageProgress(npcId, threadId, stageId, choiceId));
            Changed?.Invoke(this);
            return true;
        }

        public void Clear()
        {
            if (_stages.Count == 0)
                return;
            _stages.Clear();
            Changed?.Invoke(this);
        }

        /// <summary>
        /// Restores one durable stage record. Invalid or duplicate records are rejected
        /// so a malformed save cannot overwrite an earlier player choice.
        /// </summary>
        public bool TryRestore(NpcAgendaStageProgress progress)
        {
            string id = BuildProgressId(progress.NpcId, progress.ThreadId, progress.StageId);
            if (string.IsNullOrWhiteSpace(progress.ChoiceId) || _stages.ContainsKey(id))
                return false;
            _stages.Add(id, progress);
            return true;
        }

        private static string BuildProgressId(string npcId, string threadId, string stageId)
        {
            ValidateId(npcId, nameof(npcId));
            ValidateId(threadId, nameof(threadId));
            ValidateId(stageId, nameof(stageId));
            return npcId.Length + ":" + npcId + threadId.Length + ":" + threadId +
                stageId.Length + ":" + stageId;
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Stable agenda IDs cannot be empty.", name);
        }
    }

    public readonly struct NpcAgendaStageProgress
    {
        public string NpcId { get; }
        public string ThreadId { get; }
        public string StageId { get; }
        public string ChoiceId { get; }
        public string ProgressId =>
            NpcId.Length + ":" + NpcId + ThreadId.Length + ":" + ThreadId +
            StageId.Length + ":" + StageId;

        public NpcAgendaStageProgress(string npcId, string threadId, string stageId, string choiceId)
        {
            if (string.IsNullOrWhiteSpace(npcId)) throw new ArgumentException("NPC ID is required.", nameof(npcId));
            if (string.IsNullOrWhiteSpace(threadId)) throw new ArgumentException("Thread ID is required.", nameof(threadId));
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage ID is required.", nameof(stageId));
            if (string.IsNullOrWhiteSpace(choiceId)) throw new ArgumentException("Choice ID is required.", nameof(choiceId));
            NpcId = npcId;
            ThreadId = threadId;
            StageId = stageId;
            ChoiceId = choiceId;
        }
    }
}
