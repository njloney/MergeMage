using System.Collections.Generic;
using UnityEngine;

// Stationary AoE that applies damage + slow each tick.
[RequireComponent(typeof(SphereCollider))]
public class GroundSpikeField : MonoBehaviour
{
    [Header("AoE")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float lifetime = 4f;

    [Header("Ticking")]
    [SerializeField] private float damagePerTick = 4f;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float tickInterval = 1f; // match your burn if you prefer

    [Header("Effects")]
    [SerializeField] private StatusEffect slowEffect; // assign SlowEffect asset
    [SerializeField] private float slowDuration = 1.2f;
    [SerializeField] private float slowMagnitude = 0f; // unused; kept for consistency

    [SerializeField] private LayerMask hitMask = ~0;

    private SphereCollider col;
    private float time, tick;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void Update()
    {
        // lifetime
        time += Time.deltaTime;
        if (time >= lifetime) { Destroy(gameObject); return; }

        // ticks
        tick += Time.deltaTime;
        if (tick >= tickInterval)
        {
            tick -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        var hits = Physics.OverlapSphere(transform.position, radius, hitMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            if (h.TryGetComponent<Health>(out var hp))
            {
                if (damagePerTick > 0f) hp.TakeDamage(damagePerTick, damageType, this);

                if (slowEffect && h.TryGetComponent<StatusController>(out var status))
                {
                    status.ApplyEffect(slowEffect, slowDuration, slowMagnitude, this);
                }
            }
        }
    }
}
