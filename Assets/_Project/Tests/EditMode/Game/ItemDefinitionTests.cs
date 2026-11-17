using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class ItemDefinitionTests
    {
        private readonly List<ItemDefinition> _created = new List<ItemDefinition>();

        [TearDown]
        public void TearDown()
        {
            foreach (var definition in _created)
                UnityEngine.Object.DestroyImmediate(definition);

            _created.Clear();
        }

        private ItemDefinition Create()
        {
            var definition = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(definition);
            return definition;
        }

        private static void SetField(ItemDefinition definition, string fieldName, object value)
        {
            FieldInfo field = typeof(ItemDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field: " + fieldName);
            field.SetValue(definition, value);
        }

        private static string GetId(ItemDefinition definition)
        {
            FieldInfo field = typeof(ItemDefinition).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field: id");
            return (string)field.GetValue(definition);
        }

        private static void RunOnValidate(ItemDefinition definition)
        {
            MethodInfo method = typeof(ItemDefinition).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing private method: OnValidate");
            method.Invoke(definition, null);
        }

        [Test]
        public void ValidAssetConvertsToAMatchingItemType()
        {
            var definition = Create();
            var guid = Guid.NewGuid();
            SetField(definition, "id", guid.ToString());
            SetField(definition, "gridWidth", 2);
            SetField(definition, "gridHeight", 3);
            SetField(definition, "maxStack", 4);

            var type = definition.ToItemType();

            Assert.That(type.Id, Is.EqualTo(guid));
            Assert.That(type.Width, Is.EqualTo(2));
            Assert.That(type.Height, Is.EqualTo(3));
            Assert.That(type.MaxStack, Is.EqualTo(4));
        }

        [Test]
        public void EmptyIdThrowsWhenConverted()
        {
            var definition = Create();
            SetField(definition, "id", "");

            Assert.Throws<InvalidOperationException>(() => definition.ToItemType());
        }

        [Test]
        public void NewlyCreatedAssetAlreadyHasAValidId()
        {
            var definition = Create();

            Assert.That(Guid.TryParse(GetId(definition), out Guid guid), Is.True);
            Assert.That(guid, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void InvalidIdThrowsWhenConverted()
        {
            var definition = Create();
            SetField(definition, "id", "not-a-guid");

            Assert.Throws<InvalidOperationException>(() => definition.ToItemType());
        }

        [Test]
        public void TwoDefinitionsGetDifferentValidIds()
        {
            var first = Create();
            var second = Create();

            RunOnValidate(first);
            RunOnValidate(second);

            Assert.That(Guid.TryParse(GetId(first), out _), Is.True);
            Assert.That(Guid.TryParse(GetId(second), out _), Is.True);
            Assert.That(GetId(first), Is.Not.EqualTo(GetId(second)));
        }

        [Test]
        public void AGeneratedIdIsNeverRegenerated()
        {
            var definition = Create();

            RunOnValidate(definition);
            string firstId = GetId(definition);
            RunOnValidate(definition);

            Assert.That(GetId(definition), Is.EqualTo(firstId));
        }
    }
}
