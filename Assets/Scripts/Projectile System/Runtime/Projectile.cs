using UnityEngine;

// Handles projectile movement, collision, and applying damage/effects.
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;  // Used for movement
    private ProjectileConfig cfg;           // Holds projectile data (speed, damage, effects)
    private float elapsed;                  // Tracks how long the projectile has existed

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
        // If the target has a Health component, apply base damage
        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.TakeDamage(cfg.stats.baseDamage, cfg.stats.damageType, cfg.owner);

            // If the target can have effects, apply each one
            if (other.TryGetComponent<StatusController>(out var status))
            {
                foreach (var e in cfg.stats.effects)
                {
                    if (e.effect && e.duration > 0f)
                        status.ApplyEffect(e.effect, e.duration, e.magnitude, cfg.owner);
                }
            }
        }

        // Spawn explosion if enabled in stats
        if (cfg.stats.spawnExplosion && cfg.stats.explosion.explosionPrefab)
        {
            var data = cfg.stats.explosion;
            var exp = Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);

            // pass all designer values
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

        // Remove projectile after collision if set to do so
        if (cfg.stats.destroyOnHit)
            Despawn();
    }

    // Deactivates the projectile (can be pooled instead of destroyed)
    private void Despawn() => gameObject.SetActive(false);
}
