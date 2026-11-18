using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Generic Unity-facing resident. Identity, routine, destinations, appearance and
    /// dialogue come from a character definition; social knowledge and decisions stay in Core.
    /// </summary>
    public sealed class NpcPresenter : MonoBehaviour
    {
        private NpcCharacterDefinition _definition;
        private NpcUtilityBrain _brain;
        private GameClock _clock;
        private NpcSocialWorld _socialWorld;
        private int _conversationSequence;
        private bool _registered;
        private bool _initialized;
        private Vector2 _destination;
        private NpcActivity _activity;
        private SpriteRenderer _bodyRenderer;
        private string _activePlace = "Hideout";
        private bool _debugSimulationPaused;
        private bool _debugActivityOverride;

        public string NpcId => _definition != null ? _definition.NpcId : string.Empty;
        public string DisplayName => _definition != null ? _definition.DisplayName : string.Empty;
        public string PublicRole => _definition != null ? _definition.PublicRole : string.Empty;
        public NpcActivity Activity => _activity;
        public Vector2 Position => transform.position;
        public string CurrentPlace => ActivityPlace(_activity);

        public void Bind(NpcCharacterDefinition definition, GameClock clock, NpcSocialWorld socialWorld)
        {
            if (definition == null || clock == null)
                return;
            if (string.IsNullOrWhiteSpace(definition.NpcId))
                throw new System.ArgumentException("NPC definition must have a stable ID.", nameof(definition));

            if (_clock != null)
                _clock.Changed -= OnClockChanged;

            _definition = definition;
            _clock = clock;
            _socialWorld = socialWorld;
            _brain = new NpcUtilityBrain(definition.UtilityProfile);
            _clock.Changed += OnClockChanged;
            BuildVisual();

            _destination = _definition.PositionFor(_activity);
            transform.position = new Vector3(_destination.x, _destination.y, 0f);
            if (_socialWorld != null)
            {
                _socialWorld.RegisterNpc(NpcId, ActivityPlace(_activity), transform.position.x,
                    transform.position.y, _definition.AwarenessRadius);
                _registered = true;
            }

            RefreshFromClock(true);
            _initialized = true;
        }

        public void SetActivePlace(string place)
        {
            _activePlace = place ?? string.Empty;
            transform.position = new Vector3(_destination.x, _destination.y, 0f);
            SyncSocialPosition();
            UpdateLocationVisibility();
        }

        public bool CanTalkFrom(Vector2 position)
        {
            return _initialized && gameObject.activeInHierarchy &&
                Vector2.Distance(position, Position) <= 1.85f;
        }

        public bool TryTalk(Vector2 playerPosition, out string dialogue)
        {
            dialogue = null;
            if (!CanTalkFrom(playerPosition))
                return false;

            dialogue = DisplayName + ": " + _definition.DialogueFor(_activity);
            if (_socialWorld != null)
            {
                _conversationSequence++;
                _socialWorld.PublishEvent(new NpcWorldEvent(
                    NpcId + "-conversation-" + _conversationSequence,
                    "Conversation",
                    DisplayName + " spoke with someone nearby.",
                    NpcId,
                    ActivityPlace(_activity),
                    playerPosition.x,
                    playerPosition.y,
                    4.5f,
                    _clock.Day,
                    _clock.MinuteOfDay));
            }
            return true;
        }

        private void OnEnable()
        {
            if (_initialized)
                RefreshFromClock(true);
        }

        private void OnDestroy()
        {
            if (_clock != null)
                _clock.Changed -= OnClockChanged;
        }

        public void SetDebugSimulationPaused(bool paused)
        {
            _debugSimulationPaused = paused;
            if (!paused)
            {
                _debugActivityOverride = false;
                RefreshFromClock(true);
            }
        }

        public void DebugForceActivity(NpcActivity activity)
        {
            _debugActivityOverride = true;
            ApplyActivity(activity, true);
        }

        public void DebugResumeSchedule()
        {
            _debugActivityOverride = false;
            if (!_debugSimulationPaused)
                RefreshFromClock(true);
        }

        private void Update()
        {
            if (!_initialized || _debugSimulationPaused)
                return;

            Vector3 destination = new Vector3(_destination.x, _destination.y, transform.position.z);
            Vector3 current = transform.position;
            if (Mathf.Abs(destination.x - current.x) > 0.02f)
                _bodyRenderer.flipX = destination.x < current.x;

            transform.position = Vector3.MoveTowards(current, destination,
                _definition.WalkingSpeed * Time.deltaTime);
            if ((transform.position - current).sqrMagnitude > 0.0001f)
                SyncSocialPosition();
        }

        private void OnClockChanged(GameClock clock)
        {
            RefreshFromClock(false);
        }

        private void RefreshFromClock(bool snapToDestination)
        {
            if (_clock == null || _brain == null || _debugSimulationPaused || _debugActivityOverride)
                return;

            NpcPerception perception = _socialWorld != null && _registered
                ? _socialWorld.BuildPerceptionFor(NpcId, _definition.AwarenessRadius)
                : null;
            NpcActivity next = _brain.AdvanceTo(_clock.Day, _clock.MinuteOfDay, perception);
            if (next == _activity && !snapToDestination)
                return;

            ApplyActivity(next, snapToDestination);
        }

        private void ApplyActivity(NpcActivity next, bool snapToDestination)
        {
            bool wasInCurrentPlace = ActivityPlace(_activity) == _activePlace;
            bool willBeInCurrentPlace = ActivityPlace(next) == _activePlace;
            _activity = next;
            _destination = _definition.PositionFor(next);

            // Cross-location travel is resolved off-screen; only same-place movement animates.
            if (snapToDestination || !wasInCurrentPlace || !willBeInCurrentPlace)
                transform.position = new Vector3(_destination.x, _destination.y, 0f);
            SyncSocialPosition();
            UpdateLocationVisibility();
        }

        private void UpdateLocationVisibility()
        {
            bool shouldBeActive = ActivityPlace(_activity) == _activePlace;
            if (gameObject.activeSelf != shouldBeActive)
                gameObject.SetActive(shouldBeActive);
        }

        private void SyncSocialPosition()
        {
            if (_socialWorld == null || !_registered)
                return;
            _socialWorld.MoveNpc(NpcId, ActivityPlace(_activity), transform.position.x, transform.position.y);
        }

        private static string ActivityPlace(NpcActivity activity)
        {
            switch (activity)
            {
                case NpcActivity.Work: return "CornerShop";
                case NpcActivity.Home: return "Apartment";
                default: return "Street";
            }
        }

        private void BuildVisual()
        {
            if (_bodyRenderer != null)
                return;

            _bodyRenderer = CreatePart("Coat", new Vector2(0f, -0.12f), new Vector2(0.58f, 0.76f),
                _definition.CoatColor, 8);
            CreatePart("Clothing", new Vector2(0f, -0.18f), new Vector2(0.34f, 0.5f),
                _definition.ClothingColor, 9);
            CreatePart("Face", new Vector2(0f, 0.42f), new Vector2(0.43f, 0.43f),
                _definition.SkinColor, 10);
            CreatePart("Hair", new Vector2(0f, 0.61f), new Vector2(0.46f, 0.2f),
                _definition.HairColor, 11);
            CreatePart("Detail", new Vector2(0f, 0.73f), new Vector2(0.48f, 0.16f),
                _definition.DetailColor, 12);
        }

        private SpriteRenderer CreatePart(string partName, Vector2 localPosition, Vector2 size,
            Color color, int sortingOrder)
        {
            var part = new GameObject(partName);
            part.transform.SetParent(transform, false);
            part.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
