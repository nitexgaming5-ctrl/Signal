using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public readonly struct Ingredient
    {
        public Ingredient(ItemType type, int count)
        {
            if (type.MaxStack < 1)
                throw new ArgumentException("ItemType is not initialised.", nameof(type));
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");

            Type = type;
            Count = count;
        }

        public ItemType Type { get; }

        public int Count { get; }
    }

    public sealed class Recipe
    {
        private readonly List<Ingredient> _inputs;

        public Recipe(IEnumerable<Ingredient> inputs, Ingredient output)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            _inputs = new List<Ingredient>(inputs);
            if (_inputs.Count == 0)
                throw new ArgumentException("A recipe needs at least one input.", nameof(inputs));

            foreach (var input in _inputs)
            {
                if (input.Count < 1)
                    throw new ArgumentException("Every input needs a count of at least 1.", nameof(inputs));
            }

            if (output.Count < 1)
                throw new ArgumentException("The output needs a count of at least 1.", nameof(output));

            Output = output;
        }

        public IReadOnlyList<Ingredient> Inputs => _inputs;

        public Ingredient Output { get; }
    }

    public enum CraftResult
    {
        Done,
        MissingInputs,
        NoSpaceForOutput
    }

    public static class Crafting
    {
        /// <summary>
        /// Consumes the inputs and adds the output. If the output does not fit, everything is put back.
        /// </summary>
        public static CraftResult TryCraft(Recipe recipe, GridInventory inventory)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            foreach (var input in recipe.Inputs)
            {
                if (inventory.CountOf(input.Type) < RequiredOf(recipe, input.Type))
                    return CraftResult.MissingInputs;
            }

            InventorySnapshot before = inventory.CreateSnapshot();

            foreach (var input in recipe.Inputs)
                inventory.TryRemoveSilently(input.Type, input.Count);

            int leftover = inventory.AddSilently(recipe.Output.Type, recipe.Output.Count, ItemInstanceState.Default, out bool outputChanged);
            if (leftover > 0)
            {
                inventory.RestoreSilently(before);
                return CraftResult.NoSpaceForOutput;
            }

            inventory.NotifyChanged();
            return CraftResult.Done;
        }

        private static int RequiredOf(Recipe recipe, ItemType type)
        {
            int total = 0;
            foreach (var input in recipe.Inputs)
            {
                if (input.Type == type)
                    total += input.Count;
            }

            return total;
        }
    }
}
