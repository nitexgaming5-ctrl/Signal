using System;
using System.Collections.Generic;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Modular pixel-art player renderer. All body parts share one direction and frame clock,
    /// preventing animation drift. Sprite sheets are imported by the Signal character-pack tool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerCharacterVisual : MonoBehaviour
    {
        private const int FrameWidth = 96;
        private const int FrameHeight = 128;
        private const float PixelsPerUnit = 96f;
        private const float VisualScale = 1.8f; // Enlarge artwork without changing player physics.
        private static readonly string[] Parts = { "Legs", "Arm_L", "Chest", "Arm_R", "Head" };

        private readonly Dictionary<string, SpriteRenderer> _renderers =
            new Dictionary<string, SpriteRenderer>(StringComparer.Ordinal);
        private readonly Dictionary<string, Sprite[]> _sprites =
            new Dictionary<string, Sprite[]>(StringComparer.Ordinal);

        private Rigidbody2D _body;
        private string _facing = "Down";
        private bool _facingLeft;
        private bool _ready;
        private int _lastFrame = -1;
        private string _lastAppliedKey;
        private bool _lastAppliedFacingLeft;

        public bool IsReady => _ready;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _ready = LoadRequiredSheets();
            if (_ready)
                ApplyPose(false);
        }

        private void Update()
        {
            if (!_ready || _body == null)
                return;

            Vector2 velocity = _body.linearVelocity;
            bool moving = velocity.sqrMagnitude > 0.025f;
            if (moving)
            {
                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                {
                    _facing = "Side";
                    _facingLeft = velocity.x < 0f;
                }
                else
                {
                    _facing = velocity.y > 0f ? "Top" : "Down";
                }
            }

            string action = moving ? "Walk" : "Idle";
            string key = action + "_" + _facing;
            if (!_sprites.TryGetValue(Parts[0] + "/" + key, out Sprite[] firstPart) || firstPart.Length == 0)
                return;

            int frame = Mathf.FloorToInt(Time.time * (moving ? 8f : 3f)) % firstPart.Length;
            if (frame != _lastFrame || _lastAppliedKey != key || _lastAppliedFacingLeft != _facingLeft)
            {
                ApplyPose(moving, frame);
                _lastFrame = frame;
                _lastAppliedKey = key;
                _lastAppliedFacingLeft = _facingLeft;
            }
        }

        private bool LoadRequiredSheets()
        {
            string[] actions = { "Idle", "Walk" };
            string[] directions = { "Down", "Side", "Top" };
            foreach (string part in Parts)
            {
                foreach (string action in actions)
                {
                    foreach (string direction in directions)
                    {
                        string key = action + "_" + direction;
                        string resourcePath = "Character/Body_A/" + part + "/" + action + "/" + key;
                        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                        if (texture == null)
                        {
                            Debug.LogWarning("Signal character pack is not imported or is missing: " + resourcePath, this);
                            return false;
                        }

                        texture.filterMode = FilterMode.Point;
                        texture.wrapMode = TextureWrapMode.Clamp;
                        if (texture.height < FrameHeight || texture.width < FrameWidth || texture.width % FrameWidth != 0)
                        {
                            Debug.LogWarning("Unexpected character sheet dimensions for " + resourcePath +
                                ". Expected horizontal 96x128 frames; got " + texture.width + "x" + texture.height, this);
                            return false;
                        }

                        int frameCount = texture.width / FrameWidth;
                        var frames = new Sprite[frameCount];
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            var rect = new Rect(frame * FrameWidth, 0, FrameWidth, FrameHeight);
                            frames[frame] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.04f),
                                PixelsPerUnit, 0, SpriteMeshType.FullRect);
                            frames[frame].name = part + "_" + key + "_" + frame;
                        }
                        _sprites.Add(part + "/" + key, frames);
                    }
                }
            }

            foreach (string part in Parts)
            {
                var layer = new GameObject("Visual_" + part);
                layer.transform.SetParent(transform, false);
                layer.transform.localScale = Vector3.one * VisualScale;
                var renderer = layer.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 10;
                _renderers.Add(part, renderer);
            }
            return true;
        }

        private void ApplyPose(bool moving, int frame = 0)
        {
            string key = (moving ? "Walk" : "Idle") + "_" + _facing;
            foreach (string part in Parts)
            {
                if (!_sprites.TryGetValue(part + "/" + key, out Sprite[] frames) || frames.Length == 0)
                    continue;
                SpriteRenderer renderer = _renderers[part];
                renderer.sprite = frames[frame % frames.Length];
                renderer.flipX = _facing == "Side" && _facingLeft;
                renderer.sortingOrder = 10 + GetLayerOrder(part, _facing, _facingLeft);
            }
        }

        private static int GetLayerOrder(string part, string facing, bool facingLeft)
        {
            if (facing == "Down")
            {
                if (part == "Legs") return 0;
                if (part == "Arm_L") return 1;
                if (part == "Chest") return 2;
                if (part == "Arm_R") return 3;
                return 4;
            }

            if (facing == "Top")
            {
                if (part == "Legs") return 0;
                if (part == "Arm_L") return 1;
                if (part == "Arm_R") return 2;
                if (part == "Chest") return 3;
                return 4;
            }

            if (part == "Legs") return 0;
            bool nearArm = facingLeft ? part == "Arm_L" : part == "Arm_R";
            if (part == "Chest") return 2;
            if (part == "Head") return 3;
            return nearArm ? 4 : 1;
        }

        private void OnDestroy()
        {
            foreach (Sprite[] frames in _sprites.Values)
            {
                foreach (Sprite sprite in frames)
                {
                    if (sprite == null) continue;
                    if (Application.isPlaying) Destroy(sprite);
                    else DestroyImmediate(sprite);
                }
            }
        }
    }
}
