using UnityEngine;
using UnityEngine.Tilemaps;

namespace Signal.Game
{
    /// <summary>
    /// Builds the first walkable hideout room, its starter furniture and a player.
    /// The street-facing doorway is left physically open for the connected city slice.
    /// </summary>
    public sealed class RoomBootstrap : MonoBehaviour
    {
        [SerializeField] private int roomWidth = 20;
        [SerializeField] private int roomHeight = 12;
        [SerializeField] private float cameraSize = 7f;

        public Transform Player { get; private set; }
        public Rigidbody2D PlayerBody { get; private set; }
        public Transform Workbench { get; private set; }
        public Transform Storage { get; private set; }
        public Rect Bounds { get; private set; }

        public void Build(Camera followCamera)
        {
            if (Player != null)
                return;

            var gridObject = new GameObject("RoomGrid");
            gridObject.transform.SetParent(transform, false);
            gridObject.AddComponent<Grid>();

            Tilemap floor = CreateLayer(gridObject.transform, "Floor", 0, false);
            Tilemap walls = CreateLayer(gridObject.transform, "Walls", 1, true);

            Tile floorA = NewTile(new Color32(0x55, 0x52, 0x4B, 0xFF), Tile.ColliderType.None);
            Tile floorB = NewTile(new Color32(0x4D, 0x4A, 0x44, 0xFF), Tile.ColliderType.None);
            Tile wall = NewTile(new Color32(0x6A, 0x6C, 0x68, 0xFF), Tile.ColliderType.Grid);

            for (int y = 0; y < roomHeight; y++)
            {
                for (int x = 0; x < roomWidth; x++)
                    floor.SetTile(new Vector3Int(x, y, 0), (x + y) % 2 == 0 ? floorA : floorB);
            }

            int doorX = roomWidth / 2;
            for (int x = -1; x <= roomWidth; x++)
            {
                // Leave the centre of the south wall open so the player can walk outside.
                if (x != doorX)
                    walls.SetTile(new Vector3Int(x, -1, 0), wall);
                walls.SetTile(new Vector3Int(x, roomHeight, 0), wall);
            }

            for (int y = 0; y < roomHeight; y++)
            {
                walls.SetTile(new Vector3Int(-1, y, 0), wall);
                walls.SetTile(new Vector3Int(roomWidth, y, 0), wall);
            }

            Bounds = new Rect(-1f, -1f, roomWidth + 2f, roomHeight + 2f);

            CreateFurniture();
            CreatePlayer();

            if (followCamera != null)
                SetUpCamera(followCamera);
        }

        private void CreateFurniture()
        {
            GameObject bed = CreateFurnitureProp("Bed", new Vector2(15f, 9f), new Vector2(3.2f, 1.65f), new Color32(0x65, 0x50, 0x4B, 0xFF));
            AddBenchDetail(bed.transform, "BedBlanket", new Vector2(0.35f, -0.08f), new Vector2(1.7f, 1.15f), new Color32(0x4F, 0x65, 0x66, 0xFF), 4);
            AddBenchDetail(bed.transform, "BedPillow", new Vector2(-1.05f, 0.28f), new Vector2(0.75f, 0.48f), new Color32(0xC0, 0xB8, 0xA4, 0xFF), 5);

            GameObject table = CreateFurnitureProp("Table", new Vector2(15f, 3f), new Vector2(2.6f, 1.25f), new Color32(0x77, 0x54, 0x3E, 0xFF));
            AddBenchDetail(table.transform, "TableTop", new Vector2(0f, 0.08f), new Vector2(2.42f, 0.92f), new Color32(0x9A, 0x75, 0x55, 0xFF), 4);
            AddBenchDetail(table.transform, "Paper", new Vector2(-0.45f, 0.18f), new Vector2(0.55f, 0.38f), new Color32(0xC0, 0xB8, 0xA4, 0xFF), 5);
            AddBenchDetail(table.transform, "Mug", new Vector2(0.72f, 0.16f), new Vector2(0.24f, 0.28f), new Color32(0x4F, 0x6D, 0x78, 0xFF), 5);

            GameObject chair = CreateFurnitureProp("Chair", new Vector2(12.8f, 3f), new Vector2(0.95f, 0.95f), new Color32(0x5B, 0x43, 0x35, 0xFF));
            AddBenchDetail(chair.transform, "ChairSeat", new Vector2(0f, -0.02f), new Vector2(0.7f, 0.62f), new Color32(0x8B, 0x6A, 0x50, 0xFF), 4);

            GameObject storage = CreateFurnitureProp("Storage", new Vector2(3f, 3f), new Vector2(1.5f, 1.75f), new Color32(0x5D, 0x62, 0x5D, 0xFF));
            Storage = storage.transform;
            AddBenchDetail(storage.transform, "StorageDrawer", new Vector2(0f, 0.35f), new Vector2(1.15f, 0.42f), new Color32(0x77, 0x7B, 0x70, 0xFF), 4);
            AddBenchDetail(storage.transform, "StorageHandle", new Vector2(0f, 0.35f), new Vector2(0.2f, 0.07f), new Color32(0xC0, 0xA5, 0x72, 0xFF), 5);

            GameObject rug = CreateDecorationProp("WornRug", new Vector2(10f, 2f), new Vector2(4.4f, 1.8f), new Color32(0x5A, 0x43, 0x3F, 0xFF), 2);
            AddBenchDetail(rug.transform, "RugPattern", Vector2.zero, new Vector2(3.9f, 1.35f), new Color32(0x78, 0x5B, 0x50, 0xFF), 3);

            GameObject window = CreateDecorationProp("Window", new Vector2(10f, 11.35f), new Vector2(2.4f, 0.85f), new Color32(0x35, 0x52, 0x59, 0xFF), 2);
            AddBenchDetail(window.transform, "WindowMullion", Vector2.zero, new Vector2(0.09f, 0.85f), new Color32(0x8A, 0x8D, 0x85, 0xFF), 3);
            AddBenchDetail(window.transform, "WindowRail", Vector2.zero, new Vector2(2.4f, 0.08f), new Color32(0x8A, 0x8D, 0x85, 0xFF), 3);

            GameObject plant = CreateDecorationProp("Plant", new Vector2(18f, 6.5f), new Vector2(0.8f, 0.8f), new Color32(0x4D, 0x75, 0x59, 0xFF), 3);
            AddBenchDetail(plant.transform, "PlantPot", new Vector2(0f, -0.25f), new Vector2(0.48f, 0.32f), new Color32(0x8A, 0x5C, 0x43, 0xFF), 4);

            GameObject bench = CreateFurnitureProp("Workbench", new Vector2(5f, 9f), new Vector2(3.4f, 1.45f), new Color32(0x7B, 0x58, 0x3F, 0xFF));
            Workbench = bench.transform;
            AddBenchDetail(bench.transform, "WorkbenchTop", new Vector2(0f, 0.2f), new Vector2(3.25f, 0.95f), new Color32(0x9A, 0x75, 0x55, 0xFF), 4);
            AddBenchDetail(bench.transform, "JarA", new Vector2(-0.85f, 0.35f), new Vector2(0.32f, 0.45f), new Color32(0x8E, 0xB4, 0x9D, 0xFF), 5);
            AddBenchDetail(bench.transform, "Notebook", new Vector2(0.45f, 0.38f), new Vector2(0.72f, 0.48f), new Color32(0xC0, 0xA5, 0x72, 0xFF), 5);
            AddBenchDetail(bench.transform, "Tool", new Vector2(1.05f, -0.02f), new Vector2(0.62f, 0.12f), new Color32(0xB7, 0xB9, 0xB2, 0xFF), 5);
        }

