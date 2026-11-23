using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Signal.Core;

namespace Signal.Game
{
    /// <summary>
    /// A close-up, tactile first-pass workbench workspace. This is a presentation and
    /// interaction surface only; it deliberately does not invent crafting or economy rules.
    /// </summary>
    public sealed class WorkbenchWorkspace : MonoBehaviour
    {
        private static readonly Color BackdropColor = new Color32(8, 11, 15, 232);
        private static readonly Color FrameColor = new Color32(32, 38, 43, 255);
        private static readonly Color SurfaceColor = new Color32(119, 85, 61, 255);
        private static readonly Color InkColor = new Color32(235, 226, 207, 255);
        private static readonly Color MutedColor = new Color32(185, 181, 168, 255);

        private PlayerMotor2D _motor;
        private Canvas _canvas;
        private GameObject _canvasObject;
        private Text _status;
        private RectTransform _surface;
        private bool _built;
        private GridInventory _inventory;
        private AssetOwnershipBook _ownership;
        private IReadOnlyList<RecipeDefinition> _recipes;
        private Func<RecipeDefinition, bool> _isRecipeUnlocked;
        private Func<Guid, string> _produce;
        private RectTransform _recipeList;
        private Text _recipeDetails;
        private Button _produceButton;
        private RecipeDefinition _selectedRecipe;

        public bool IsOpen { get; private set; }

        public void Bind(PlayerMotor2D motor)
        {
            _motor = motor;
        }

        public void Bind(PlayerMotor2D motor, GridInventory inventory, AssetOwnershipBook ownership,
            IReadOnlyList<RecipeDefinition> recipes, Func<RecipeDefinition, bool> isRecipeUnlocked,
            Func<Guid, string> produce)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (ownership == null) throw new ArgumentNullException(nameof(ownership));
            _motor = motor;
            _inventory = inventory;
            _ownership = ownership;
            _recipes = recipes ?? Array.Empty<RecipeDefinition>();
            _isRecipeUnlocked = isRecipeUnlocked ?? throw new ArgumentNullException(nameof(isRecipeUnlocked));
            _produce = produce ?? throw new ArgumentNullException(nameof(produce));
        }

        public void Open()
        {
            if (IsOpen)
                return;

            if (!_built)
                Build();

            _canvasObject.SetActive(true);
            IsOpen = true;
            RefreshProduction();
            if (_motor != null)
                _motor.enabled = false;
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            if (_canvasObject != null)
                _canvasObject.SetActive(false);
            if (_motor != null)
                _motor.enabled = true;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void OnDestroy()
        {
            if (_motor != null)
                _motor.enabled = true;
        }

        private void Build()
        {
            EnsureEventSystem();

            _canvasObject = new GameObject("WorkbenchWorkspaceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject.transform.SetParent(transform, false);
            _canvas = _canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform overlay = CreatePanel(_canvasObject.transform, "Backdrop", BackdropColor);
            Stretch(overlay, Vector2.zero, Vector2.zero);

            RectTransform frame = CreatePanel(overlay, "WorkbenchFrame", FrameColor);
            SetCentered(frame, new Vector2(1240f, 760f), Vector2.zero);

            Text title = CreateText(frame, "WORKBENCH  /  PERSONAL SPACE", 28, InkColor, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(820f, 42f));

            Text subtitle = CreateText(frame, "Arrange the tools. Inspect the objects. More functionality will come with the game.", 16, MutedColor, TextAnchor.MiddleLeft);
            Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -70f), new Vector2(900f, 30f));

            Button closeButton = CreateButton(frame, "BACK TO ROOM  [ESC]", new Vector2(230f, 48f));
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-28f, -28f);
            closeButton.onClick.AddListener(Close);

