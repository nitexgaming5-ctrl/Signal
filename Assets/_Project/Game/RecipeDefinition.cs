using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public enum ProductionTimingMode
    {
        Unconfigured = 0,
        Immediate = 1
    }

    /// <summary>A stable, authored recipe definition for one immediate production action.</summary>
    [CreateAssetMenu(menuName = "Signal/Recipe Definition", fileName = "RecipeDefinition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class IngredientEntry
        {
            public ItemDefinition item;
            [Min(1)] public int count = 1;
        }

        [SerializeField, HideInInspector] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private ProductionTimingMode timingMode;
        [SerializeField] private IngredientEntry[] inputs;
        [SerializeField] private IngredientEntry output;

        public Guid Id => Guid.TryParse(id, out Guid value) ? value : Guid.Empty;
        public string DisplayName => displayName;
        public string Description => description;
        public ProductionTimingMode TimingMode => timingMode;
        public IngredientEntry[] Inputs => inputs ?? Array.Empty<IngredientEntry>();
        public IngredientEntry Output => output;

        public bool TryValidate(out string error)
        {
            if (Id == Guid.Empty)
            {
                error = "The recipe has no valid stable ID.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(displayName))
            {
                error = "The recipe needs a display name.";
                return false;
            }
            if (timingMode != ProductionTimingMode.Immediate)
            {
                error = "Select Immediate production timing. Timed production is not implemented.";
                return false;
            }
            if (inputs == null || inputs.Length == 0)
            {
                error = "The recipe needs at least one input.";
                return false;
            }
            for (int i = 0; i < inputs.Length; i++)
                if (!ValidateEntry(inputs[i], "input " + (i + 1), out error))
                    return false;
            if (!ValidateEntry(output, "output", out error))
                return false;

            error = null;
            return true;
        }

        public Recipe ToRecipe()
        {
            if (!TryValidate(out string error))
                throw new InvalidOperationException("Recipe '" + name + "' is invalid: " + error);

            var ingredients = new Ingredient[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                ingredients[i] = new Ingredient(inputs[i].item.ToItemType(), inputs[i].count);

            return new Recipe(ingredients, new Ingredient(output.item.ToItemType(), output.count));
        }

        private bool ValidateEntry(IngredientEntry entry, string role, out string error)
        {
            if (entry == null || entry.item == null)
            {
                error = role + " has no item assigned.";
                return false;
            }
            if (entry.count < 1)
            {
                error = role + " needs a quantity of at least one.";
                return false;
            }
            try { entry.item.ToItemType(); }
            catch (InvalidOperationException exception)
            {
                error = role + " references an invalid item: " + exception.Message;
                return false;
            }
            error = null;
            return true;
        }

        private void Reset() => EnsureId();
        private void OnValidate() => EnsureId();

        private void EnsureId()
        {
            if (!string.IsNullOrEmpty(id)) return;
            id = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}