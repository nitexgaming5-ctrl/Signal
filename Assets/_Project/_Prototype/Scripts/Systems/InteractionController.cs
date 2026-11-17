using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Finds the nearest interactable around the player and handles the E key.
/// The controller reports prompt/feedback events so UI remains optional and
/// interactables never need to know about Canvas objects.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public sealed class InteractionController : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float interactionRadius = 1.35f;

    [SerializeField]
    private LayerMask interactionLayers = Physics2D.AllLayers;

    private InteractableBase currentTarget;

    public event Action<string> PromptChanged;
    public event Action<string> FeedbackRaised;

    public InteractableBase CurrentTarget => currentTarget;

    private void Update()
    {
        RefreshTarget();

        if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            currentTarget.Interact(this);
    }

    public void ShowFeedback(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            FeedbackRaised?.Invoke(message);
    }

    private void RefreshTarget()
    {
        InteractableBase nearest = null;
        var nearestDistance = float.MaxValue;
        var hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactionLayers);

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponentInParent<InteractableBase>();
            if (interactable == null || !interactable.CanInteract)
                continue;

            var distance = (hit.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = interactable;
                nearestDistance = distance;
            }
        }

        if (nearest == currentTarget)
            return;

        currentTarget = nearest;
        PromptChanged?.Invoke(currentTarget == null ? string.Empty : currentTarget.Prompt);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
