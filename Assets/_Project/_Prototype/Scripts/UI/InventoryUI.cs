using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Toggleable prototype inventory panel using the new Input System.</summary>
public sealed class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Text contentsLabel;
    [SerializeField] private InventorySystem inventory;

    private void Start()
    {
        if (inventory == null)
            inventory = FindAnyObjectByType<InventorySystem>();
        if (inventory != null)
            inventory.Changed += Refresh;

        if (panel != null)
            panel.SetActive(false);
        Refresh();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && panel != null)
        {
            panel.SetActive(!panel.activeSelf);
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (contentsLabel == null || inventory == null)
            return;

        var text = "INVENTORY\n";
        if (inventory.Items.Count == 0)
        {
            text += "EMPTY";
        }
        else
        {
            foreach (var stack in inventory.Items)
                text += $"{stack.itemId.ToUpperInvariant(),-12} {stack.quantity:00}\n";
        }

        contentsLabel.text = text.TrimEnd();
    }
}
