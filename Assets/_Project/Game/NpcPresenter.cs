using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public struct NpcTravelWaypoint
    {
        public string LocationId { get; }
        public Vector2 Position { get; }

        public NpcTravelWaypoint(string locationId, Vector2 position)
        {
            LocationId = locationId;
            Position = position;
        }
    }

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
        private CityBlockBootstrap _city;
        private string _currentPlace = "Apartment";
        private readonly List<NpcTravelWaypoint> _travelRoute = new List<NpcTravelWaypoint>();
        private int _nextTravelWaypoint;
        private SpriteRenderer[] _visualRenderers = new SpriteRenderer[0];

        public string NpcId => _definition != null ? _definition.NpcId : string.Empty;
        public string DisplayName => _definition != null ? _definition.DisplayName : string.Empty;
        public string PublicRole => _definition != null ? _definition.PublicRole : string.Empty;
        public NpcActivity Activity => _activity;
        public Vector2 Position => transform.position;
        public string CurrentPlace => _currentPlace;
        public bool NavigationFailed { get; private set; }
        public bool IsTravelling => NavigationFailed || _nextTravelWaypoint < _travelRoute.Count;
        public Vector2 Destination => _destination;
        public IReadOnlyList<NpcTravelWaypoint> DebugTravelRoute => _travelRoute;
        public int DebugNextWaypoint => _nextTravelWaypoint;

        public void Bind(NpcCharacterDefinition definition, GameClock clock, NpcSocialWorld socialWorld, CityBlockBootstrap city)
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
            _city = city;
            _brain = new NpcUtilityBrain(definition.UtilityProfile);
            _clock.Changed += OnClockChanged;
            BuildVisual();

            _destination = _definition.PositionFor(_activity);
            _currentPlace = ActivityPlace(_activity);
            transform.position = new Vector3(_destination.x, _destination.y, 0f);
            if (_socialWorld != null)
            {
                _socialWorld.RegisterNpc(NpcId, _currentPlace, transform.position.x,
                    transform.position.y, _definition.AwarenessRadius);
                _registered = true;
            }

            RefreshFromClock(true);
            _initialized = true;
        }

        public void SetActivePlace(string place)
        {
            _activePlace = place ?? string.Empty;
            UpdateLocationVisibility();
        }

        public bool CanTalkFrom(Vector2 position)
        {
            return _initialized && _currentPlace == _activePlace &&
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
                    _currentPlace,
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
                RefreshFromClock(false);
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
                RefreshFromClock(false);
        }

        private void Update()
        {
            if (_initialized && !_debugSimulationPaused)
                AdvanceMovement(Time.deltaTime);
        }

        public void DebugAdvanceMovement(float deltaTime)
        {
            AdvanceMovement(deltaTime);
        }

        private void AdvanceMovement(float deltaTime)
        {
            if (!_initialized || _debugSimulationPaused || deltaTime <= 0f ||
                _definition == null || _definition.WalkingSpeed <= 0f)
                return;

            float remainingDistance = _definition.WalkingSpeed * deltaTime;
            while (remainingDistance > 0f && _nextTravelWaypoint < _travelRoute.Count)
            {
                NpcTravelWaypoint waypoint = _travelRoute[_nextTravelWaypoint];
                if (!string.IsNullOrWhiteSpace(waypoint.LocationId) &&
                    waypoint.LocationId != _currentPlace)
                {
                    // Crossing a doorway changes the active location coordinate space.
                    _currentPlace = waypoint.LocationId;
                    transform.position = new Vector3(waypoint.Position.x, waypoint.Position.y, transform.position.z);
                    SyncSocialPosition();
                    UpdateLocationVisibility();
                    _nextTravelWaypoint++;
                    continue;
                }

                Vector2 current = transform.position;
                Vector2 offset = waypoint.Position - current;
                float distance = offset.magnitude;
                if (distance <= 0.001f)
                {
                    ArriveAtWaypoint(waypoint);
                    continue;
                }

                float step = Mathf.Min(remainingDistance, distance);
                Vector2 next = current + offset / distance * step;
                if (Mathf.Abs(next.x - current.x) > 0.001f)
                    _bodyRenderer.flipX = next.x < current.x;
                transform.position = new Vector3(next.x, next.y, transform.position.z);
                remainingDistance -= step;
                SyncSocialPosition();

                if (step >= distance - 0.001f)
                    ArriveAtWaypoint(waypoint);
            }
        }

        private void ArriveAtWaypoint(NpcTravelWaypoint waypoint)
        {
            transform.position = new Vector3(waypoint.Position.x, waypoint.Position.y, transform.position.z);
            if (!string.IsNullOrWhiteSpace(waypoint.LocationId) &&
                _currentPlace != waypoint.LocationId)
            {
                _currentPlace = waypoint.LocationId;
                SyncSocialPosition();
                UpdateLocationVisibility();
            }

            _nextTravelWaypoint++;
            if (_nextTravelWaypoint >= _travelRoute.Count)
            {
                _travelRoute.Clear();
                _nextTravelWaypoint = 0;
                SyncSocialPosition();
            }
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
            _activity = next;
            _destination = _definition.PositionFor(next);
            string targetPlace = ActivityPlace(next);
            _travelRoute.Clear();
            _nextTravelWaypoint = 0;
            NavigationFailed = false;

            if (snapToDestination)
            {
                // Used only for first-time placement. Resume and schedule changes must walk.
                _currentPlace = targetPlace;
                transform.position = new Vector3(_destination.x, _destination.y, 0f);
            }
            else
            {
                TryPlanRouteToDestination();
            }
            SyncSocialPosition();
            UpdateLocationVisibility();
        }

        /// <summary>
        /// Makes one explicit retry after a previous route failure. This is intentionally
        /// not called every frame; callers decide when the navigation world may have changed.
        /// </summary>
        public bool DebugRetryNavigation()
        {
            if (!_initialized || !NavigationFailed || _debugSimulationPaused || _debugActivityOverride)
                return false;
            return TryPlanRouteToDestination();
        }

        private bool TryPlanRouteToDestination()
        {
            _travelRoute.Clear();
            _nextTravelWaypoint = 0;

            if (_city != null)
            {
                _travelRoute.AddRange(_city.BuildNpcTravelRoute(
                    _currentPlace, transform.position, ActivityPlace(_activity), _destination));
            }
            else if (_currentPlace == ActivityPlace(_activity))
            {
                _travelRoute.Add(new NpcTravelWaypoint(_currentPlace, _destination));
            }

            NavigationFailed = _travelRoute.Count == 0 &&
                Vector2.Distance(transform.position, _destination) > 0.1f;
            if (NavigationFailed)
            {
                Debug.LogWarning("NPC navigation could not find a route for " + NpcId +
                    " from " + _currentPlace + " to " + ActivityPlace(_activity) + ".");
            }
            SyncSocialPosition();
            UpdateLocationVisibility();
            return !NavigationFailed;
        }

        private void UpdateLocationVisibility()
        {
            bool shouldBeVisible = _currentPlace == _activePlace;
            for (int i = 0; i < _visualRenderers.Length; i++)
                if (_visualRenderers[i] != null)
                    _visualRenderers[i].enabled = shouldBeVisible;
        }

        private void SyncSocialPosition()
        {
            if (_socialWorld == null || !_registered)
                return;
            _socialWorld.MoveNpc(NpcId, _currentPlace, transform.position.x, transform.position.y);
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
            _visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            UpdateLocationVisibility();
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
