using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Lightweight visual-only renderer for the three temporary city NPC sprite tests.
    /// It reads movement from the existing presenter transform; it owns no NPC behaviour.
    /// </summary>
    public sealed class NpcSpriteSheetVisual : MonoBehaviour
    {
        private const int CellWidth = 48;
        private const int CellHeight = 72;
        private const float PixelsPerUnit = 16f;
        private static readonly string[] ResourceNames =
        {
            "NpcSprites/npc1_hoodie",
            "NpcSprites/npc2_sweater",
            "NpcSprites/npc3_jacket"
        };

        private SpriteRenderer _renderer;
        private Sprite[,] _idle = new Sprite[4, 4];
        private Sprite[,] _walk = new Sprite[4, 6];
        private Vector3 _lastPosition;
        private float _animationClock;
        private string _lastPose;
        private int _lastFrame = -1;

        public SpriteRenderer Renderer => _renderer;

        public static bool AreAssetsAvailable
        {
            get
            {
                for (int i = 0; i < ResourceNames.Length; i++)
                    if (Resources.Load<Texture2D>(ResourceNames[i]) == null)
                        return false;
                return true;
            }
        }

        public bool Initialize(int appearanceIndex)
        {
            if (appearanceIndex < 0 || appearanceIndex >= ResourceNames.Length)
                return false;

            Texture2D texture = Resources.Load<Texture2D>(ResourceNames[appearanceIndex]);
            if (texture == null || texture.width != CellWidth * 6 || texture.height != CellHeight * 12)
            {
                Debug.LogWarning("Signal NPC test sprite sheet is missing or has unexpected dimensions: " +
                    ResourceNames[appearanceIndex]);
                return false;
            }

            texture.filterMode = FilterMode.Point;
            for (int direction = 0; direction < 4; direction++)
            {
                for (int frame = 0; frame < 4; frame++)
                    _idle[direction, frame] = CreateFrame(texture, direction, frame);
                for (int frame = 0; frame < 6; frame++)
                    _walk[direction, frame] = CreateFrame(texture, 4 + direction, frame);
            }

            if (_renderer == null)
                _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = 10;
            _renderer.sprite = _idle[0, 0];
            _lastPosition = transform.position;
            _lastPose = "Idle";
            _lastFrame = 0;
            enabled = true;
            return true;
        }

        private static Sprite CreateFrame(Texture2D texture, int row, int column)
        {
            // Sprite-sheet row zero is at the top; Unity texture coordinates start at bottom-left.
            float y = texture.height - ((row + 1) * CellHeight);
            return Sprite.Create(texture, new Rect(column * CellWidth, y, CellWidth, CellHeight),
                new Vector2(0.5f, 0f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private void LateUpdate()
        {
            if (_renderer == null)
                return;

            Vector3 position = transform.position;
            Vector2 velocity = (position - _lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPosition = position;

            bool moving = velocity.sqrMagnitude > 0.025f;
            int direction = 0;
            if (moving)
            {
                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                    direction = velocity.x < 0f ? 1 : 2;
                else
                    direction = velocity.y > 0f ? 3 : 0;
            }

            string pose = moving ? "Walk" : "Idle";
            int frameCount = moving ? 6 : 4;
            float framesPerSecond = moving ? 10f : 6f;
            _animationClock += Time.deltaTime;
            int frame = Mathf.FloorToInt(_animationClock * framesPerSecond) % frameCount;

            if (pose == _lastPose && frame == _lastFrame && _renderer.sprite != null)
                return;

            _renderer.sprite = moving ? _walk[direction, frame] : _idle[direction, frame];
            _lastPose = pose;
            _lastFrame = frame;
        }

        private void OnDestroy()
        {
            for (int direction = 0; direction < 4; direction++)
            {
                for (int frame = 0; frame < 4; frame++)
                    if (_idle[direction, frame] != null) Destroy(_idle[direction, frame]);
                for (int frame = 0; frame < 6; frame++)
                    if (_walk[direction, frame] != null) Destroy(_walk[direction, frame]);
            }
        }
    }
}
