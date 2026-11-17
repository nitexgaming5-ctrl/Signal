using UnityEngine;

/// <summary>
/// Shared authoring surface for prototype interactables.
/// Add a Collider2D to the same object so InteractionController can discover it.
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string prompt = "E  Interact";

    public string Prompt => prompt;
    public virtual bool CanInteract => isActiveAndEnabled;

    public abstract void Interact(InteractionController interactor);

    protected void SetPrompt(string value)
    {
        prompt = value;
    }
}
