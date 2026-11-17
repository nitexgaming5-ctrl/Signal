using UnityEngine;

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
        private NeighbourNpc _neighbour;
        private GameClock _clock;

        private static readonly Vector2 HomeDoor = new Vector2(10f, -1.5f);
        private static readonly Vector2 ShopDoor = new Vector2(0.5f, -10.8f);
        private static readonly Vector2 ApartmentDoor = new Vector2(19.5f, -10.8f);
        private const float InteractionDistance = 1.85f;

        public bool IsAtHome => _place == Place.Hideout;
        public string CurrentPlace => _place.ToString();
        public Vector2 NeighbourPosition => _neighbour != null ? _neighbour.Position : Vector2.zero;
        public NpcActivity NeighbourActivity => _neighbour != null ? _neighbour.Activity : NpcActivity.Home;

        public void Build(Transform player, Rigidbody2D body, Camera camera, GameClock clock = null)
        {
            _player = player;
            _clock = clock ?? new GameClock();
            _body = body;
            _cameraFollow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;

            _streetRoot = CreateRoot("CityBlock");
            BuildStreet(_streetRoot.transform);
            _streetRoot.SetActive(false);

            _shopRoot = CreateRoot("CornerShopInterior");
            BuildInterior(_shopRoot.transform, new Vector2(1000f, 1000f), false);
            _shopRoot.SetActive(false);

            _apartmentRoot = CreateRoot("ApartmentInterior");
            BuildInterior(_apartmentRoot.transform, new Vector2(1020f, 1000f), true);
            _apartmentRoot.SetActive(false);

            _place = Place.Hideout;
            if (_cameraFollow != null)
                _cameraFollow.Configure(_player, _worldBounds);
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

        public string GetPrompt(Vector2 playerPosition)
        {
            switch (_place)
            {
                case Place.Hideout:
                    return Near(playerPosition, HomeDoor) ? "E  Leave hideout" : string.Empty;
                case Place.Street:
                    if (_neighbour != null && _neighbour.CanTalkFrom(playerPosition))
                        return "E  Talk to Milo";
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
            return _place == Place.Street && _neighbour != null &&
                   _neighbour.TryTalk(playerPosition, out dialogue);
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

            var npcObject = new GameObject("Milo - Neighbourhood Baker");
            npcObject.transform.SetParent(root, false);
            _neighbour = npcObject.AddComponent<NeighbourNpc>();
            _neighbour.Bind(_clock);
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