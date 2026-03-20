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
    public float tickInterval = 0.1f;
    public LayerMask hitMask = ~0;

    [Header("Aim & Follow")]
    public bool followOrigin = true;
    public bool stopOnHit = false;

    [Header("Visuals")]
    [Tooltip("Prefab to spawn at the hit point (sparks, scorch marks, etc).")]
    public GameObject hitEffectPrefab;
}