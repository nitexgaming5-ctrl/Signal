using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prototype day transition input. Space is the first stand-in for the future
/// Sleep action or a bed interaction, keeping the day boundary explicit.
/// </summary>
public sealed class DayCycle : MonoBehaviour
{
    [SerializeField]
    private Key sleepKey = Key.Space;

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current[sleepKey].wasPressedThisFrame)
            return;

        Sleep();
    }

    /// <summary>Advances time. Bed and interaction systems can call this later.</summary>
    public void Sleep()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Cannot sleep: no GameManager exists in the scene.");
            return;
        }

        GameManager.Instance.NextDay();
        Debug.Log($"Player slept → Day {GameManager.Instance.CurrentDay}");
    }
}

