using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class RecipeDefinitionTests
    {
        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                UnityEngine.Object.DestroyImmediate(created);

            _created.Clear();
        }

        private ItemDefinition NewItem()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            return item;
        }

        private RecipeDefinition NewRecipe(RecipeDefinition.IngredientEntry[] inputs, RecipeDefinition.IngredientEntry output)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            _created.Add(recipe);
            SetField(recipe, "inputs", inputs);
            SetField(recipe, "output", output);
            return recipe;
        }

        private static void SetField(RecipeDefinition recipe, string fieldName, object value)
        {
            FieldInfo field = typeof(RecipeDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field: " + fieldName);
            field.SetValue(recipe, value);
        }

        [Test]
        public void ToRecipeBuildsTheInputsAndOutput()
        {
            ItemDefinition a = NewItem();
            ItemDefinition b = NewItem();
            RecipeDefinition definition = NewRecipe(
                new[] { new RecipeDefinition.IngredientEntry { item = a, count = 2 } },
                new RecipeDefinition.IngredientEntry { item = b, count = 1 });

            var recipe = definition.ToRecipe();

            Assert.That(recipe.Inputs.Count, Is.EqualTo(1));
            Assert.That(recipe.Inputs[0].Type, Is.EqualTo(a.ToItemType()));
            Assert.That(recipe.Inputs[0].Count, Is.EqualTo(2));
            Assert.That(recipe.Output.Type, Is.EqualTo(b.ToItemType()));
            Assert.That(recipe.Output.Count, Is.EqualTo(1));
        }

        [Test]
        public void AMissingItemThrows()
        {
            RecipeDefinition definition = NewRecipe(
                new[] { new RecipeDefinition.IngredientEntry { item = null, count = 1 } },
                new RecipeDefinition.IngredientEntry { item = NewItem(), count = 1 });

            Assert.Throws<InvalidOperationException>(() => definition.ToRecipe());
        }

        [Test]
        public void NoInputsThrows()
        {
            RecipeDefinition definition = NewRecipe(
                new RecipeDefinition.IngredientEntry[0],
                new RecipeDefinition.IngredientEntry { item = NewItem(), count = 1 });

            Assert.Throws<InvalidOperationException>(() => definition.ToRecipe());
        }
    }
}
