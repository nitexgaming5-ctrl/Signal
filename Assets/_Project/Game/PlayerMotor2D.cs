using UnityEngine;
using UnityEngine.InputSystem;

namespace Signal.Game
{
    /// <summary>Top-down movement with WASD or the arrow keys (physical key positions, so it also suits AZERTY).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 6f;

        private Rigidbody2D _body;
        private Vector2 _input;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _input = ReadInput();
        }

        private void FixedUpdate()
        {
            if (_body != null)
                _body.linearVelocity = _input * speed;
        }

        private void OnDisable()
        {
            _input = Vector2.zero;

            if (_body != null)
                _body.linearVelocity = Vector2.zero;
        }

        private static Vector2 ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            var direction = new Vector2(x, y);
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }
    }
}
