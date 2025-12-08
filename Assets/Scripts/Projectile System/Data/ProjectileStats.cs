using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Stats")]
public class ProjectileStats : ScriptableObject
{
    [Header("Projectile Motion")]
    public float speed = 20f;
    public float lifetime = 5f;
    public bool useGravity = false;

    [Header("Mana Effects")]
    public float manaCost = 25f;

    [Header("Projectile Visuals")]
    public Material projectileMaterial;
    [Tooltip("VFX to spawn immediately on impact")]
    public GameObject impactVFX;

    [Header("Direct Hit Damage")]
    public int baseDamage = 10;
    public DamageType damageType = DamageType.Physical;
    public bool destroyOnHit = true;

    [Header("Cast Type")]
    public SpellCastType castType = SpellCastType.None;

    [Header("Status Effects on Direct Hit")]
    public List<EffectSpec> effects = new();

    [Header("Explosion Settings")]
    public bool spawnExplosion = false;
    public ExplosionSettings explosion;

    [Header("Piercing")]
    public bool enablePierce = false;
    public int pierceCount = 0;

    [Header("Spawn Object On Impact (Persistent Zones)")]
    public bool spawnObjectOnHit = false;
    public GameObject onHitPrefab;
    public LayerMask groundMask;

    [Header("Chain Lightning")]
    public bool chainOnHit = false;
    public int chainMaxJumps = 3;
    public float chainRadius = 6f;
    public float chainDamagePerJump = 8f;
    public LayerMask chainMask = ~0;

    [Header("Light Ray Beam")]
    public bool isBeamSpell = false;
    public LightRayBeam beamPrefab;
    public LightRayConfig beamConfig;

    [Header("Prefab Override")]
    public GameObject projectileOverridePrefab;

    [Header("Ground Target Spell")]
    public bool isGroundSpell = false;
    public GameObject ghostIndicatorPrefab;
    public GameObject groundSpellPrefab;
    public float maxCastDistance = 20f;

    [Header("Wind Pull")]
    public bool isWindPullSpell = false;
    public GameObject windPullPrefab;

    [Header("Earth Spike Path")]
    public bool isEarthPathSpell = false;
    public GameObject earthPathPrefab;
    public float earthPathMaxLength = 12f;
    public float earthPathWidth = 3f;
}

[System.Serializable]
public struct ExplosionSettings
{
    [Tooltip("Prefab containing FireballExplosion script and visuals.")]
    public FireballExplosion explosionPrefab;

    [Tooltip("Base damage dealt by the explosion.")]
    public float damage;

    [Tooltip("Extra status effects applied by explosion.")]
    public List<EffectSpec> effects;

    [Tooltip("Starting trigger radius.")]
    public float startRadius;

    [Tooltip("Maximum trigger radius reached at end of lifetime.")]
    public float maxRadius;

    [Tooltip("How fast the radius grows (units per second).")]
    public float expandSpeed;

    [Tooltip("Lifetime before the explosion despawns.")]
    public float lifetime;

    [Tooltip("Starting visual scale of the explosion object.")]
    public float startVisualScale;

    [Tooltip("Maximum visual scale of the explosion object.")]
    public float maxVisualScale;
}
