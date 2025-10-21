using UnityEngine;

// Reduces movement to 0 and attack power to 0 while active.
// Uses placeholder scripts until real enemy logic exists.
[CreateAssetMenu(menuName = "Combat/Effects/Freeze")]
public class FreezeEffect : StatusEffect
{
    [SerializeField] private bool restoreOnExpire = true; // restore original values when done

    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        // Try to freeze movement (placeholder)
        if (target.TryGetComponent<MovementPlaceholder>(out var move))
        {
            // Store original in magnitude if you want; here we just use source to stash a tiny holder
            // Minimal approach: stash originals in the runtime via a small holder object.
            runtime.source = runtime.source ?? target; // keep non-null
            // NOTE: In a full system you'd have a safer store, but this keeps it simple.
            move.moveSpeed = 0f; // freeze movement
        }

        // Try to disable attacks (placeholder)
        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
        {
            atk.attackPower = 0; // disable attacks
        }
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        if (!restoreOnExpire) return;

        // Restore placeholders to basic defaults.
        // NOTE: These are placeholders — replace with your real restore logic later.
        if (target.TryGetComponent<MovementPlaceholder>(out var move))
        {
            move.moveSpeed = 5f; // placeholder default; replace with your saved/base value later
        }
        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
        {
            atk.attackPower = 10; // placeholder default; replace later
        }
    }
}
