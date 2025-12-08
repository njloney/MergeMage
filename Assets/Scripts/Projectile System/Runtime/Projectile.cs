using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileConfig))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer visualRenderer;
    [SerializeField] private TrailRenderer trail;

    private ProjectileConfig cfg;
    private float elapsed;
    private int hitsSoFar;
    private readonly HashSet<Collider> hitThisFrame = new();

    private void Awake()
    {
        cfg = GetComponent<ProjectileConfig>();
        if (!rb) rb = GetComponent<Rigidbody>();

        // Make sure collider + rigidbody are configured correctly
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnEnable()
    {
        elapsed = 0f;
        hitsSoFar = 0;
        hitThisFrame.Clear();

        if (cfg != null && cfg.stats != null)
        {
            rb.linearVelocity = transform.forward * cfg.stats.speed;

            if (visualRenderer != null && cfg.stats.projectileMaterial != null)
                visualRenderer.material = cfg.stats.projectileMaterial;
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (cfg != null && cfg.stats != null && elapsed >= cfg.stats.lifetime)
            Despawn();
    }

    private void LateUpdate()
    {
        hitThisFrame.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cfg == null || cfg.stats == null)
            return;

        // Avoid double-processing the same collider in a single frame
        if (hitThisFrame.Contains(other))
            return;
        hitThisFrame.Add(other);

        Debug.Log($"[Projectile] Trigger hit: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})", this);

        var responder = other.GetComponentInParent<GolemSpellResponder>();
        if (responder != null)
            responder.OnHitBySpell(cfg.stats, cfg.owner);

        var hp = other.GetComponentInParent<Health>();
        var status = other.GetComponentInParent<StatusController>();

        if (hp != null)
        {
            hp.TakeDamage(cfg.stats.baseDamage, cfg.stats.damageType, cfg.owner);

            if (status != null && cfg.stats.effects != null)
            {
                foreach (var e in cfg.stats.effects)
                {
                    if (e.effect != null && e.duration > 0f)
                        status.ApplyEffect(e.effect, e.duration, e.magnitude, cfg.owner);
                }
            }
        }


        // EXPLOSION
        if (cfg.stats.spawnExplosion)
        {
            var data = cfg.stats.explosion;

            if (data.explosionPrefab != null)
            {
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
        }

        // PIERCE LOGIC
        if (cfg.stats.enablePierce)
        {
            hitsSoFar++;
            if (hitsSoFar >= cfg.stats.pierceCount)
                Despawn();

            return;
        }

        // EXTRA BEHAVIOR (EARTH SPIKES / CHAIN LIGHTNING / ETC.)
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        TrySpawnOnHitObject(hitPoint);
        TryChainLightning(other);

        if (cfg.stats.destroyOnHit)
            Despawn();
    }

    void TrySpawnOnHitObject(Vector3 hitPoint)
    {
        if (!cfg.stats.spawnObjectOnHit || cfg.stats.onHitPrefab == null)
            return;

        Vector3 spawnPos = hitPoint;
        Quaternion spawnRot = Quaternion.identity;

        if (Physics.Raycast(
            hitPoint + Vector3.up * 2f,
            Vector3.down,
            out RaycastHit groundHit,
            10f,
            cfg.stats.groundMask,
            QueryTriggerInteraction.Ignore))
        {
            spawnPos = groundHit.point;
            spawnRot = Quaternion.FromToRotation(Vector3.up, groundHit.normal);
        }

        Instantiate(cfg.stats.onHitPrefab, spawnPos, spawnRot);
    }

    private void TryChainLightning(Collider firstTarget)
    {
        if (!cfg.stats.chainOnHit || firstTarget == null)
            return;

        var visited = new HashSet<Collider> { firstTarget };
        Collider current = firstTarget;

        for (int i = 0; i < cfg.stats.chainMaxJumps; i++)
        {
            if (!current) break;

            var hp = current.GetComponentInParent<Health>();
            if (hp != null)
                hp.TakeDamage(cfg.stats.chainDamagePerJump, cfg.stats.damageType, cfg.owner);

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

    private void Despawn()
    {
        // Check if we have a trail that needs to finish fading
        if (trail != null && trail.enabled)
        {
            StartCoroutine(SoftDespawn());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator SoftDespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;

        if (visualRenderer != null) visualRenderer.enabled = false;

        yield return new WaitForSeconds(trail.time);
        if (col != null) col.enabled = true;
        if (visualRenderer != null) visualRenderer.enabled = true;
        trail.Clear();

        gameObject.SetActive(false);
    }
}
