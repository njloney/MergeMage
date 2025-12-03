using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileConfig))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer visualRenderer;

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
        col.isTrigger = true;                       // projectile uses triggers
        rb.useGravity = false;                     // no falling
        rb.isKinematic = false;                    // dynamic body
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

        //------------------------------------------------------------------
        // 2. EXPLOSION (if you use it)
        //------------------------------------------------------------------
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
         
        
        //------------------------------------------------------------------
        // 3. PIERCE LOGIC
        //------------------------------------------------------------------
        if (cfg.stats.enablePierce)
        {
            hitsSoFar++;
            if (hitsSoFar >= cfg.stats.pierceCount)
                Despawn();

            // NOTE: we still damage this target, then continue flying
            return;
        }

        //------------------------------------------------------------------
        // 4. EXTRA BEHAVIOR (EARTH SPIKES / CHAIN LIGHTNING / ETC.)
        //------------------------------------------------------------------
        TrySpawnOnHitObject();
        TryChainLightning(other);

        if (cfg.stats.destroyOnHit)
            Despawn();
    }

    private void TrySpawnOnHitObject()
    {
        if (!cfg.stats.spawnObjectOnHit || !cfg.stats.onHitPrefab)
            return;

        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = Quaternion.identity;

        int groundMask = 1 << LayerMask.NameToLayer("Ground");
        Vector3 startPos = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 20f, groundMask))
        {
            spawnPos = hit.point;
            spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        spawnPos += Vector3.up * 0.05f;
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
        gameObject.SetActive(false);
    }
}
