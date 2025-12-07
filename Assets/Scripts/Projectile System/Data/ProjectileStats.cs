// ProjectileStats.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Stats")]
public class ProjectileStats : ScriptableObject
{
    [Header("Projectile Motion")]
    public float speed = 20f;
    public float lifetime = 5f;

    [Header("Mana Effects")]
    public float manaCost = 25f;
    [Header("Projectile Visuals")]
    public Material projectileMaterial;

    [Header("Direct Hit Damage")]
    public int baseDamage = 10;
    public DamageType damageType = DamageType.Physical;
    public bool destroyOnHit = true;

    [Header("Cast Type")]
    public SpellCastType castType = SpellCastType.None;

    [Header("Status Effects on Direct Hit")]
    public List<EffectSpec> effects = new();

    [Header("Explosion Settings")]
    public bool spawnExplosion = false;           // enable/disable explosion
    public ExplosionSettings explosion;           // holds explosion data

    [Header("Piercing (for things like Wind Bullet)")]
    public bool enablePierce = false;     // if true, the projectile can pass through targets
    public int pierceCount = 0;           // how many distinct targets it can hit before despawning


    [Header("Spawn Object On Impact (Earth / Fire zones)")]
    public bool spawnObjectOnHit = false;
    public GameObject onHitPrefab;
    public LayerMask groundMask;


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

    [Header("Prefab Override")]
    public GameObject projectileOverridePrefab;

    [Header("Ground Target Spell (e.g. Lightning Strike)")]
    public bool isGroundSpell = false;
    [Tooltip("The ghost/reticle to show on the ground where the player is aiming.")]
    public GameObject ghostIndicatorPrefab;
    [Tooltip("The actual spell prefab to spawn at the target point (must have LightningStrike script or similar).")]
    public GameObject groundSpellPrefab;
    [Tooltip("Max distance to cast this spell.")]
    public float maxCastDistance = 20f;

    [Header("Screen Shake")]
    public float impactShakeMagnitude = 0.5f;
    public float impactShakeDuration = 0.2f;

    [Header("Wind Pull (Boss AoE)")]
    public bool isWindPullSpell = false;
    public GameObject windPullPrefab;

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
