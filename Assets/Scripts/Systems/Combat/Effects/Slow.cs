using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Slow")]
public class SlowEffect : StatusEffect
{
    [SerializeField] private float speedMultiplier = 0.4f;

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
                slime.maxSpeed = backup.originalMaxSpeed * speedMultiplier;

            if (idol != null)
                idol.maxSpeed = backup.originalMaxSpeed * speedMultiplier;

            Debug.Log($"[SlowEffect] Applied to {target.name}, base {backup.originalMaxSpeed}, new {(backup.originalMaxSpeed * speedMultiplier)}");
            return;
        }

        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods == null)
            mods = target.gameObject.AddComponent<MovementModifiersPlaceholder>();

        mods.speedMultiplier = Mathf.Min(mods.speedMultiplier, speedMultiplier);
        Debug.Log($"[SlowEffect] Applied via MovementModifiers to {target.name}, mult {mods.speedMultiplier}");
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        var slime = target.GetComponent<EnemySlime>();
        var idol = target.GetComponent<EnemyIdol>();
        var backup = target.GetComponent<EnemySpeedBackup>();

        if (backup != null && backup.hasOriginal && (slime != null || idol != null))
        {
            if (slime != null)
                slime.maxSpeed = backup.originalMaxSpeed;

            if (idol != null)
                idol.maxSpeed = backup.originalMaxSpeed;

            Debug.Log($"[SlowEffect] Expired on {target.name}, restored {backup.originalMaxSpeed}");
            backup.hasOriginal = false;
            return;
        }

        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods != null)
        {
            mods.speedMultiplier = 1f;
            Debug.Log($"[SlowEffect] Expired via MovementModifiers on {target.name}, mult reset to 1");
        }
    }
}
