/// <summary>
/// Contract for world objects that can be used by the player.
/// Keeping this small lets beds, workbenches, doors, NPCs, and containers
/// share one interaction flow without coupling them together.
/// </summary>
public interface IInteractable
{
    string Prompt { get; }
    bool CanInteract { get; }
    void Interact(InteractionController interactor);
}
