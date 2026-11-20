using System;
using System.Reflection;
using NUnit.Framework;
using Signal.Core;
using Signal.Game;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class ResidentPropertyContainerDefinitionTests
    {
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing serialized field " + fieldName);
            field.SetValue(target, value);
        }

        [Test]
        public void AuthoredPropertyContentsAreFixedAndInitializeOnlyOnce()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            ResidentPropertyContainerDefinition definition =
                ScriptableObject.CreateInstance<ResidentPropertyContainerDefinition>();

            try
            {
                SetPrivateField(item, "id", Guid.NewGuid().ToString());
                SetPrivateField(item, "gridWidth", 1);
                SetPrivateField(item, "gridHeight", 1);
                SetPrivateField(item, "maxStack", 5);

                Type entryType = typeof(ResidentPropertyContainerDefinition).GetNestedType(
                    "ContentEntry", BindingFlags.NonPublic);
                Assert.That(entryType, Is.Not.Null);
                object entry = Activator.CreateInstance(entryType, true);
                SetPrivateField(entry, "item", item);
                SetPrivateField(entry, "quantity", 2);
                Array entries = Array.CreateInstance(entryType, 1);
                entries.SetValue(entry, 0);

                SetPrivateField(definition, "containerId", "property-test");
                SetPrivateField(definition, "ownerNpcId", "resident-test");
                SetPrivateField(definition, "locationId", "Apartment");
                SetPrivateField(definition, "displayName", "Test wardrobe");
                SetPrivateField(definition, "interactionPosition", new Vector2(5f, 5f));
                SetPrivateField(definition, "witnessRadius", 3f);
                SetPrivateField(definition, "heatAward", 2);
                SetPrivateField(definition, "progressionAward", 1);
                SetPrivateField(definition, "actionId", "property-theft-test");
                SetPrivateField(definition, "witnessDescription", "Someone took an item.");
                SetPrivateField(definition, "contents", entries);

                var container = new SearchableContainer("property-test", 8, 6);
                Assert.That(definition.GetHeatAward(true), Is.EqualTo(2));
                Assert.That(definition.GetHeatAward(false), Is.Zero);
                Assert.That(definition.TryValidate(out string validationError), Is.True, validationError);
                Assert.That(definition.TryPopulate(container, out string populateError), Is.True, populateError);
                Assert.That(container.IsGenerated, Is.True);
                Assert.That(container.Inventory.Items.Count, Is.EqualTo(1));
                Assert.That(container.Inventory.Items[0].Count, Is.EqualTo(2));
                Assert.That(container.SearchOrder.Count, Is.EqualTo(1));
                Assert.That(container.SearchOrder[0], Is.EqualTo(container.Inventory.Items[0].ItemInstanceId));
                Assert.That(definition.TryPopulate(container, out _), Is.False);
                Assert.That(container.Inventory.CountOf(item.ToItemType()), Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(item);
            }
        }
    }
}
