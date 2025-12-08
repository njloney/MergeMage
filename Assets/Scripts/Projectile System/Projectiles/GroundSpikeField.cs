using UnityEngine;

// Stationary AoE that applies damage + slow each tick.
[RequireComponent(typeof(BoxCollider))]
public class GroundSpikeField : MonoBehaviour
{
    [Header("AoE")]
    [SerializeField] private float lifetime = 4f;

    [Header("Ticking")]
    [SerializeField] private float damagePerTick = 4f;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float tickInterval = 1f;

    [Header("Effects")]
    [SerializeField] private StatusEffect slowEffect; // assign SlowEffect asset
    [SerializeField] private float slowDuration = 1.2f;
    [SerializeField] private float slowMagnitude = 0f; // unused; kept for consistency

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
        // lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // ticking
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        // Use OverlapBox so scale + rotation are respected
        Collider[] hits = Physics.OverlapBox(
            boxCol.bounds.center,
            boxCol.bounds.extents,
            boxCol.transform.rotation,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            if (h.TryGetComponent<Health>(out var hp))
            {
                if (damagePerTick > 0f)
                    hp.TakeDamage(damagePerTick, damageType, this);

                if (slowEffect && h.TryGetComponent<StatusController>(out var status))
                {
                    status.ApplyEffect(slowEffect, slowDuration, slowMagnitude, this);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<BoxCollider>(out var bc)) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.matrix = bc.transform.localToWorldMatrix;
        Gizmos.DrawCube(bc.center, bc.size);
    }
}
