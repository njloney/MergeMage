using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FireballExplosion : MonoBehaviour
{
    private float damage;
    private DamageType damageType;
    private List<EffectSpec> effects;
    private Object owner;

    private float startRadius;
    private float maxRadius;
    private float expandSpeed;
    private float lifetime;

    private float startVisualScale;
    private float maxVisualScale;

    private SphereCollider col;
    private float timer;

    private readonly HashSet<Health> hitTargets = new();

    public void Init(
        float damage,
        DamageType damageType,
        List<EffectSpec> effects,
        Object owner,
        float startRadius,
        float maxRadius,
        float expandSpeed,
        float lifetime,
        float startVisualScale,
        float maxVisualScale
    )
    {
        this.damage = damage;
        this.damageType = damageType;
        this.effects = effects;
        this.owner = owner;

        this.startRadius = startRadius;
        this.maxRadius = maxRadius;
        this.expandSpeed = expandSpeed;
        this.lifetime = lifetime;

        this.startVisualScale = startVisualScale;
        this.maxVisualScale = maxVisualScale;
    }

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, lifetime));
        float radius = Mathf.Lerp(startRadius, maxRadius, t);

        col.radius = radius;

        float visScale = Mathf.Lerp(startVisualScale, maxVisualScale, t);
        transform.localScale = Vector3.one * visScale;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var hp = other.GetComponentInParent<Health>();
        if (hp == null || hitTargets.Contains(hp))
            return;

        hitTargets.Add(hp);

        hp.TakeDamage(damage, damageType, owner);

        var status = other.GetComponentInParent<StatusController>();
        if (status != null && effects != null)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                var e = effects[i];
                if (e.effect != null && e.duration > 0f)
                    status.ApplyEffect(e.effect, e.duration, e.magnitude, owner);
            }
        }
    }
}
