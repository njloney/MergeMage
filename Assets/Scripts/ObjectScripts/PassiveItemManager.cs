using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages all passive items the player has collected.
/// Tracks stacks and triggers effects.
/// ATTACH THIS TO YOUR PLAYER GAMEOBJECT
/// </summary>
public class PassiveItemManager : MonoBehaviour
{
    // Dictionary: ItemData -> stack count
    private Dictionary<ItemData, int> passiveItems = new Dictionary<ItemData, int>();

    // Events for UI updates
    public event Action<ItemData, int> OnPassiveItemAcquired;
    public event Action<ItemData, int> OnPassiveItemStacked;

    /// <summary>
    /// Add a passive item to the player's collection
    /// </summary>
    public void AddPassiveItem(ItemData item)
    {
        if (item == null || item.itemType != ItemType.Passive)
        {
            Debug.LogWarning("Attempted to add non-passive item to PassiveItemManager");
            return;
        }

        if (item.passiveEffect == null)
        {
            Debug.LogError($"Passive item {item.itemName} has no PassiveItemEffect assigned!");
            return;
        }

        // Check if we already have this item
        if (passiveItems.ContainsKey(item))
        {
            // Stack it
            if (item.passiveEffect.canStack)
            {
                passiveItems[item]++;
                int newCount = passiveItems[item];

                item.passiveEffect.OnStack(gameObject, newCount);
                OnPassiveItemStacked?.Invoke(item, newCount);

                Debug.Log($"Stacked {item.itemName} (x{newCount})");
            }
            else
            {
                Debug.Log($"{item.itemName} cannot stack - discarding duplicate");
            }
        }
        else
        {
            // First time acquiring this item
            passiveItems.Add(item, 1);

            item.passiveEffect.OnAcquire(gameObject, 1);
            OnPassiveItemAcquired?.Invoke(item, 1);

            Debug.Log($"Acquired {item.itemName}");
        }
    }

    /// <summary>
    /// Remove a passive item (useful for cursed items or temporary effects)
    /// </summary>
    public void RemovePassiveItem(ItemData item)
    {
        if (passiveItems.ContainsKey(item))
        {
            item.passiveEffect.OnRemove(gameObject);
            passiveItems.Remove(item);
        }
    }

    /// <summary>
    /// Get the stack count for a specific item
    /// </summary>
    public int GetStackCount(ItemData item)
    {
        return passiveItems.ContainsKey(item) ? passiveItems[item] : 0;
    }

    /// <summary>
    /// Check if player has a specific passive item
    /// </summary>
    public bool HasPassiveItem(ItemData item)
    {
        return passiveItems.ContainsKey(item);
    }

    /// <summary>
    /// Get all passive items and their stack counts
    /// </summary>
    public Dictionary<ItemData, int> GetAllPassiveItems()
    {
        return new Dictionary<ItemData, int>(passiveItems);
    }

    /// <summary>
    /// Clear all passive items (for respawn/reset)
    /// </summary>
    public void ClearAllPassiveItems()
    {
        foreach (var kvp in passiveItems)
        {
            kvp.Key.passiveEffect.OnRemove(gameObject);
        }
        passiveItems.Clear();
    }
}