using System;
using System.Reflection;
using NUnit.Framework;
using Signal.Core;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class ContainerLootProfileTests
    {
        [Test]
        public void ProfileGeneratesPersistentItemInstancesOnlyOnce()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            ContainerLootProfile profile = ScriptableObject.CreateInstance<ContainerLootProfile>();
            try
            {
                typeof(ItemDefinition).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(item, Guid.NewGuid().ToString());
                typeof(ItemDefinition).GetField("displayName", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(item, "Test item");

                Type entryType = typeof(ContainerLootProfile).GetNestedType("LootEntry", BindingFlags.NonPublic);
                object entry = Activator.CreateInstance(entryType, true);
                entryType.GetField("item").SetValue(entry, item);
                entryType.GetField("weight").SetValue(entry, 1);
                entryType.GetField("minimumCount").SetValue(entry, 1);
                entryType.GetField("maximumCount").SetValue(entry, 2);
                typeof(ContainerLootProfile).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, Array.CreateInstance(entryType, 1));
                Array entries = (Array)typeof(ContainerLootProfile).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(profile);
                entries.SetValue(entry, 0);
                typeof(ContainerLootProfile).GetField("minimumStacks", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, 2);
                typeof(ContainerLootProfile).GetField("maximumStacks", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, 3);

                var container = new SearchableContainer("profile-test-bin", 8, 6);
                Assert.That(profile.TryGenerate(container, out string error), Is.True, error);
                Assert.That(container.IsGenerated, Is.True);
                Assert.That(container.Inventory.Items.Count, Is.GreaterThanOrEqualTo(1));
                Guid[] generatedIds = new Guid[container.Inventory.Items.Count];
                for (int i = 0; i < generatedIds.Length; i++)
                    generatedIds[i] = container.Inventory.Items[i].ItemInstanceId;

                Assert.That(profile.TryGenerate(container, out _), Is.False);
                Assert.That(container.Inventory.Items.Count, Is.EqualTo(generatedIds.Length));
                for (int i = 0; i < generatedIds.Length; i++)
                    Assert.That(container.Inventory.Items[i].ItemInstanceId, Is.EqualTo(generatedIds[i]));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(item);
            }
        }
    }
}
