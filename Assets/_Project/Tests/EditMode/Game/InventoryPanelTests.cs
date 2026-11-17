using NUnit.Framework;
using Signal.Core;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class InventoryPanelTests
    {
        [Test]
        public void BindBuildsTheCanvasAndStartsClosed()
        {
            var host = new GameObject("InventoryPanelHost");
            var definition = ScriptableObject.CreateInstance<ItemDefinition>();

            try
            {
                var grid = new GridInventory();
                grid.Add(definition.ToItemType(), 1);

                var panel = host.AddComponent<InventoryPanel>();
                panel.Bind(grid, new[] { definition });

                Assert.That(panel.IsOpen, Is.False);
                Assert.That(host.transform.childCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }
    }
}
