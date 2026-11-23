using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public enum WorkshopUpgradeAcquisitionMode
    {
        Unconfigured = 0,
        CleanCardPurchase = 1
    }

    public enum WorkshopUpgradePrerequisiteMode
    {
        Unconfigured = 0,
        None = 1,
        CompletedContract = 2
    }

    /// <summary>One authored, visible hideout improvement that unlocks a recipe.</summary>
    [CreateAssetMenu(menuName = "Signal/Workshop Upgrade", fileName = "WorkshopUpgrade")]
    public sealed class WorkshopUpgradeDefinition : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private RecipeDefinition unlockedRecipe;
        [SerializeField] private GameObject installedPropPrefab;
        [SerializeField] private Vector3 installedPropLocalOffset;
        [SerializeField] private WorkshopUpgradeAcquisitionMode acquisitionMode;
        [SerializeField] private WorkshopUpgradePrerequisiteMode prerequisiteMode;
        [SerializeField] private CustomerContractDefinition prerequisiteContract;
        [SerializeField, Min(1)] private int cleanCardCost = 1;
        [SerializeField] private string payeeNpcId;

        public Guid Id => Guid.TryParse(id, out Guid value) ? value : Guid.Empty;
        public string DisplayName => displayName;
        public string Description => description;
        public RecipeDefinition UnlockedRecipe => unlockedRecipe;
        public GameObject InstalledPropPrefab => installedPropPrefab;
        public Vector3 InstalledPropLocalOffset => installedPropLocalOffset;
        public WorkshopUpgradeAcquisitionMode AcquisitionMode => acquisitionMode;
        public WorkshopUpgradePrerequisiteMode PrerequisiteMode => prerequisiteMode;
        public CustomerContractDefinition PrerequisiteContract => prerequisiteContract;
        public int CleanCardCost => cleanCardCost;
        public string PayeeNpcId => payeeNpcId;
        public EconomicActorId PayeeActor => new EconomicActorId("resident:" + payeeNpcId);

        public bool TryValidate(out string error)
        {
            if (Id == Guid.Empty || string.IsNullOrWhiteSpace(displayName) ||
                unlockedRecipe == null || string.IsNullOrWhiteSpace(payeeNpcId) || cleanCardCost < 1)
            {
                error = "Assign a stable ID, display name, unlocked recipe, resident payee and positive CARD cost.";
                return false;
            }
            if (acquisitionMode != WorkshopUpgradeAcquisitionMode.CleanCardPurchase)
            {
                error = "Select Clean CARD Purchase as the upgrade acquisition mode.";
                return false;
            }
            if (!Enum.IsDefined(typeof(WorkshopUpgradePrerequisiteMode), prerequisiteMode) ||
                prerequisiteMode == WorkshopUpgradePrerequisiteMode.Unconfigured)
            {
                error = "Choose either no prerequisite or a completed customer contract.";
                return false;
            }
            if (prerequisiteMode == WorkshopUpgradePrerequisiteMode.CompletedContract &&
                (prerequisiteContract == null || !prerequisiteContract.TryValidate(out _)))
            {
                error = "A valid prerequisite customer contract is required.";
                return false;
            }
            if (prerequisiteMode == WorkshopUpgradePrerequisiteMode.None && prerequisiteContract != null)
            {
                error = "Remove the prerequisite contract when the prerequisite mode is None.";
                return false;
            }
            if (!unlockedRecipe.TryValidate(out string recipeError))
            {
                error = "The unlocked recipe is invalid: " + recipeError;
                return false;
            }
            if (installedPropPrefab == null)
            {
                error = "Assign a visible installed prop prefab for this practical upgrade.";
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