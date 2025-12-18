// ProjectileStats.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Stats")]
public class ProjectileStats : ScriptableObject
{
    // ---------------- C ASTING / TARGETING ----------------

    [Header("Casting / Targeting")]
    public SpellCastType castType = SpellCastType.Projectile;

    [Tooltip("Max distance from the caster this spell can be aimed / cast.")]
    public float maxCastDistance = 30f;

    [Tooltip("Optional ghost / targeting indicator prefab for ground-targeted spells.")]
    public GameObject ghostIndicatorPrefab;

    // ---------------- PROJECTILE MOTION ----------------

    [Header("Projectile Motion")]
    public float speed = 20f;
    public float lifetime = 5f;

    // ---------------- COST ----------------

    [Header("Mana Cost")]
    public float manaCost = 25f;

    // ---------------- VISUALS ----------------

    [Header("Projectile Visuals")]
    [Tooltip("Material to apply to the projectile mesh renderer, if any.")]
    public Material projectileMaterial;

    [Tooltip("If set, this prefab is used instead of the default projectile prefab.")]
    public GameObject projectileOverridePrefab;

    [Tooltip("Small VFX spawned on impact for regular projectile hits.")]
    public GameObject impactVFX;

    // ---------------- DAMAGE / EFFECTS ----------------

    [Header("Damage")]
    public float baseDamage = 10f;
    public DamageType damageType = DamageType.Physical;

    [Header("Status Effects")]
    public EffectSpec[] effects;

    // ---------------- PIERCE / HIT BEHAVIOR ----------------

    [Header("Piercing")]
    public bool enablePierce = false;
    public int pierceCount = 0;

    [Header("Hit Behaviour")]
    [Tooltip("If true, projectile despawns on the first non-pierced hit.")]
    public bool destroyOnHit = true;

    // ---------------- ON-HIT OBJECT SPAWNING ----------------

    [Header("On-Hit Object Spawning")]
    [Tooltip("If true, spawn an object (eg. lava, ice patch) when this projectile hits.")]
    public bool spawnObjectOnHit = false;

    public GameObject onHitPrefab;
    public LayerMask groundMask = ~0;

    // ---------------- GROUND / PATH SPELLS (LEGACY) ----------------

    [Header("Ground Spell (legacy)")]
    [Tooltip("If true, this stats asset is used by a ground-only spell (eg. geyser).")]
    public bool isGroundSpell = false;
    public GameObject groundSpellPrefab;

    [Header("Earth Path Spell (legacy)")]
    [Tooltip("If true, this uses the older earth-path logic instead of EarthSpikeField.")]
    public bool isEarthPathSpell = false;
    public GameObject earthPathPrefab;
    public float earthPathMaxLength = 12f;
    public float earthPathWidth = 3f;

    // ---------------- EXPLOSION ----------------

    [System.Serializable]
    public class ExplosionData
    {
        [Tooltip("Explosion prefab that has a FireballExplosion component.")]
        public FireballExplosion explosionPrefab;

        [Tooltip("Damage dealt by this explosion.")]
        public float damage = 10f;

        [Tooltip("Extra effects applied by the explosion.")]
        public List<EffectSpec> effects = new List<EffectSpec>();

        [Tooltip("Initial radius of the explosion's hit area.")]
        public float startRadius = 1f;

        [Tooltip("Maximum radius the explosion will reach.")]
        public float maxRadius = 4f;

        [Tooltip("How fast the radius grows (units per second).")]
        public float expandSpeed = 10f;

        [Tooltip("Lifetime before the explosion despawns.")]
        public float lifetime = 0.5f;

        [Tooltip("Starting visual scale of the explosion object.")]
        public float startVisualScale = 1f;

        [Tooltip("Maximum visual scale of the explosion object.")]
        public float maxVisualScale = 3f;
    }

    [Header("Explosion")]
    public bool spawnExplosion = false;
    public ExplosionData explosion;

    // ---------------- CHAIN LIGHTNING ----------------

    [Header("Chain Lightning")]
    public bool chainOnHit = false;
    public int chainMaxJumps = 3;
    public float chainRadius = 6f;
    public float chainDamagePerJump = 8f;
    public LayerMask chainMask = ~0;

    // ---------------- BEAM / LIGHT RAY ----------------

    [Header("Light Ray Beam")]
    [Tooltip("If true, this spell will cast a LightRayBeam instead of spawning a projectile.")]
    public bool isBeamSpell = false;

    [Tooltip("Prefab that has a LightRayBeam component.")]
    public LightRayBeam beamPrefab;

    [Tooltip("Config asset that defines DPS, range, duration, etc.")]
    public LightRayConfig beamConfig;

    // ---------------- BOSS-ONLY SPELLS ----------------

    [Header("Wind Pull (Boss Only)")]
    public bool isWindPullSpell = false;
    public GameObject windPullPrefab;

    [Header("Earth Spike Field (Boss Only)")]
    public bool isEarthSpikeSpell = false;
    public GameObject earthSpikePrefab;

    [Header("Lightning Strike (Boss Only)")]
    public bool isLightningStrikeSpell = false;
    public GameObject lightningStrikePrefab;

    // ---------------- METADATA ----------------

    [Header("Metadata")]
    public string spellName;
}
