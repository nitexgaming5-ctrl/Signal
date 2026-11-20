using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    [CreateAssetMenu(menuName = "Signal/Resident Property Container", fileName = "ResidentPropertyContainer")]
    public sealed class ResidentPropertyContainerDefinition : ScriptableObject
    {
        [Serializable]
        private sealed class ContentEntry
        {
            public ItemDefinition item;
            [Min(1)] public int quantity = 1;
        }

        [SerializeField] private string containerId;
        [SerializeField] private string ownerNpcId;
        [SerializeField] private string locationId;
        [SerializeField] private string displayName;
        [SerializeField] private Vector2 interactionPosition;
        [SerializeField, Min(0f)] private float witnessRadius = 3f;
        [SerializeField, Min(0)] private int heatAward = 2;
        [SerializeField, Min(0)] private int progressionAward = 1;
        [SerializeField] private string actionId = "apartment-container-theft";
        [SerializeField, TextArea(2, 3)] private string witnessDescription =
            "Someone took something from the wardrobe.";
        [SerializeField] private ContentEntry[] contents;

        public string ContainerId => containerId;
        public string OwnerNpcId => ownerNpcId;
        public string LocationId => locationId;
        public string DisplayName => displayName;
        public Vector2 InteractionPosition => interactionPosition;
        public float WitnessRadius => witnessRadius;
        public int HeatAward => heatAward;
        public int ProgressionAward => progressionAward;
        public string ActionId => actionId;
        public string WitnessDescription => witnessDescription;

        public IEnumerable<ItemDefinition> Items
        {
            get
            {
                if (contents == null) yield break;
                foreach (ContentEntry entry in contents)
                    if (entry != null && entry.item != null)
                        yield return entry.item;
            }
        }

        public bool TryValidate(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(containerId) || string.IsNullOrWhiteSpace(ownerNpcId) ||
                string.IsNullOrWhiteSpace(locationId) || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(actionId) || string.IsNullOrWhiteSpace(witnessDescription))
            {
                error = "The property container requires stable container, owner, display and action IDs.";
                return false;
            }
            if (float.IsNaN(interactionPosition.x) || float.IsInfinity(interactionPosition.x) ||
                float.IsNaN(interactionPosition.y) || float.IsInfinity(interactionPosition.y) ||
                float.IsNaN(witnessRadius) || float.IsInfinity(witnessRadius) || witnessRadius < 0f ||
                heatAward < 0 || progressionAward < 0)
            {
                error = "The property container has invalid position, witness radius or crime tuning.";
                return false;
            }
            if (contents == null || contents.Length == 0)
            {
                error = "The property container has no authored contents.";
                return false;
            }

            for (int i = 0; i < contents.Length; i++)
            {
                ContentEntry entry = contents[i];
                if (entry == null || entry.item == null || entry.quantity < 1)
                {
                    error = "The property container has an invalid item entry.";
                    return false;
                }
                try { entry.item.ToItemType(); }
                catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
                {
                    error = "The property container references an invalid item definition: " + exception.Message;
                    return false;
                }
            }
            return true;
        }

        /// <summary>Creates fixed authored property contents once; unlike scavenging, no loot is rolled.</summary>
        public bool TryPopulate(SearchableContainer container, out string error)
        {
            error = null;
            if (container == null || container.ContainerId != containerId ||
                container.IsGenerated || container.Inventory.Items.Count != 0)
            {
                error = "The property container is unavailable or has already been populated.";
                return false;
            }
            if (!TryValidate(out error))
                return false;

            for (int i = 0; i < contents.Length; i++)
            {
                ContentEntry entry = contents[i];
                ItemType type = entry.item.ToItemType();
                if (container.Inventory.Add(type, entry.quantity, entry.item.DefaultInstanceState) > 0)
                {
                    container.Reset();
                    error = "The authored property contents do not fit in the container.";
                    return false;
                }
            }

            var order = new List<Guid>();
            foreach (PlacedItem item in container.Inventory.Items)
                order.Add(item.ItemInstanceId);

            if (!container.TryInitializeSearchOrder(order))
            {
                container.Reset();
                error = "The authored property contents could not be initialized.";
                return false;
            }
            return true;
        }
    }
}
