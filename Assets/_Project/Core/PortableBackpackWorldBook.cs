using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum PortableBackpackWorldStatus { Equipped = 0, Dropped = 1 }

    public readonly struct PortableBackpackWorldState
    {
        public PortableBackpackWorldState(Guid backpackInstanceId, PortableBackpackWorldStatus status, float worldX, float worldY)
        { BackpackInstanceId = backpackInstanceId; Status = status; WorldX = worldX; WorldY = worldY; }
        public Guid BackpackInstanceId { get; }
        public PortableBackpackWorldStatus Status { get; }
        public float WorldX { get; }
        public float WorldY { get; }
    }

    /// <summary>Tracks a backpack's physical state while its contents remain keyed to the same item identity.</summary>
    public sealed class PortableBackpackWorldBook
    {
        private readonly Dictionary<Guid, PortableBackpackWorldState> _states = new Dictionary<Guid, PortableBackpackWorldState>();
        public int Count => _states.Count;

        public bool TryRegisterEquipped(Guid id, PortableStorageBook storage)
        {
            if (id == Guid.Empty || storage == null || !storage.TryGetContents(id, out _) || _states.ContainsKey(id)) return false;
            _states.Add(id, new PortableBackpackWorldState(id, PortableBackpackWorldStatus.Equipped, 0f, 0f));
            return true;
        }

        public bool TryDrop(Guid id, float x, float y)
        {
            if (!IsFinite(x) || !IsFinite(y) || !_states.TryGetValue(id, out PortableBackpackWorldState current) ||
                current.Status != PortableBackpackWorldStatus.Equipped) return false;
            _states[id] = new PortableBackpackWorldState(id, PortableBackpackWorldStatus.Dropped, x, y);
            return true;
        }

        public bool TryRecover(Guid id)
        {
            if (!_states.TryGetValue(id, out PortableBackpackWorldState current) ||
                current.Status != PortableBackpackWorldStatus.Dropped) return false;
            _states[id] = new PortableBackpackWorldState(id, PortableBackpackWorldStatus.Equipped, 0f, 0f);
            return true;
        }

        public bool TryGet(Guid id, out PortableBackpackWorldState state)
        {
            if (id == Guid.Empty) { state = default; return false; }
            return _states.TryGetValue(id, out state);
        }

        public IReadOnlyList<PortableBackpackWorldState> GetAll() =>
            new List<PortableBackpackWorldState>(_states.Values).AsReadOnly();

        public void Clear() => _states.Clear();

        public bool TryRestore(Guid id, PortableBackpackWorldStatus status, float x, float y, PortableStorageBook storage)
        {
            if (id == Guid.Empty || storage == null || !Enum.IsDefined(typeof(PortableBackpackWorldStatus), status) ||
                !storage.TryGetContents(id, out _) || _states.ContainsKey(id) || !IsFinite(x) || !IsFinite(y)) return false;
            _states.Add(id, new PortableBackpackWorldState(id, status,
                status == PortableBackpackWorldStatus.Dropped ? x : 0f,
                status == PortableBackpackWorldStatus.Dropped ? y : 0f));
            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public static class PortableBackpackWorldSaveMapper
    {
        public static void Capture(PortableBackpackWorldBook source, SaveData destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.portableBackpacks.Clear();
            if (source == null) return;
            foreach (PortableBackpackWorldState state in source.GetAll())
                destination.portableBackpacks.Add(new SavedPortableBackpack
                {
                    instanceId = state.BackpackInstanceId.ToString(), status = (int)state.Status,
                    worldX = state.WorldX, worldY = state.WorldY
                });
        }

        public static int Apply(SaveData source, PortableBackpackWorldBook destination, PortableStorageBook storage)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();
            if (source == null || source.portableBackpacks == null) return 0;
            int skipped = 0;
            var seen = new HashSet<Guid>();
            foreach (SavedPortableBackpack saved in source.portableBackpacks)
            {
                if (saved == null || !Guid.TryParse(saved.instanceId, out Guid id) || !seen.Add(id) ||
                    !Enum.IsDefined(typeof(PortableBackpackWorldStatus), saved.status) ||
                    !destination.TryRestore(id, (PortableBackpackWorldStatus)saved.status, saved.worldX, saved.worldY, storage))
                    skipped++;
            }
            return skipped;
        }
    }
}
