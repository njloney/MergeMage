using UnityEngine;

// How this effect behaves when applied again.
public enum EffectStackingMode { RefreshDuration, StackMagnitude, IgnoreIfActive }

// Base class for all status effects (Burn, Poison, Slow, etc.).
// StatusController calls these methods.
public abstract class StatusEffect : ScriptableObject
{
    [SerializeField] private string effectId = "effect-id"; // For identification/debug
    [SerializeField] private bool uniquePerTarget = true;    // Only one active on a target?
    [SerializeField] private EffectStackingMode stackingMode = EffectStackingMode.RefreshDuration;

    public string EffectId => effectId;
    public bool UniquePerTarget => uniquePerTarget;
    public EffectStackingMode StackingMode => stackingMode;

    // Called when the effect is applied (or re-applied).
    public virtual void OnApply(StatusController target, ref EffectRuntime runtime) { }

    // Called while the effect is active (every update in StatusController).
    public virtual void OnTick(StatusController target, ref EffectRuntime runtime, float dt) { }

    // Called when the effect ends or is removed.
    public virtual void OnExpire(StatusController target, ref EffectRuntime runtime) { }
}

