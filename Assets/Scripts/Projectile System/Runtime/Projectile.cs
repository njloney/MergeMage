using UnityEngine;
using System.Collections.Generic;

// Handles projectile movement, collision, and applying damage/effects.
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;  // Used for movement
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
        if (rb)
            rb.linearVelocity = transform.forward * cfg.stats.speed; // Move forward
        _hitsSoFar = 0;
        _hitThisFrame.Clear();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // Destroy projectile after its lifetime expires
        if (elapsed >= cfg.stats.lifetime)
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

            // Example: pierceCount = 2 → projectile disappears after 2 targets hit
            if (_hitsSoFar >= cfg.stats.pierceCount)
                Despawn();

            // Exit early to allow it to continue flying through other targets
            return;
        }

        // --- 3. Default behavior for non-piercing projectiles ---
        // If the projectile should be destroyed immediately after any hit,
        // remove it from play here.
        if (cfg.stats.destroyOnHit)
            Despawn();
    }

    // Deactivates the projectile (can be pooled instead of destroyed)
    private void Despawn() => gameObject.SetActive(false);

    private void LateUpdate()
    {
        // clear the per-frame collider cache
        _hitThisFrame.Clear();
    }
}

