using UnityEngine;
using System;

/// <summary>
/// Base class for all passive item effects.
/// Extend this to create specific item behaviors (stat mods, on-hit effects, etc.)
/// </summary>
public abstract class PassiveItemEffect : ScriptableObject
{
    [Header("Effect Info")]
    public string effectName;
    [TextArea(3, 5)]
    public string effectDescription;

    [Header("Stacking Behavior")]
    public bool canStack = true;
    public StackingType stackingType = StackingType.Linear;

    public virtual void OnAcquire(GameObject player, int stackCount)
    {
        // Override in derived classes
    }

    public virtual void OnStack(GameObject player, int newStackCount)
    {
        // Override in derived classes
    }

    public virtual void OnRemove(GameObject player)
    {
        // Override in derived classes
    }

    public float GetStackedValue(float baseValue, int stackCount)
    {
        if (stackCount <= 0) return 0f;

        switch (stackingType)
        {
            case StackingType.Linear:
                return baseValue * stackCount;

            case StackingType.Hyperbolic:
                // Formula: 1 - 1/(1 + value * count)
                // Example: 50% dodge becomes 75% at 2 stacks, 87.5% at 3 stacks
                return 1f - (1f / (1f + baseValue * stackCount));

            case StackingType.Diminishing:
                // Each stack is worth 50% of the previous
                float total = 0f;
                float currentValue = baseValue;
                for (int i = 0; i < stackCount; i++)
                {
                    total += currentValue;
                    currentValue *= 0.5f;
                }
                return total;

            default:
                return baseValue * stackCount;
        }
    }
}

public enum StackingType
{
    Linear,      // Each stack adds the same amount (damage, health, etc.)
    Hyperbolic,  // Diminishing returns approaching a limit (dodge chance, crit chance)
    Diminishing  // Each stack worth less than previous (cooldown reduction)
}