using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GroundRectArea : MonoBehaviour
{
    [Header("Dimensions")]
    // We rely on the BoxCollider size, but we can track lifetime here
    [SerializeField] private float lifetime = 5f;

    [Header("Damage & Ticking")]
    [SerializeField] private float damagePerTick = 10f;
    [SerializeField] private DamageType damageType = DamageType.Fire;
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Status Effect")]
    [SerializeField] private StatusEffect debuffEffect; // Drag your BurnEffect here
    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float debuffMagnitude = 5f; // e.g. burn damage per tick

    [SerializeField] private LayerMask hitMask = ~0;

    private BoxCollider boxCol;
    private float timeAlive;
    private float tickTimer;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;
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
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        // Use OverlapBox instead of Sphere. 
        // bounds.extents gives us the half-size of the box in world space (handling scale).
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            boxCol.bounds.extents,
            transform.rotation,
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

    // Draw gizmo to see the hit area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        // Draw the box based on local collider size if it exists, otherwise generic
        Vector3 size = GetComponent<BoxCollider>() ? GetComponent<BoxCollider>().size : Vector3.one;
        Gizmos.DrawCube(Vector3.zero, size);
    }
}