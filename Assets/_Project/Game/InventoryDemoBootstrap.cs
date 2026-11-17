using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Temporary scene driver for the inventory UI: builds a 6x4 grid, fills it with the assigned items
    /// and attaches an InventoryPanel. Replaced once the real player owns an inventory.
    /// </summary>
    public sealed class InventoryDemoBootstrap : MonoBehaviour
    {
        [SerializeField] private ItemDefinition[] startingItems;

        private GridInventory _inventory;

        private void Start()
        {
            _inventory = new GridInventory();

            if (startingItems != null)
            {
                foreach (ItemDefinition definition in startingItems)
                {
                    if (definition == null)
                        continue;

                    ItemType type = definition.ToItemType();
                    _inventory.Add(type, type.MaxStack);
                }
            }

            var panel = gameObject.AddComponent<InventoryPanel>();
            panel.Bind(_inventory, startingItems);
            panel.SetOpen(true);
        }
    }
}
