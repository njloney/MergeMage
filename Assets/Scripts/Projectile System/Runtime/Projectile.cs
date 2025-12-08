using System.Collections.Generic;
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
    public bool useGravity = false;

    private void Awake()
    {
        cfg = GetComponent<ProjectileConfig>();
        if (!rb) rb = GetComponent<Rigidbody>();

        var col = GetComponent<Collider>();
        col.isTrigger = true;
        rb.useGravity = useGravity;
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

        // var impactAudio = GetComponent<SoundImpactAudio>();
        // if (impactAudio != null)
        // {
        //     impactAudio.PlayImpactSound();
        // }

        if (other.TryGetComponent<Projectile>(out _))
            return;

        if (other.isTrigger && other.GetComponentInParent<Health>() == null)
            return;

        if (IsOwner(other))
            return;

        if (hitThisFrame.Contains(other))
            return;
        hitThisFrame.Add(other);

        // --- IMPACT LOGIC ---

        // Check for specific Golem Boss reaction
        var responder = other.GetComponentInParent<GolemSpellResponder>();
        if (responder != null)
            responder.OnHitBySpell(cfg.stats, cfg.owner);

        // Apply Damage / Status
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
        else
        {
        }

        if (cfg.stats.impactVFX != null)
        {
            Instantiate(cfg.stats.impactVFX, transform.position, Quaternion.identity);
        }

        // EXPLOSION
        if (cfg.stats.spawnExplosion)
        {
            SpawnExplosion();
        }

        // PIERCE LOGIC
        if (cfg.stats.enablePierce)
        {
            hitsSoFar++;
            if (hitsSoFar >= cfg.stats.pierceCount)
                Despawn();
            return;
        }

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        TrySpawnOnHitObject(hitPoint);
        TryChainLightning(other);

        // Finally, destroy the projectile
        if (cfg.stats.destroyOnHit)
            Despawn();
    }

    private bool IsOwner(Collider other)
    {
        if (cfg.owner == null) return false;

        Transform ownerTransform = null;
        if (cfg.owner is Component c) ownerTransform = c.transform;
        else if (cfg.owner is GameObject g) ownerTransform = g.transform;

        if (ownerTransform == null) return false;

        // Did we hit the exact owner object?
        if (other.transform == ownerTransform) return true;

        // Did we hit a child of the owner? (e.g. hit the player's arm collider)
        if (other.transform.IsChildOf(ownerTransform)) return true;

        // Did we hit the parent of the owner? (e.g. Owner is the Wand, Other is the Player)
        if (ownerTransform.IsChildOf(other.transform)) return true;

        return false;
    }

    void SpawnExplosion()
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
        if (trail != null && trail.enabled)
        {
            StartCoroutine(SoftDespawn());
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f); // Ensure it cleans up
        }
    }

    private System.Collections.IEnumerator SoftDespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;

        if (visualRenderer != null) visualRenderer.enabled = false;

        yield return new WaitForSeconds(trail.time);

        // Safety check if object was destroyed mid-wait
        if (this == null) yield break;

        if (col != null) col.enabled = true;
        if (visualRenderer != null) visualRenderer.enabled = true;
        trail.Clear();

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}