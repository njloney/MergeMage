using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Blind")]
public class BlindEffect : StatusEffect
{
    [SerializeField] private float blindDuration = 2f; // optional
    [SerializeField] private bool randomizeYawOnApply = true;

    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods == null)
            mods = target.gameObject.AddComponent<MovementModifiersPlaceholder>();

        mods.isBlinded = true;

        if (randomizeYawOnApply)
        {
            float yaw = Random.Range(0f, 360f);
            target.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        var mods = target.GetComponent<MovementModifiersPlaceholder>();
        if (mods != null)
            mods.isBlinded = false; // restore normal movement
    }
}
