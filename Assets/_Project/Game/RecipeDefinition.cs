using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>A recipe authored as an asset: some items in, one item out.</summary>
    [CreateAssetMenu(menuName = "Signal/Recipe Definition", fileName = "RecipeDefinition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class IngredientEntry
        {
            public ItemDefinition item;
            [Min(1)] public int count = 1;
        }

        [SerializeField] private string displayName;
        [SerializeField] private IngredientEntry[] inputs;
        [SerializeField] private IngredientEntry output;

        public string DisplayName => displayName;

        public Recipe ToRecipe()
        {
            if (inputs == null || inputs.Length == 0)
                throw new InvalidOperationException("Recipe '" + name + "' has no inputs.");

            var ingredients = new Ingredient[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                ingredients[i] = ToIngredient(inputs[i], "input " + (i + 1));

            return new Recipe(ingredients, ToIngredient(output, "output"));
        }

        private Ingredient ToIngredient(IngredientEntry entry, string role)
        {
            if (entry == null || entry.item == null)
                throw new InvalidOperationException("Recipe '" + name + "': " + role + " has no item.");
            if (entry.count < 1)
                throw new InvalidOperationException("Recipe '" + name + "': " + role + " needs a count of at least 1.");

            return new Ingredient(entry.item.ToItemType(), entry.count);
        }
    }
}
