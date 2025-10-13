using UnityEngine;

[System.Serializable]
public struct EffectSpec
{
    public StatusEffect effect;
    public float duration;   // seconds
    public float magnitude;  // DPS for burn, % for slow, etc.
}