            RectTransform tableBorder = CreatePanel(frame, "TableBorder", new Color32(47, 34, 28, 255));
            Anchor(tableBorder, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 72f), new Vector2(-28f, -126f));

            _surface = CreatePanel(tableBorder, "TableSurface", SurfaceColor);
            Stretch(_surface, new Vector2(10f, 10f), new Vector2(-10f, -10f));

            CreateTableMark(_surface, new Vector2(0f, 0f), new Vector2(900f, 2f), new Color32(93, 66, 49, 120));
            CreateTableMark(_surface, new Vector2(0f, 95f), new Vector2(900f, 2f), new Color32(93, 66, 49, 100));
            CreateTableMark(_surface, new Vector2(0f, -120f), new Vector2(900f, 2f), new Color32(93, 66, 49, 100));

            AddDraggable("SCALE", "A battered pocket scale. The display is scratched.", new Color32(74, 82, 84, 255), new Vector2(-300f, 80f), new Vector2(190f, 116f));
            AddDraggable("GLASS JARS", "A few empty jars, cleaned and ready to be used.", new Color32(92, 132, 119, 255), new Vector2(80f, 125f), new Vector2(210f, 120f));
            AddDraggable("NOTEBOOK", "Loose notes, crossed-out numbers and a coffee stain.", new Color32(157, 132, 91, 255), new Vector2(300f, -105f), new Vector2(220f, 150f));
            AddDraggable("TOOLS", "Small tools. Some are useful. Some are just old.", new Color32(99, 105, 112, 255), new Vector2(-80f, -135f), new Vector2(210f, 72f));
            AddDraggable("TRAY", "A shallow metal tray with a dent in one corner.", new Color32(111, 111, 105, 255), new Vector2(-350f, -135f), new Vector2(170f, 92f));

            BuildProductionControls(frame);

            _status = CreateText(frame, "Drag objects to rearrange the table. Click one to inspect it.", 17, InkColor, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, 18f), new Vector2(1050f, 34f));
            closeRect.SetAsLastSibling();

            _canvasObject.SetActive(false);
            _built = true;
        }

        private void BuildProductionControls(Transform frame)
        {
            RectTransform panel = CreatePanel(frame, "ProductionPanel", new Color32(20, 25, 30, 248));
            Anchor(panel, Vector2.one, Vector2.one, new Vector2(-24f, -124f), new Vector2(330f, 560f));

            Text title = CreateText(panel, "PRODUCTION  /  RECIPE BOOK", 18, InkColor, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(14f, -12f), new Vector2(300f, 30f));

            _recipeList = CreatePanel(panel, "RecipeList", new Color32(27, 33, 39, 255));
            Anchor(_recipeList, new Vector2(12f, -52f), new Vector2(306f, 190f));

            _recipeDetails = CreateText(panel, "Select an authored recipe.", 14, MutedColor, TextAnchor.UpperLeft);
            Anchor(_recipeDetails.rectTransform, new Vector2(14f, -250f), new Vector2(300f, 150f));

            _produceButton = CreateButton(panel, "PRODUCE", new Vector2(300f, 42f));
            Anchor(_produceButton.GetComponent<RectTransform>(), new Vector2(14f, -408f), new Vector2(300f, 42f));
            _produceButton.onClick.AddListener(() =>
            {
                if (_selectedRecipe == null || _produce == null) return;
                SelectStatus(_produce(_selectedRecipe.Id));
                RefreshProduction();
            });

            RefreshProduction();
        }

        private void RefreshProduction()
        {
            if (_recipeList == null) return;
            for (int i = _recipeList.childCount - 1; i >= 0; i--)
                Destroy(_recipeList.GetChild(i).gameObject);

            if (_recipes == null || _recipes.Count == 0)
            {
                _recipeDetails.text = "No authored recipes assigned yet. Recipe content must be approved and added to GameSession.";
                _produceButton.interactable = false;
            }
            else
            {
                int index = 0;
                bool selectedStillValid = false;
                foreach (RecipeDefinition recipe in _recipes)
                {
                    if (recipe == null) continue;
                    if (recipe == _selectedRecipe) selectedStillValid = true;
                    bool unlocked = _isRecipeUnlocked != null && _isRecipeUnlocked(recipe);
                    string label = recipe.DisplayName + (unlocked ? string.Empty : "  [LOCKED]");
                    Button button = CreateButton(_recipeList, label, new Vector2(288f, 34f));
                    Anchor(button.GetComponent<RectTransform>(), new Vector2(8f, -6f - index * 38f), new Vector2(288f, 34f));
                    button.onClick.AddListener(() => { _selectedRecipe = recipe; RefreshProduction(); });
                    index++;
                }

                if (!selectedStillValid) _selectedRecipe = null;
                if (_selectedRecipe == null)
                {
                    foreach (RecipeDefinition recipe in _recipes)
                    {
                        if (recipe != null && _isRecipeUnlocked != null && _isRecipeUnlocked(recipe))
                        {
                            _selectedRecipe = recipe;
                            break;
                        }
                    }
                }

                if (_selectedRecipe == null)
                {
                    _recipeDetails.text = "No recipe is currently unlocked.";
                    _produceButton.interactable = false;
                }
                else if (!_selectedRecipe.TryValidate(out string definitionError))
                {
                    _recipeDetails.text = "RECIPE CONFIGURATION ERROR\n" + definitionError;
                    _produceButton.interactable = false;
                }
                else
                {
                    _recipeDetails.text = RecipeDescription(_selectedRecipe);
                    _produceButton.interactable = _isRecipeUnlocked(_selectedRecipe) && HasOwnedInputs(_selectedRecipe);
                }
            }

        }

        private bool HasOwnedInputs(RecipeDefinition recipe)
        {
            if (_inventory == null || _ownership == null || recipe == null) return false;
            var required = new Dictionary<ItemType, int>();
            try
            {
                foreach (RecipeDefinition.IngredientEntry input in recipe.Inputs)
                {
                    if (input == null || input.item == null || input.count < 1) return false;
                    ItemType type;
                    try { type = input.item.ToItemType(); }
                    catch (InvalidOperationException) { return false; }
                    required.TryGetValue(type, out int current);
                    required[type] = checked(current + input.count);
                }
            }
            catch (OverflowException)
            {
                return false;
            }

            foreach (KeyValuePair<ItemType, int> requirement in required)
            {
                long total = 0;
                foreach (PlacedItem item in _inventory.Items)
                    if (item.Type == requirement.Key &&
                        _ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) &&
                        owner == EconomicActorId.Player)
                        total += item.Count;
                if (total < requirement.Value) return false;
            }
            return true;
        }

        private string RecipeDescription(RecipeDefinition recipe)
        {
            var lines = new System.Collections.Generic.List<string>();
            lines.Add(recipe.Description);
            lines.Add("INPUTS");
            foreach (RecipeDefinition.IngredientEntry input in recipe.Inputs)
                lines.Add((input.item != null ? input.item.DisplayName : "Unknown item") + " × " + input.count);
            lines.Add("OUTPUT");
            lines.Add((recipe.Output.item != null ? recipe.Output.item.DisplayName : "Unknown item") + " × " + recipe.Output.count);
            lines.Add(HasOwnedInputs(recipe) ? "Materials available." : "Missing player-owned materials.");
            return string.Join("\n", lines);
        }

        private void SelectStatus(string message)
        {
            if (_status != null) _status.text = message;
        }

        private void AddDraggable(string label, string description, Color color, Vector2 position, Vector2 size)
        {
            RectTransform item = CreatePanel(_surface, label.Replace(" ", string.Empty), color);
            SetCentered(item, size, position);

            Image image = item.GetComponent<Image>();
            image.raycastTarget = true;

            Text itemLabel = CreateText(item, label, 18, Color.white, TextAnchor.MiddleCenter);
            Stretch(itemLabel.rectTransform, new Vector2(8f, 6f), new Vector2(-8f, -6f));

            WorkbenchDraggable draggable = item.gameObject.AddComponent<WorkbenchDraggable>();
            draggable.Configure(_surface, _canvas, label, description, SelectObject);
        }

        private void SelectObject(string message)
        {
            if (_status != null)
                _status.text = message;
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                return;
            }

            GameObject eventObject = new GameObject("WorkbenchEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(transform, false);
        }

        private static RectTransform CreatePanel(Transform parent, string objectName, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static Text CreateText(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Font.CreateDynamicFontFromOSFont("Arial", size);
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 size)
        {
            RectTransform rect = CreatePanel(parent, "CloseButton", new Color32(55, 63, 67, 255));
            SetCentered(rect, size, Vector2.zero);
            Text text = CreateText(rect, label, 16, InkColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            return button;
        }

        private static void CreateTableMark(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            RectTransform mark = CreatePanel(parent, "WoodGrain", color);
            SetCentered(mark, size, position);
            mark.GetComponent<Image>().raycastTarget = false;
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = maxOffset;
        }

        private static void SetCentered(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = min;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size)
        {
            Anchor(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        }
    }
}