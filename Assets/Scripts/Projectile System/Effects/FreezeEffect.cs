using UnityEngine;

// Reduces movement to 0 and attack power to 0 while active.
[CreateAssetMenu(menuName = "Combat/Effects/Freeze")]
public class FreezeEffect : StatusEffect
{
    [SerializeField] private bool restoreOnExpire = true; // restore original values when done
    int ap = 0;
    float ms = 0f;
    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        // Try to freeze movement
        if (target.TryGetComponent<MovementPlaceholder>(out var move))
        {
            // Store original in magnitude if you want; here we just use source to stash a tiny holder
            runtime.source = runtime.source ?? target; // keep non-null
            ms = move.maxSpeed;
            move.maxSpeed = 0f; // freeze movement
        }

        // Try to disable attacks
        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
        {
            ap = atk.attackPower;
            atk.attackPower = 0; // disable attacks
        }
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        if (!restoreOnExpire) return;

        // Restore placeholders.
        if (target.TryGetComponent<MovementPlaceholder>(out var move))
        {
            move.maxSpeed = ms;
        }
        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
        {
            atk.attackPower = ap; 
        }
    }
}
