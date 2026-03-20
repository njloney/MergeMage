using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Freeze")]
public class FreezeEffect : StatusEffect
{
    [SerializeField] private bool restoreOnExpire = true;

    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        var slime = target.GetComponent<EnemySlime>();
        var idol = target.GetComponent<EnemyIdol>();

        if (slime != null || idol != null)
        {
            var backup = target.GetComponent<EnemySpeedBackup>();
            if (backup == null)
                backup = target.gameObject.AddComponent<EnemySpeedBackup>();

            if (!backup.hasOriginal)
            {
                backup.originalMaxSpeed = slime != null ? slime.maxSpeed : idol.maxSpeed;
                backup.hasOriginal = true;
            }

            if (slime != null)
                slime.maxSpeed = 0f;

            if (idol != null)
                idol.maxSpeed = 0f;

            Debug.Log($"[FreezeEffect] Applied to {target.name}, base {backup.originalMaxSpeed}, now 0");
            return;
        }

        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods == null)
            mods = target.gameObject.AddComponent<MovementModifiersPlaceholder>();

        runtime.magnitude = mods.speedMultiplier <= 0f ? 1f : mods.speedMultiplier;
        mods.speedMultiplier = 0f;

        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
        {
            runtime.stacks = atk.attackPower;
            atk.attackPower = 0;
        }

        Debug.Log($"[FreezeEffect] Applied via MovementModifiers to {target.name}");
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        if (!restoreOnExpire)
            return;

        var slime = target.GetComponent<EnemySlime>();
        var idol = target.GetComponent<EnemyIdol>();
        var backup = target.GetComponent<EnemySpeedBackup>();

        if (backup != null && backup.hasOriginal && (slime != null || idol != null))
        {
            if (slime != null)
                slime.maxSpeed = backup.originalMaxSpeed;

            if (idol != null)
                idol.maxSpeed = backup.originalMaxSpeed;

            Debug.Log($"[FreezeEffect] Expired on {target.name}, restored {backup.originalMaxSpeed}");
            backup.hasOriginal = false;
            return;
        }

        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods != null)
            mods.speedMultiplier = runtime.magnitude > 0f ? runtime.magnitude : 1f;

        if (target.TryGetComponent<AttackPlaceholder>(out var atk))
            atk.attackPower = runtime.stacks;

        Debug.Log($"[FreezeEffect] Expired via MovementModifiers on {target.name}");
    }
}