        private GameObject CreateDecorationProp(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            var prop = new GameObject(objectName);
            prop.transform.SetParent(transform, false);
            prop.transform.position = new Vector3(position.x, position.y, 0f);
            prop.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;
            return prop;
        }

        private GameObject CreateFurnitureProp(string objectName, Vector2 position, Vector2 size, Color color)
        {
            var prop = new GameObject(objectName);
            prop.transform.SetParent(transform, false);
            prop.transform.position = new Vector3(position.x, position.y, 0f);
            prop.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = 3;

            BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            return prop;
        }

        private static void AddBenchDetail(Transform parent, string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            var detail = new GameObject(objectName);
            detail.transform.SetParent(parent, false);
            Vector3 parentScale = parent.lossyScale;
            detail.transform.localPosition = new Vector3(position.x / parentScale.x, position.y / parentScale.y, -0.05f);
            detail.transform.localScale = new Vector3(size.x / parentScale.x, size.y / parentScale.y, 1f);

            SpriteRenderer renderer = detail.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;
        }

        private void CreatePlayer()
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform, false);
            playerObject.transform.position = new Vector3(roomWidth / 2f, roomHeight / 2f, 0f);
            playerObject.transform.localScale = new Vector3(0.75f, 0.9f, 1f);

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            playerObject.AddComponent<BoxCollider2D>();
            playerObject.AddComponent<PlayerMotor2D>();

            PlayerCharacterVisual characterVisual = playerObject.AddComponent<PlayerCharacterVisual>();
            if (!characterVisual.IsReady)
            {
                var fallback = new GameObject("CharacterPackMissing");
                fallback.transform.SetParent(playerObject.transform, false);
                var fallbackRenderer = fallback.AddComponent<SpriteRenderer>();
                fallbackRenderer.sprite = PixelSprites.Solid(new Color32(0x3E, 0xC9, 0xB9, 0xFF));
                fallbackRenderer.sortingOrder = 10;
            }

            Player = playerObject.transform;
            PlayerBody = body;
        }

        private void SetUpCamera(Camera camera)
        {
            camera.orthographic = true;
            camera.orthographicSize = cameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(0x18, 0x1A, 0x1A, 0xFF);

            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = camera.gameObject.AddComponent<CameraFollow2D>();

            follow.Configure(Player, Bounds);
        }

        private static Tilemap CreateLayer(Transform parent, string layerName, int sortingOrder, bool solid)
        {
            var layer = new GameObject(layerName);
            layer.transform.SetParent(parent, false);

            Tilemap tilemap = layer.AddComponent<Tilemap>();
            TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (solid)
                layer.AddComponent<TilemapCollider2D>();

            return tilemap;
        }

        private static Tile NewTile(Color color, Tile.ColliderType colliderType)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = PixelSprites.Solid(color);
            tile.colliderType = colliderType;
            tile.hideFlags = HideFlags.HideAndDontSave;
            return tile;
        }
    }
}