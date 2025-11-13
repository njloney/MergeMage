using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Spells/Light Ray Config")]
public class LightRayConfig : ScriptableObject
{
    [Header("Damage")]
    public float dps = 10f;
    public DamageType damageType = DamageType.Light;

    [Header("Behavior")]
    public float maxRange = 30f;
    public float duration = 2f;
    public float tickInterval = 0.1f;   // apply small, frequent ticks
    public LayerMask hitMask = ~0;      // what the beam can hit

    [Header("Aim & Follow")]
    public bool followOrigin = true;    // if true, beam tracks origin’s motion/rotation each frame
    public bool stopOnHit = false;      // if true, beam ends at first collider; else draw full length
}
