using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Signal.Game
{
    /// <summary>
    /// Builds a compact connected street and two placeholder interiors. The same player,
    /// inventory, clock and session remain alive while the visible location changes.
    /// </summary>
    public sealed class CityBlockBootstrap : MonoBehaviour
    {
        private enum Place { Hideout, Street, CornerShop, Apartment }

        private static readonly Color Ground = new Color32(75, 79, 75, 255);
        private static readonly Color Asphalt = new Color32(48, 53, 55, 255);
        private static readonly Color Concrete = new Color32(115, 112, 101, 255);
        private static readonly Color Wall = new Color32(91, 83, 76, 255);

        private Transform _player;
        private Rigidbody2D _body;
        private CameraFollow2D _cameraFollow;
        private readonly Rect _worldBounds = new Rect(-9f, -26f, 38f, 40f);
        private GameObject _streetRoot;
        private GameObject _shopRoot;
        private GameObject _apartmentRoot;
        private Place _place;
        [SerializeField] private List<NpcCharacterDefinition> characterDefinitions = new List<NpcCharacterDefinition>();
        private readonly List<NpcPresenter> _residents = new List<NpcPresenter>();
        private readonly List<NpcCharacterDefinition> _temporaryDefinitions = new List<NpcCharacterDefinition>();
        private GameClock _clock;
        private NpcSocialWorld _socialWorld;
        private NpcSocialEncounterSystem _socialEncounterSystem;
        private bool _debugSimulationPaused;
        private CityNavigationMap _navigationMap;
        private CityNavigationDebugView _navigationDebugView;

        private static readonly Vector2 HomeDoor = new Vector2(10f, -1.5f);
        private static readonly Vector2 ShopDoor = new Vector2(0.5f, -10.8f);
        private static readonly Vector2 ApartmentDoor = new Vector2(19.5f, -10.8f);
        private const float InteractionDistance = 1.85f;

        public bool IsAtHome => _place == Place.Hideout;
        public string CurrentPlace => _place.ToString();
        public int ResidentCount => _residents.Count;
        public int SocialEncounterCount => _socialEncounterSystem != null ? _socialEncounterSystem.EncounterCount : 0;
        public CityNavigationMap NavigationMap => _navigationMap;
        public IReadOnlyList<NpcPresenter> DebugResidents => _residents;
        public bool DebugNavigationOverlayVisible => _navigationDebugView != null && _navigationDebugView.IsVisible;

        public bool DebugToggleNavigationOverlay()
        {
            return _navigationDebugView != null && _navigationDebugView.Toggle();
        }

        public Vector2 GetResidentPosition(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            return resident != null ? resident.Position : Vector2.zero;
        }

        public NpcActivity GetResidentActivity(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            return resident != null ? resident.Activity : NpcActivity.Home;
        }

        public string GetResidentCurrentPlace(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            return resident != null ? resident.CurrentPlace : string.Empty;
        }

        public bool IsResidentTravelling(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            return resident != null && resident.IsTravelling;
        }

        public bool HasResident(string npcId)
        {
            return FindResident(npcId) != null;
        }

        public bool IsAnyResidentWithin(Vector2 position, float radius, string excludeNpcId = null)
        {
            if (radius < 0f)
                return false;
            float squaredRadius = radius * radius;
            for (int i = 0; i < _residents.Count; i++)
            {
                NpcPresenter resident = _residents[i];
                if (resident == null || resident.CurrentPlace != _place.ToString() ||
                    (!string.IsNullOrWhiteSpace(excludeNpcId) && resident.NpcId == excludeNpcId))
                    continue;
                if ((resident.Position - position).sqrMagnitude <= squaredRadius)
                    return true;
            }
            return false;
        }
        public NpcSocialWorld SocialWorld => _socialWorld;

        public bool DebugSimulationPaused => _debugSimulationPaused;

        public bool DebugSetNpcSimulationPaused(bool paused)
        {
            _debugSimulationPaused = paused;
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i] != null)
                    _residents[i].SetDebugSimulationPaused(paused);
            return _debugSimulationPaused;
        }

        public bool DebugForceResidentActivity(string npcId, NpcActivity activity)
        {
            NpcPresenter resident = FindResident(npcId);
            if (resident == null)
                return false;
            resident.DebugForceActivity(activity);
            return true;
        }

        public bool DebugResumeResidentSchedule(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            if (resident == null)
                return false;
            resident.DebugResumeSchedule();
            return true;
        }

        public void DebugAdvanceNpcMovement(float seconds)
        {
            if (seconds <= 0f)
                return;
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i] != null)
                    _residents[i].DebugAdvanceMovement(seconds);
        }

        public List<NpcTravelWaypoint> BuildNpcTravelRoute(string fromPlace, Vector2 fromPosition,
            string targetPlace, Vector2 targetPosition)
        {
            var route = new List<NpcTravelWaypoint>();
            if (string.IsNullOrWhiteSpace(fromPlace))
                fromPlace = "Street";
            if (string.IsNullOrWhiteSpace(targetPlace))
                targetPlace = "Street";

            if (fromPlace == targetPlace)
            {
                route.Add(new NpcTravelWaypoint(targetPlace, targetPosition));
                return route;
            }

            string routePlace = fromPlace;
            if (routePlace != "Street")
            {
                Vector2 currentExit = InteriorExitFor(routePlace);
                if ((fromPosition - currentExit).sqrMagnitude > 0.01f)
                    route.Add(new NpcTravelWaypoint(routePlace, currentExit));
                route.Add(new NpcTravelWaypoint("Street", StreetDoorFor(routePlace)));
            }

            if (targetPlace != "Street")
            {
                Vector2 targetStreetDoor = StreetDoorFor(targetPlace);
                Vector2 routeEnd = route.Count > 0 ? route[route.Count - 1].Position : fromPosition;
                if ((routeEnd - targetStreetDoor).sqrMagnitude > 0.01f)
                    route.Add(new NpcTravelWaypoint("Street", targetStreetDoor));

                route.Add(new NpcTravelWaypoint(targetPlace, InteriorEntryFor(targetPlace)));
            }

            route.Add(new NpcTravelWaypoint(targetPlace, targetPosition));
            return route;
        }

        public bool DebugFocusResident(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            if (resident == null)
                return false;

            string place = resident.CurrentPlace;
            Vector2 position = resident.Position + new Vector2(-1.1f, 0f);
            return TryRestorePlace(place, position);
        }

        public string DebugResidentSummary(string npcId)
        {
            NpcPresenter resident = FindResident(npcId);
            return resident == null ? npcId + ": unavailable" :
                resident.DisplayName + " — " + resident.Activity +
                (resident.IsTravelling ? " [travelling]" : string.Empty) + " @ " + resident.CurrentPlace +
                " (" + resident.Position.x.ToString("0.0") + ", " +
                resident.Position.y.ToString("0.0") + ")";
        }

        public void Build(Transform player, Rigidbody2D body, Camera camera, GameClock clock = null,
            NpcCharacterDefinition[] definitions = null)
        {
            if (definitions != null)
            {
                characterDefinitions.Clear();
                characterDefinitions.AddRange(definitions);
            }

            if (_clock != null)
                _clock.Changed -= OnClockChanged;

            _player = player;
            _clock = clock ?? new GameClock();
            _socialWorld = new NpcSocialWorld();
            _socialEncounterSystem = new NpcSocialEncounterSystem();
            _body = body;
            _cameraFollow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;

            _streetRoot = CreateRoot("CityBlock");
            BuildStreet(_streetRoot.transform);

            _shopRoot = CreateRoot("CornerShopInterior");
            BuildInterior(_shopRoot.transform, new Vector2(1000f, 1000f), false);

            _apartmentRoot = CreateRoot("ApartmentInterior");
            BuildInterior(_apartmentRoot.transform, new Vector2(1020f, 1000f), true);

            _navigationMap = CreateNavigationMap(player);
            _navigationDebugView = gameObject.AddComponent<CityNavigationDebugView>();
            _navigationDebugView.Bind(_navigationMap, this, _player, camera);

            // Capture collider geometry while all generated locations are active.
            _streetRoot.SetActive(false);
            _shopRoot.SetActive(false);
            _apartmentRoot.SetActive(false);

            _place = Place.Hideout;
            SetResidentsActivePlace(_place.ToString());
            _clock.Changed += OnClockChanged;
            if (_cameraFollow != null)
                _cameraFollow.Configure(_player, _worldBounds);
        }

        private CityNavigationMap CreateNavigationMap(Transform player)
        {
            var map = new CityNavigationMap();
            RoomBootstrap room = player != null ? player.GetComponentInParent<RoomBootstrap>() : null;
            Rect hideoutBounds = room != null ? room.Bounds : new Rect(-1f, -1f, 22f, 14f);
            // Include the doorway threshold, which sits half a tile beyond the room's tile-coordinate bounds.
            hideoutBounds = new Rect(hideoutBounds.xMin - 0.5f, hideoutBounds.yMin - 0.5f,
                hideoutBounds.width, hideoutBounds.height);
            map.AddRegion("Hideout", hideoutBounds);
            map.AddRegion("Street", new Rect(-9f, -26f, 38f, 27f));
            map.AddRegion("CornerShop", InteriorBounds(new Vector2(1000f, 1000f)));
            map.AddRegion("Apartment", InteriorBounds(new Vector2(1020f, 1000f)));

            if (room != null)
            {
                AddNavigationObstacles(map, "Hideout", room.transform);
                AddHideoutBoundaryObstacles(map, room.Bounds);
            }
            AddNavigationObstacles(map, "Street", _streetRoot.transform);
            AddNavigationObstacles(map, "CornerShop", _shopRoot.transform);
            AddNavigationObstacles(map, "Apartment", _apartmentRoot.transform);

            map.AddConnection("hideout-street-door", "Hideout", HomeDoor,
                "Street", new Vector2(10f, -4f));
            map.AddConnection("street-shop-door", "Street", new Vector2(0.5f, -12f),
                "CornerShop", new Vector2(1000f, 995.8f));
            map.AddConnection("street-apartment-door", "Street", new Vector2(19.5f, -12f),
                "Apartment", new Vector2(1020f, 995.8f));
            return map;
        }

        private static void AddHideoutBoundaryObstacles(CityNavigationMap map, Rect roomBounds)
        {
            // RoomBootstrap builds a tilemap perimeter with a single open doorway in its south wall.
            // Describe those wall runs as separate rectangles so the doorway remains genuinely open.
            float left = roomBounds.xMin - 0.5f;
            float bottom = roomBounds.yMin - 0.5f;
            float width = roomBounds.width;
            float height = roomBounds.height;
            float doorCentre = roomBounds.center.x;
            map.AddObstacle("Hideout", "North wall", new Rect(left, bottom + height - 1f, width, 1f));
            map.AddObstacle("Hideout", "West wall", new Rect(left, bottom + 1f, 1f, height - 2f));
            map.AddObstacle("Hideout", "East wall", new Rect(left + width - 1f, bottom + 1f, 1f, height - 2f));
            map.AddObstacle("Hideout", "South wall west",
                new Rect(left, bottom, doorCentre - 0.5f - left, 1f));
            map.AddObstacle("Hideout", "South wall east",
                new Rect(doorCentre + 0.5f, bottom, left + width - doorCentre - 0.5f, 1f));
        }

        private static void AddNavigationObstacles(CityNavigationMap map, string place, Transform root)
        {
            if (root == null)
                return;

            Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                if (collider == null || !collider.enabled || collider.isTrigger ||
                    collider is TilemapCollider2D ||
                    collider.GetComponentInParent<PlayerMotor2D>() != null)
                    continue;

                Bounds bounds = collider.bounds;
                map.AddObstacle(place, collider.gameObject.name,
                    new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
            }
        }

        public bool TryRestorePlace(string place, Vector2 position)
        {
            if (!System.Enum.TryParse(place, true, out Place restored) || !System.Enum.IsDefined(typeof(Place), restored))
                return false;

            Rect bounds;
            switch (restored)
            {
                case Place.Hideout:
                    bounds = _worldBounds;
                    break;
                case Place.Street:
                    bounds = new Rect(-9f, -26f, 38f, 27f);
                    break;
                case Place.CornerShop:
                    bounds = InteriorBounds(new Vector2(1000f, 1000f));
                    break;
                case Place.Apartment:
                    bounds = InteriorBounds(new Vector2(1020f, 1000f));
                    break;
                default:
                    return false;
            }

            SetPlace(restored, position, bounds);
            return true;
        }

        public void PublishWorldEvent(string eventId, string kind, string description, Vector2 position, float visibilityRadius, string actorNpcId = null)
        {
            PublishWorldEventAt(_place.ToString(), eventId, kind, description, position, visibilityRadius, actorNpcId);
        }

        public void PublishWorldEventAt(string locationId, string eventId, string kind, string description,
            Vector2 position, float visibilityRadius, string actorNpcId = null)
        {
            if (_socialWorld == null || _clock == null)
                return;

            _socialWorld.PublishEvent(new NpcWorldEvent(eventId, kind, description, actorNpcId,
                locationId, position.x, position.y, visibilityRadius, _clock.Day, _clock.MinuteOfDay));
        }

        public string GetPrompt(Vector2 playerPosition)
        {
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i].CanTalkFrom(playerPosition))
                    return "E  Talk to " + _residents[i].DisplayName;

            switch (_place)
            {
                case Place.Hideout:
                    return Near(playerPosition, HomeDoor) ? "E  Leave hideout" : string.Empty;
                case Place.Street:
                    if (Near(playerPosition, HomeDoor))
                        return "E  Return to hideout";
                    if (Near(playerPosition, ShopDoor))
                        return "E  Enter corner shop";
                    if (Near(playerPosition, ApartmentDoor))
                        return "E  Enter apartment";
                    return string.Empty;
                case Place.CornerShop:
                case Place.Apartment:
                    Vector2 exit = InteriorExit;
                    return Near(playerPosition, exit) ? "E  Return to street" : string.Empty;
                default:
                    return string.Empty;
            }
        }

        public bool TryInteractNpc(Vector2 playerPosition, out string dialogue)
        {
            dialogue = null;
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i].TryTalk(playerPosition, out dialogue))
                    return true;
            return false;
        }

        public bool TryInteract(Vector2 playerPosition)
        {
            switch (_place)
            {
                case Place.Hideout:
                    if (!Near(playerPosition, HomeDoor)) return false;
                    SetPlace(Place.Street, new Vector2(10f, -4f), new Rect(-9f, -26f, 38f, 27f));
                    return true;

                case Place.Street:
                    if (Near(playerPosition, HomeDoor))
                    {
                        SetPlace(Place.Hideout, new Vector2(10f, -0.15f), _worldBounds);
                        return true;
                    }
                    if (Near(playerPosition, ShopDoor))
                    {
                        SetPlace(Place.CornerShop, new Vector2(1000f, 997.4f), InteriorBounds(new Vector2(1000f, 1000f)));
                        return true;
                    }
                    if (Near(playerPosition, ApartmentDoor))
                    {
                        SetPlace(Place.Apartment, new Vector2(1020f, 997.4f), InteriorBounds(new Vector2(1020f, 1000f)));
                        return true;
                    }
                    return false;

                case Place.CornerShop:
                case Place.Apartment:
                    if (!Near(playerPosition, InteriorExit)) return false;
                    Vector2 returnPosition = _place == Place.CornerShop
                        ? new Vector2(0.5f, -13.2f)
                        : new Vector2(19.5f, -13.2f);
                    SetPlace(Place.Street, returnPosition, new Rect(-9f, -26f, 38f, 27f));
                    return true;

                default:
                    return false;
            }
        }

        private Vector2 InteriorExit => (_place == Place.CornerShop
            ? new Vector2(1000f, 995.8f)
            : new Vector2(1020f, 995.8f));

        private void SetPlace(Place next, Vector2 position, Rect cameraBounds)
        {
            _place = next;
            _streetRoot.SetActive(next == Place.Street);
            _shopRoot.SetActive(next == Place.CornerShop);
            _apartmentRoot.SetActive(next == Place.Apartment);

            _body.position = position;
            _player.position = new Vector3(position.x, position.y, 0f);
            _body.linearVelocity = Vector2.zero;

            if (_cameraFollow != null)
                _cameraFollow.Configure(_player, cameraBounds);

            SetResidentsActivePlace(next.ToString());
        }

        private void SetResidentsActivePlace(string place)
        {
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i] != null)
                    _residents[i].SetActivePlace(place);
        }

        private void OnClockChanged(GameClock clock)
        {
            ProcessSocialEncounters(clock);
        }

        private void ProcessSocialEncounters(GameClock clock)
        {
            if (_debugSimulationPaused || _socialEncounterSystem == null || _socialWorld == null)
                return;

            var socializingNpcIds = new List<string>();
            for (int i = 0; i < _residents.Count; i++)
            {
                NpcPresenter resident = _residents[i];
                if (resident != null && resident.Activity == NpcActivity.Socialize &&
                    !resident.IsTravelling && resident.CurrentPlace == "Street")
                    socializingNpcIds.Add(resident.NpcId);
            }

            _socialEncounterSystem.AdvanceTo(_socialWorld, socializingNpcIds,
                clock.Day, clock.MinuteOfDay);
        }

        private static Vector2 InteriorExitFor(string place)
        {
            switch (place)
            {
                case "CornerShop": return new Vector2(1000f, 995.8f);
                case "Apartment": return new Vector2(1020f, 995.8f);
                case "Hideout": return HomeDoor;
                default: return Vector2.zero;
            }
        }

        private static Vector2 StreetDoorFor(string place)
        {
            switch (place)
            {
                case "CornerShop": return new Vector2(0.5f, -12f);
                case "Apartment": return new Vector2(19.5f, -12f);
                case "Hideout": return new Vector2(10f, -4f);
                default: return Vector2.zero;
            }
        }

        private static Vector2 InteriorEntryFor(string place)
        {
            switch (place)
            {
                case "CornerShop": return new Vector2(1000f, 997.4f);
                case "Apartment": return new Vector2(1020f, 997.4f);
                default: return Vector2.zero;
            }
        }

        private static bool Near(Vector2 position, Vector2 target)
        {
            return Vector2.Distance(position, target) <= InteractionDistance;
        }

        private static Rect InteriorBounds(Vector2 origin)
        {
            return new Rect(origin.x - 7f, origin.y - 5f, 14f, 10f);
        }

        private GameObject CreateRoot(string objectName)
        {
            var root = new GameObject(objectName);
            root.transform.SetParent(transform, false);
            return root;
        }

        private void BuildStreet(Transform root)
        {
            AddBlock(root, "StreetGround", new Vector2(10f, -13f), new Vector2(38f, 27f), Ground, false, 0);
            AddBlock(root, "Road", new Vector2(10f, -13f), new Vector2(12f, 27f), Asphalt, false, 1);
            AddBlock(root, "WestSidewalk", new Vector2(2.5f, -13f), new Vector2(3.5f, 27f), Concrete, false, 2);
            AddBlock(root, "EastSidewalk", new Vector2(17.5f, -13f), new Vector2(3.5f, 27f), Concrete, false, 2);

            for (int y = -24; y <= -4; y += 5)
                AddBlock(root, "LaneMarking", new Vector2(10f, y), new Vector2(0.12f, 2.2f), new Color32(185, 174, 139, 255), false, 3);

            BuildStreetBuilding(root, "CornerShop", new Vector2(-3.5f, -7.5f), new Color32(111, 86, 72, 255), new Color32(180, 129, 79, 255));
            BuildStreetBuilding(root, "Apartment", new Vector2(23.5f, -7.5f), new Color32(76, 91, 103, 255), new Color32(126, 151, 151, 255));

            for (int y = -5; y >= -22; y -= 6)
            {
                AddBlock(root, "StreetTreeTrunk", new Vector2(-7f, y), new Vector2(0.45f, 0.9f), new Color32(86, 64, 48, 255), true, 4);
                AddBlock(root, "StreetTreeCanopy", new Vector2(-7f, y + 0.65f), new Vector2(1.8f, 1.6f), new Color32(58, 91, 67, 255), false, 5);
                AddBlock(root, "StreetTreeTrunk", new Vector2(27f, y), new Vector2(0.45f, 0.9f), new Color32(86, 64, 48, 255), true, 4);
                AddBlock(root, "StreetTreeCanopy", new Vector2(27f, y + 0.65f), new Vector2(1.8f, 1.6f), new Color32(58, 91, 67, 255), false, 5);
            }

            AddBlock(root, "CornerShopDoor", ShopDoor, new Vector2(0.8f, 1.25f), new Color32(62, 49, 39, 255), false, 6);
            AddBlock(root, "ApartmentDoor", ApartmentDoor, new Vector2(0.8f, 1.25f), new Color32(49, 60, 66, 255), false, 6);
            AddBlock(root, "HideoutEntryMarker", HomeDoor, new Vector2(2.2f, 0.18f), new Color32(173, 139, 89, 255), false, 4);
            AddBlock(root, "StreetBenchSeat", new Vector2(3.5f, -17.5f), new Vector2(1.8f, 0.28f), new Color32(112, 81, 58, 255), false, 6);
            AddBlock(root, "StreetBenchLeg", new Vector2(2.9f, -17.85f), new Vector2(0.18f, 0.45f), new Color32(76, 61, 49, 255), false, 5);
            AddBlock(root, "StreetBenchLeg", new Vector2(4.1f, -17.85f), new Vector2(0.18f, 0.45f), new Color32(76, 61, 49, 255), false, 5);

            SpawnResidents();
        }

        private void SpawnResidents()
        {
            _residents.Clear();
            if (characterDefinitions.Count == 0)
            {
                characterDefinitions.Add(CreateTemporaryDefinition("milo", "Milo", "Baker",
                    "I'm the baker at the corner shop. The ovens are on before most of the street wakes up.",
                    "Those ovens run hot, and my knees complain louder every year.",
                    "Evenings are quieter. Funny how the street changes once the shutters come down.",
                    "Shop's closed for today. I start before sunrise. Catch me when the shutters are up.",
                    new Color32(65, 78, 83, 255), 390));
                NpcCharacterDefinition rhea = CreateTemporaryDefinition("rhea", "Rhea", "Neighbour",
                    "I'm just getting started for the day.",
                    "Taking a breather before I head back out.",
                    "You hear a lot when you spend time around this block.",
                    "I'm home for the evening.",
                    new Color32(89, 72, 91, 255), 540);
                rhea.WorkPosition = new Vector2(1001.2f, 1000.4f);
                rhea.RestPosition = new Vector2(4.8f, -17.5f);
                rhea.SocialPosition = new Vector2(17.2f, -11.6f);
                rhea.HomePosition = new Vector2(1018f, 1000f);
                characterDefinitions.Add(rhea);
            }

            for (int i = 0; i < characterDefinitions.Count; i++)
            {
                NpcCharacterDefinition definition = characterDefinitions[i];
                if (definition == null)
                    continue;

                var residentObject = new GameObject(definition.DisplayName + " - " + definition.PublicRole);
                residentObject.transform.SetParent(transform, false);
                NpcPresenter presenter = residentObject.AddComponent<NpcPresenter>();
                presenter.Bind(definition, _clock, _socialWorld, this);
                _residents.Add(presenter);
            }
        }

        private NpcCharacterDefinition CreateTemporaryDefinition(string id, string displayName, string role,
            string work, string rest, string social, string home, Color coat, int workStartMinute)
        {
            NpcCharacterDefinition definition = ScriptableObject.CreateInstance<NpcCharacterDefinition>();
            definition.NpcId = id;
            definition.DisplayName = displayName;
            definition.PublicRole = role;
            definition.AwarenessRadius = 8f;
            definition.WalkingSpeed = 1.6f;
            definition.UtilityProfile = NpcUtilityProfile.CreateDefault();
            definition.UtilityProfile.WorkStartMinute = workStartMinute;
            definition.WorkDialogue = work;
            definition.RestDialogue = rest;
            definition.SocialDialogue = social;
            definition.HomeDialogue = home;
            definition.CoatColor = coat;
            _temporaryDefinitions.Add(definition);
            return definition;
        }

        private NpcPresenter FindResident(string npcId)
        {
            for (int i = 0; i < _residents.Count; i++)
                if (_residents[i] != null && _residents[i].NpcId == npcId)
                    return _residents[i];
            return null;
        }

        private void OnDestroy()
        {
            if (_clock != null)
                _clock.Changed -= OnClockChanged;

            for (int i = 0; i < _temporaryDefinitions.Count; i++)
            {
                if (_temporaryDefinitions[i] == null)
                    continue;
                if (Application.isPlaying)
                    Destroy(_temporaryDefinitions[i]);
                else
                    DestroyImmediate(_temporaryDefinitions[i]);
            }
            _temporaryDefinitions.Clear();
        }

        private static void BuildStreetBuilding(Transform root, string name, Vector2 centre, Color facade, Color trim)
        {
            AddBlock(root, name + "Facade", centre, new Vector2(7f, 5.5f), facade, true, 3);
            AddBlock(root, name + "Roofline", centre + new Vector2(0f, 2.45f), new Vector2(7.3f, 0.45f), trim, false, 4);
            AddBlock(root, name + "WindowLeft", centre + new Vector2(-2f, -0.8f), new Vector2(1.25f, 1.5f), new Color32(47, 65, 70, 255), false, 5);
            AddBlock(root, name + "WindowRight", centre + new Vector2(2f, -0.8f), new Vector2(1.25f, 1.5f), new Color32(47, 65, 70, 255), false, 5);
            AddBlock(root, name + "Sign", centre + new Vector2(0f, -2.05f), new Vector2(2.8f, 0.48f), trim, false, 6);
        }

        private static void BuildInterior(Transform root, Vector2 origin, bool apartment)
        {
            AddBlock(root, "InteriorFloor", Vector2.zero, new Vector2(14f, 10f), new Color32(91, 83, 73, 255), false, 0);
            AddBlock(root, "BackWall", new Vector2(0f, 4.7f), new Vector2(14f, 0.6f), Wall, true, 2);
            AddBlock(root, "LeftWall", new Vector2(-6.7f, 0f), new Vector2(0.6f, 9.4f), Wall, true, 2);
            AddBlock(root, "RightWall", new Vector2(6.7f, 0f), new Vector2(0.6f, 9.4f), Wall, true, 2);
            AddBlock(root, "FrontWallLeft", new Vector2(-4.2f, -4.7f), new Vector2(5f, 0.6f), Wall, true, 2);
            AddBlock(root, "FrontWallRight", new Vector2(4.2f, -4.7f), new Vector2(5f, 0.6f), Wall, true, 2);

            if (apartment)
            {
                AddBlock(root, "Bed", new Vector2(-3.7f, 2.2f), new Vector2(2.7f, 1.6f), new Color32(92, 78, 76, 255), true, 3);
                AddBlock(root, "KitchenTable", new Vector2(2.8f, 1.5f), new Vector2(2.4f, 1.2f), new Color32(116, 84, 59, 255), true, 3);
                AddBlock(root, "Wardrobe", new Vector2(4.8f, 3.2f), new Vector2(1.2f, 2f), new Color32(78, 73, 64, 255), true, 3);
            }
            else
            {
                AddBlock(root, "ShopCounter", new Vector2(0f, 2.2f), new Vector2(5.8f, 1.2f), new Color32(110, 78, 55, 255), true, 3);
                AddBlock(root, "ShelfLeft", new Vector2(-4.8f, 0.5f), new Vector2(1.2f, 3f), new Color32(86, 76, 62, 255), true, 3);
                AddBlock(root, "ShelfRight", new Vector2(4.8f, 0.5f), new Vector2(1.2f, 3f), new Color32(86, 76, 62, 255), true, 3);
                AddBlock(root, "BackRoomDoor", new Vector2(0f, 4.15f), new Vector2(1.1f, 0.2f), new Color32(55, 48, 42, 255), false, 4);
                AddBlock(root, "BreadTray", new Vector2(0f, 3.05f), new Vector2(2.3f, 0.28f), new Color32(112, 75, 48, 255), false, 5);
                AddBlock(root, "BreadLoafLeft", new Vector2(-0.62f, 3.27f), new Vector2(0.62f, 0.28f), new Color32(190, 143, 82, 255), false, 6);
                AddBlock(root, "BreadLoafRight", new Vector2(0.38f, 3.27f), new Vector2(0.62f, 0.28f), new Color32(190, 143, 82, 255), false, 6);
            }

            root.position = new Vector3(origin.x, origin.y, 0f);
        }

        private static GameObject AddBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool solid, int sortingOrder)
        {
            var block = new GameObject(name);
            block.transform.SetParent(parent, false);
            block.transform.localPosition = new Vector3(position.x, position.y, 0f);
            block.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;

            if (solid)
                block.AddComponent<BoxCollider2D>();

            return block;
        }
    }
}