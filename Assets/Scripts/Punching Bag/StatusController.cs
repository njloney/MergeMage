using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class StatusController : MonoBehaviour
{
    // Active effects currently running on this object
    private readonly List<EffectRuntime> _active = new();

    // Cached reference to this object's Health
    private Health _health;
    private void Awake() => _health = GetComponent<Health>();

    // Expose Health so effects can deal damage, heal, etc.
    public Health Health => _health;

    public void ApplyEffect(StatusEffect effect, float duration, float magnitude, Object source = null)
    {
        // If only one of this effect is allowed at a time, try to find it
        if (effect.UniquePerTarget)
        {
            int i = _active.FindIndex(r => r.effect == effect);
            if (i >= 0)
            {
                // We already have this effect; handle how it stacks/reapplies
                var rt = _active[i];
                switch (effect.StackingMode)
                {
                    case EffectStackingMode.RefreshDuration:
                        // Just refresh the timer
                        rt.durationRemaining = duration;
                        break;

                    case EffectStackingMode.StackMagnitude:
                        // Increase power and keep the longer remaining duration
                        rt.stacks++;
                        rt.magnitude += magnitude;
                        rt.durationRemaining = Mathf.Max(rt.durationRemaining, duration);
                        break;

                    case EffectStackingMode.IgnoreIfActive:
                        // Do nothing if it's already active
                        return;
                }

                // Let the effect react to being (re)applied
                effect.OnApply(this, ref rt);

                // Write back the modified runtime
                _active[i] = rt;
                return;
            }
        }

        // Create a new runtime entry for a fresh effect
        var runtime = new EffectRuntime
        {
            effect = effect,
            baseDuration = duration,
            durationRemaining = duration,
            magnitude = magnitude,
            stacks = 1,
            source = source
        };

        // One-time setup hook for the effect (e.g., spawn VFX)
        effect.OnApply(this, ref runtime);

        // Track it as active
        _active.Add(runtime);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Loop backwards so we can safely remove expired effects
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var rt = _active[i];

            // Per-frame/tick logic (e.g., apply DoT, slow decay, etc.)
            rt.effect.OnTick(this, ref rt, dt);

            // Countdown the remaining time
            rt.durationRemaining -= dt;

            if (rt.durationRemaining <= 0f)
            {
                // Cleanup hook when the effect ends
                rt.effect.OnExpire(this, ref rt);

                // Remove from the active list
                _active.RemoveAt(i);
            }
            else
            {
                // Write back updated runtime (struct copy)
                _active[i] = rt;
            }
        }
    }
}
