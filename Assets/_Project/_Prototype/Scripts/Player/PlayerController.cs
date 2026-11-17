using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Small Rigidbody2D movement prototype using the new Input System's keyboard.
/// Replace the direct keyboard polling with a generated Move action when the
/// player input map is formalized.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = ReadKeyboardInput();
        Vector2 targetPosition = body.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        body.MovePosition(targetPosition);
    }

    private static Vector2 ReadKeyboardInput()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        Vector2 input = Vector2.zero;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input.x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            input.y -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            input.y += 1f;

        return input.sqrMagnitude > 1f ? input.normalized : input;
    }
}
