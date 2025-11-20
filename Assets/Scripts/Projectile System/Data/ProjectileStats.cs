// ProjectileStats.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Stats")]
public class ProjectileStats : ScriptableObject
{
    [Header("Projectile Motion")]
    public float speed = 20f;
    public float lifetime = 5f;

    [Header("Projectile Visuals")]
    public Material projectileMaterial;

    [Header("Direct Hit Damage")]
    public int baseDamage = 10;
    public DamageType damageType = DamageType.Physical;
    public bool destroyOnHit = true;

    [Header("Status Effects on Direct Hit")]
    public List<EffectSpec> effects = new();

    [Header("Explosion Settings")]
    public bool spawnExplosion = false;           // enable/disable explosion
    public ExplosionSettings explosion;           // holds explosion data

    // In your ProjectileStats ScriptableObject
    [Header("Piercing (for things like Wind Bullet)")]
    public bool enablePierce = false;     // if true, the projectile can pass through targets
    public int pierceCount = 0;           // how many distinct targets it can hit before despawning

    [Header("Spawn Object On Impact (Earth Rupture / Void field, etc.)")]
    public bool spawnObjectOnHit = false;
    public GameObject onHitPrefab;   // GroundSpikeField prefab

    [Header("Chain Lightning (Lightning Spell)")]
    public bool chainOnHit = false;
    public int chainMaxJumps = 3;
    public float chainRadius = 6f;
    public float chainDamagePerJump = 8f;
    public LayerMask chainMask = ~0;

    [Header("Light Ray Beam (non-projectile spell)")]
    [Tooltip("If true, this spell will cast a LightRayBeam instead of spawning a projectile.")]
    public bool isBeamSpell = false;

    [Tooltip("Prefab that has a LightRayBeam component.")]
    public LightRayBeam beamPrefab;

    [Tooltip("Config asset that defines DPS, range, duration, etc.")]
    public LightRayConfig beamConfig;

    [Header("Ground Target Spell (Earth Rupture, Void Field, Lightning Strike etc.)")]
    [Tooltip("If true, this spell will target the ground instead of firing a projectile.")]
    public bool isGroundSpell = false;
    [Tooltip("The prefab to show on the ground where the spell will hit.")]
    public GameObject groundTargetPrefab;  // prefab for the ground target effect
    [Tooltip("The actual spell prefab to spawn on the ground at the target point.")]
    public GameObject groundSpellPrefab;   // prefab for the spell effect that spawns on the ground
    [Tooltip("The max distance to cast this spell.")]
    public float maxCastDistance = 20f;
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
