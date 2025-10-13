using UnityEngine;

// Holds the live data for a single active effect on a target.
// This gets created when an effect is applied and updated until it expires.
[System.Serializable]
public struct EffectRuntime
{
    public StatusEffect effect;      // The type of effect (Burn, Poison, Slow, etc.)
    public float durationRemaining;  // Time left before the effect ends
    public float baseDuration;       // Original duration when first applied
    public float magnitude;          // Power or intensity of the effect (damage, slow %, etc.)
    public int stacks;               // How many times this effect has stacked
    public Object source;            // Who applied it (player, enemy, boss, etc.)
    public float tickElapsed;        // Time since the last tick (used for periodic effects)
}
