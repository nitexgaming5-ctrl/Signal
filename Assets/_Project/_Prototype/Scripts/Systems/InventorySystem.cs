using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Small serializable inventory foundation for resources and future items.</summary>
public sealed class InventorySystem : MonoBehaviour
{
    [Serializable]
    public sealed class Stack
    {
        public string itemId;
        [Min(0)] public int quantity;

        public Stack(string id, int amount)
        {
            itemId = id;
            quantity = amount;
        }
    }

    [SerializeField]
    private List<Stack> items = new List<Stack>();

    public IReadOnlyList<Stack> Items => items;
    public event Action Changed;

    private void Awake()
    {
        if (items.Count == 0)
        {
            AddItem("Water", 2);
            AddItem("Scrap", 3);
        }
    }

    /// <summary>Replaces current stacks with a sanitized save snapshot.</summary>
    public void RestoreItems(List<Stack> savedItems)
    {
        items.Clear();
        if (savedItems != null)
        {
            foreach (var stack in savedItems)
            {
                if (stack != null && !string.IsNullOrWhiteSpace(stack.itemId) && stack.quantity > 0)
                    items.Add(new Stack(stack.itemId, stack.quantity));
            }
        }
        Changed?.Invoke();
    }

    public int GetQuantity(string itemId)
    {
        var stack = FindStack(itemId);
        return stack == null ? 0 : stack.quantity;
    }

    public void AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return;

        var stack = FindStack(itemId);
        if (stack == null)
            items.Add(new Stack(itemId, amount));
        else
            stack.quantity += amount;

        Changed?.Invoke();
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        var stack = FindStack(itemId);
        if (stack == null || amount <= 0 || stack.quantity < amount)
            return false;

        stack.quantity -= amount;
        if (stack.quantity == 0)
            items.Remove(stack);

        Changed?.Invoke();
        return true;
    }

    private Stack FindStack(string itemId)
    {
        return items.Find(stack => string.Equals(stack.itemId, itemId, StringComparison.OrdinalIgnoreCase));
    }
}
