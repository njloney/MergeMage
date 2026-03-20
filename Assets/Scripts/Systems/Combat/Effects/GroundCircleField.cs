using UnityEngine;

// Stationary Circular AoE that applies damage + status effect each tick.
[RequireComponent(typeof(SphereCollider))]
public class GroundCircleField : MonoBehaviour
{
    [Header("AoE Settings")]
    [Tooltip("How long this zone lasts before disappearing.")]
    [SerializeField] private float lifetime = 5f;
    [Tooltip("Radius of the effect (visual and logical).")]
    [SerializeField] private float radius = 5f;

    [Header("Ticking")]
    [SerializeField] private float damagePerTick = 5f;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float tickInterval = 1.0f;

    [Header("Status Effect")]
    [Tooltip("The effect to apply (e.g., Slow for Ice, Stun for Earth).")]
    [SerializeField] private StatusEffect debuffEffect;
    [SerializeField] private float debuffDuration = 2.0f;
    [SerializeField] private float debuffMagnitude = 0f; // e.g. 0.5 for 50% slow

    [Header("Targeting")]
    [SerializeField] private LayerMask hitMask = ~0;

    private SphereCollider sphereCol;
    private float timeAlive;
    private float tickTimer;

    private void Awake()
    {
        sphereCol = GetComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = radius;
    }

    private void Update()
    {
        // Handle Lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Handle Ticking
        tickTimer += Time.deltaTime;
        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        // Get scale to ensure radius matches world space size
        float worldRadius = radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            worldRadius,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (!hit) continue;

            if (hit.TryGetComponent<Health>(out var hp))
            {
                if (damagePerTick > 0f)
                    hp.TakeDamage(damagePerTick, damageType, this);

                if (debuffEffect != null && hit.TryGetComponent<StatusController>(out var status))
                {
                    status.ApplyEffect(debuffEffect, debuffDuration, debuffMagnitude, this);
                }
            }
        }
    }

    // Update the collider radius in editor if we change the value
    private void OnValidate()
    {
        if (sphereCol == null) sphereCol = GetComponent<SphereCollider>();
        if (sphereCol != null) sphereCol.radius = radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan
        Gizmos.DrawWireSphere(transform.position, radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}