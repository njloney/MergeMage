using UnityEngine;
using System.Collections.Generic;

// Handles projectile movement, collision, and applying damage/effects.
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;  // Used for movement
    [SerializeField] private MeshRenderer visualRenderer;
    private ProjectileConfig cfg;           // Holds projectile data (speed, damage, effects)
    private float elapsed;                  // Tracks how long the projectile has existed
    private readonly HashSet<Collider> _hitThisFrame = new(); // avoid double-hit on same collider this frame
    private int _hitsSoFar = 0;

    private void Awake()
    {
        cfg = GetComponent<ProjectileConfig>();  // Get projectile configuration
        if (!rb) rb = GetComponent<Rigidbody>(); // Auto-grab Rigidbody if not set
    }

    private void OnEnable()
    {
        elapsed = 0f; // Reset lifetime timer
        if (rb && cfg != null && cfg.stats != null)
        {// Added null checks
            rb.linearVelocity = transform.forward * cfg.stats.speed; // Move forward
            if (visualRenderer != null && cfg.stats.projectileMaterial != null)
            {
                visualRenderer.material = cfg.stats.projectileMaterial;
            }
        }
        _hitsSoFar = 0;
        _hitThisFrame.Clear();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // Destroy projectile after its lifetime expires
        if (cfg != null && cfg.stats != null && elapsed >= cfg.stats.lifetime) // Added null checks
            Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- Safety check: make sure config exists ---
        if (cfg == null || cfg.stats == null)
            return;

        // --- 1. Apply direct hit damage and effects ---
        // If the target has a Health component, deal base damage.
        if (other.TryGetComponent<Health>(out var hp))
        {
            // Apply raw damage from this projectile.
            hp.TakeDamage(cfg.stats.baseDamage, cfg.stats.damageType, cfg.owner);

            // If the target can receive effects (like burn or freeze), apply them.
            if (other.TryGetComponent<StatusController>(out var status))
            {
                var effects = cfg.stats.effects;
                if (effects != null)
                {
                    foreach (var e in effects)
                    {
                        // Only apply valid effects (must have a ScriptableObject + positive duration)
                        if (e.effect && e.duration > 0f)
                            status.ApplyEffect(e.effect, e.duration, e.magnitude, cfg.owner);
                    }
                }
            }
        }

        // 2. Spawn explosion
        if (cfg.stats.spawnExplosion)
        {
            var data = cfg.stats.explosion;

            if (data.explosionPrefab != null)
            {
                Debug.Log($"[Projectile] Spawning explosion prefab '{data.explosionPrefab.name}' at {transform.position}.", this);

                var exp = Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);

                exp.Init(
                    data.damage,
                    cfg.stats.damageType,
                    data.effects,
                    cfg.owner,
                    data.startRadius,
                    data.maxRadius,
                    data.expandSpeed,
                    data.lifetime,
                    data.startVisualScale,
                    data.maxVisualScale
                );
            }
            else
            {
                Debug.LogWarning($"[Projectile] spawnExplosion is TRUE but no explosionPrefab assigned in '{cfg.stats.name}'.", this);
            }
        }


        // --- 2. Handle piercing projectiles (like Wind Bullet) ---
        // If this projectile can pierce, count how many valid targets it has hit so far.
        if (cfg.stats.enablePierce)
        {
            _hitsSoFar++;  // Increment total hits

            if (_hitsSoFar >= cfg.stats.pierceCount)
                Despawn();

            return;
        }

        if (cfg.stats.destroyOnHit)
            Despawn();

        TrySpawnOnHitObject();          // Earth Rupture spikes, or any “place object” spell
        TryChainLightning(other);       // Lightning chain starting from the first target
        TrySpawnBeam(other);            // Light Ray: spawn hitscan beam when projectile hits

    }

    // Projectile.cs (add inside class)
    private void TrySpawnOnHitObject()
    {
        if (cfg.stats.spawnObjectOnHit && cfg.stats.onHitPrefab)
        {
            Vector3 spawnPos = transform.position;
            Quaternion spawnRot = Quaternion.identity;

            // Cast straight down, but ONLY against the Ground layer
            int groundMask = 1 << LayerMask.NameToLayer("Ground");

            // Start slightly above impact to avoid immediately hitting the enemy collider
            Vector3 startPos = transform.position + Vector3.up * 2f;

            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 20f, groundMask))
            {
                spawnPos = hit.point;
                spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                // Fallback if no ground found (e.g., midair hit)
                spawnPos += Vector3.up * 0.05f;
            }

            // Slight lift to avoid clipping
            spawnPos += Vector3.up * 0.05f;

            Instantiate(cfg.stats.onHitPrefab, spawnPos, spawnRot);
        }


    }

    private void TryChainLightning(Collider firstTarget)
    {
        if (!cfg.stats.chainOnHit || firstTarget == null) return;

        // Do chain: first target + up to N jumps
        var visited = new HashSet<Collider> { firstTarget };
        Collider current = firstTarget;

        for (int i = 0; i < cfg.stats.chainMaxJumps; i++)
        {
            if (!current) break;

            // Damage current link (skip if we already did base hit; harmless to double-hit once)
            if (current.TryGetComponent<Health>(out var hp))
                hp.TakeDamage(cfg.stats.chainDamagePerJump, cfg.stats.damageType, cfg.owner);

            // Find next closest unvisited in radius
            Collider next = FindNextChainTarget(current.transform.position, visited);
            if (!next) break;
            visited.Add(next);
            current = next;
        }
    }

    private Collider FindNextChainTarget(Vector3 from, HashSet<Collider> visited)
    {
        var hits = Physics.OverlapSphere(from, cfg.stats.chainRadius, cfg.stats.chainMask, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        Collider pick = null;
        foreach (var h in hits)
        {
            if (!h || visited.Contains(h)) continue;
            float d = (h.transform.position - from).sqrMagnitude;
            if (d < best) { best = d; pick = h; }
        }
        return pick;
    }

    private void TrySpawnBeam(Collider hit)
    {
        if (!cfg.stats.spawnBeamOnHit || !cfg.stats.beamPrefab) return;

        var beam = Instantiate(cfg.stats.beamPrefab, transform.position, Quaternion.identity);
        // Choose an origin: owner’s transform if possible, else projectile’s transform
        Transform origin = null;
        if (cfg.owner is Component c) origin = c.transform;
        if (!origin) origin = transform;

        beam.transform.position = origin.position;   // align with shooter (or projectile)
        beam.Activate();                             // beam handles its own duration/damage
    }

    // Deactivates the projectile (can be pooled instead of destroyed)
    private void Despawn()
    {
        gameObject.SetActive(false);
    }


    private void LateUpdate()
    {
        // clear the per-frame collider cache
        _hitThisFrame.Clear();
    }
}