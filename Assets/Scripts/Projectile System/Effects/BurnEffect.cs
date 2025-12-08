using UnityEngine;

// Deals damage on a fixed interval.
[CreateAssetMenu(menuName = "Combat/Effects/Burn")]
public class BurnEffect : StatusEffect
{
    [Tooltip("Seconds between damage ticks.")]
    [SerializeField] public float tickInterval = 1.0f;

    [Tooltip("If true, stacks increase damage.")]
    [SerializeField] private bool scaleDamageWithStacks = true;

    // If true: magnitude = damage per tick.
    // If false: magnitude = DPS (converted to per-tick damage).
    [SerializeField] private bool magnitudeIsDamagePerTick = false;

    public override void OnTick(StatusController target, ref EffectRuntime runtime, float dt)
    {
        // Track time since last tick.
        runtime.tickElapsed += dt;

        // Run one or more ticks if enough time passed.
        while (runtime.tickElapsed >= tickInterval)
        {
            runtime.tickElapsed -= tickInterval;

            // Compute damage for this tick.
            float amount = magnitudeIsDamagePerTick
                ? runtime.magnitude                         // already per-tick
                : runtime.magnitude * tickInterval;

            if (scaleDamageWithStacks) amount *= runtime.stacks;

            // Apply damage to the target.
            target.Health.TakeDamage(amount, DamageType.Fire, runtime.source);
        }
    }
}
