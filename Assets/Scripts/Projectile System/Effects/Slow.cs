using UnityEngine;

// Reduces movement speed while active.
[CreateAssetMenu(menuName = "Combat/Effects/Slow")]
public class SlowEffect : StatusEffect
{
    [SerializeField] private float speedMultiplier = 0.4f; // 40% speed while slowed

    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods == null) mods = target.gameObject.AddComponent<MovementModifiersPlaceholder>();
        mods.speedMultiplier = Mathf.Min(mods.speedMultiplier, speedMultiplier);
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods != null) mods.speedMultiplier = 1f; // reset to normal
    }
}
