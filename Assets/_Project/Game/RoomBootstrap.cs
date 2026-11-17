using UnityEngine;
using UnityEngine.Tilemaps;

namespace Signal.Game
{
    /// <summary>
    /// Builds one walled room from code (placeholder tiles) and a player who can walk around it.
    /// Interior cells run from (0,0) to (width-1, height-1); the walls sit just outside that.
    /// </summary>
    public sealed class RoomBootstrap : MonoBehaviour
    {
        [SerializeField] private int roomWidth = 20;
        [SerializeField] private int roomHeight = 12;
        [SerializeField] private float cameraSize = 7f;

        public Transform Player { get; private set; }

        public Rigidbody2D PlayerBody { get; private set; }

        /// <summary>The room including its walls, in world units.</summary>
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

            Tile floorA = NewTile(new Color32(0x3A, 0x3F, 0x47, 0xFF), Tile.ColliderType.None);
            Tile floorB = NewTile(new Color32(0x34, 0x39, 0x41, 0xFF), Tile.ColliderType.None);
            Tile wall = NewTile(new Color32(0x6B, 0x72, 0x80, 0xFF), Tile.ColliderType.Grid);
            Tile door = NewTile(new Color32(0xC8, 0xA2, 0x4A, 0xFF), Tile.ColliderType.Grid);

            for (int y = 0; y < roomHeight; y++)
            {
                for (int x = 0; x < roomWidth; x++)
                    floor.SetTile(new Vector3Int(x, y, 0), (x + y) % 2 == 0 ? floorA : floorB);
            }

            int doorX = roomWidth / 2;
            for (int x = -1; x <= roomWidth; x++)
            {
                walls.SetTile(new Vector3Int(x, -1, 0), x == doorX ? door : wall);
                walls.SetTile(new Vector3Int(x, roomHeight, 0), wall);
            }

            for (int y = 0; y < roomHeight; y++)
            {
                walls.SetTile(new Vector3Int(-1, y, 0), wall);
                walls.SetTile(new Vector3Int(roomWidth, y, 0), wall);
            }

            Bounds = new Rect(-1f, -1f, roomWidth + 2f, roomHeight + 2f);

            CreatePlayer();

            if (followCamera != null)
                SetUpCamera(followCamera);
        }

        private void CreatePlayer()
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform, false);
            playerObject.transform.position = new Vector3(roomWidth / 2f, roomHeight / 2f, 0f);
            playerObject.transform.localScale = new Vector3(0.75f, 0.9f, 1f);

            var sprite = playerObject.AddComponent<SpriteRenderer>();
            sprite.sprite = PixelSprites.Solid(new Color32(0x3E, 0xE6, 0xD2, 0xFF));
            sprite.sortingOrder = 10;

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            playerObject.AddComponent<BoxCollider2D>();
            playerObject.AddComponent<PlayerMotor2D>();

            Player = playerObject.transform;
            PlayerBody = body;
        }

        private void SetUpCamera(Camera camera)
        {
            camera.orthographic = true;
            camera.orthographicSize = cameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(0x0E, 0x11, 0x16, 0xFF);

            var follow = camera.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = camera.gameObject.AddComponent<CameraFollow2D>();

            follow.Configure(Player, Bounds);
        }

        private static Tilemap CreateLayer(Transform parent, string layerName, int sortingOrder, bool solid)
        {
            var layer = new GameObject(layerName);
            layer.transform.SetParent(parent, false);

            var tilemap = layer.AddComponent<Tilemap>();
            var renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (solid)
                layer.AddComponent<TilemapCollider2D>();

            return tilemap;
        }

        private static Tile NewTile(Color color, Tile.ColliderType colliderType)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = PixelSprites.Solid(color);
            tile.colliderType = colliderType;
            tile.hideFlags = HideFlags.HideAndDontSave;
            return tile;
        }
    }
}
