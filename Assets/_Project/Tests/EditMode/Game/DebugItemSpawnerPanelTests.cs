using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Signal.Game.Tests
{
    public sealed class DebugItemSpawnerPanelTests
    {
        [TearDown]
        public void TearDown()
        {
            if (EventSystem.current != null)
                UnityEngine.Object.DestroyImmediate(EventSystem.current.gameObject);
        }

        [Test]
        public void BindBuildsCatalogueAndStartsClosed()
        {
            var host = new GameObject("DebugItemSpawnerHost");
            ItemDefinition item = CreateDefinition("Test Wool", "Materials");
            try
            {
                var panel = host.AddComponent<DebugItemSpawnerPanel>();
                panel.Bind(new[] { item }, _ => "Added.");

                Assert.That(panel.IsOpen, Is.False);
                Assert.That(panel.ItemCount, Is.EqualTo(1));
                panel.SetOpen(true);
                Assert.That(panel.IsOpen, Is.True);
                panel.SetOpen(false);
                Assert.That(panel.IsOpen, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void SpawnUsesTheSessionCallbackForSelectedDefinition()
        {
            var host = new GameObject("DebugItemSpawnerHost");
            ItemDefinition item = CreateDefinition("Work Jacket", "Clothing");
            ItemDefinition spawned = null;
            try
            {
                var panel = host.AddComponent<DebugItemSpawnerPanel>();
                panel.Bind(new[] { item }, selected =>
                {
                    spawned = selected;
                    return "Added Work Jacket.";
                });

                string result = panel.Spawn(item);

                Assert.That(spawned, Is.SameAs(item));
                Assert.That(result, Is.EqualTo("Added Work Jacket."));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(item);
            }
        }

        private static ItemDefinition CreateDefinition(string displayName, string category)
        {
            ItemDefinition definition = ScriptableObject.CreateInstance<ItemDefinition>();
            typeof(ItemDefinition).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(definition, Guid.NewGuid().ToString());
            typeof(ItemDefinition).GetField("displayName", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(definition, displayName);
            typeof(ItemDefinition).GetField("category", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(definition, category);
            return definition;
        }
    }
}
