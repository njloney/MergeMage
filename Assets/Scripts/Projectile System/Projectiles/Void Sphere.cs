using System.Collections.Generic;
using UnityEngine;

// Lingering AoE that deals damage per tick and applies Blind each tick.
[RequireComponent(typeof(SphereCollider))]
public class VoidSphereAOE : MonoBehaviour
{
    [Header("AoE")]
    [SerializeField] private float radius = 4f;
    [SerializeField] private float lifetime = 3f;

    [Header("Tick")]
    [SerializeField] private float damagePerTick = 5f;
    [SerializeField] private DamageType damageType = DamageType.Dark;
    [SerializeField] private float tickInterval = 1f; // can match Burn's if you want

    [Header("Effect")]
    [SerializeField] private StatusEffect blindEffect; // assign BlindEffect
    [SerializeField] private float blindDuration = 0.8f;

    [SerializeField] private LayerMask hitMask = ~0;

    private SphereCollider col;
    private float t, tick;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (t >= lifetime) { Destroy(gameObject); return; }

        tick += Time.deltaTime;
        if (tick >= tickInterval)
        {
            tick -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        var hits = Physics.OverlapSphere(transform.position, col.radius, hitMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (!h) continue;
            if (h.TryGetComponent<Health>(out var hp))
            {
                if (damagePerTick > 0f) hp.TakeDamage(damagePerTick, damageType, this);

                if (blindEffect && h.TryGetComponent<StatusController>(out var status))
                    status.ApplyEffect(blindEffect, blindDuration, 0f, this);
            }
        }
    }
}
