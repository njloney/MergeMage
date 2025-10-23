using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FireballExplosion : MonoBehaviour
{
    // runtime gameplay params
    private float _damage;
    private DamageType _damageType;
    private List<EffectSpec> _effects; 
    private Object _owner;

    // expansion + visuals
    private float _startRadius, _maxRadius, _expandSpeed, _lifetime;
    private float _startScale, _maxScale;

    // internals
    private SphereCollider _col;
    private float _timer;
    private readonly HashSet<Collider> _hit = new();

    /// Called by the Projectile right after Instantiate.
    public void Init(
        float damage, DamageType type, List<EffectSpec> effects, Object owner,
        float startRadius, float maxRadius, float expandSpeed, float lifetime,
        float startScale, float maxScale)
    {
        _damage = damage;
        _damageType = type;
        _effects = effects;   // can be null;
        _owner = owner;

        _startRadius = startRadius;
        _maxRadius   = maxRadius;
        _expandSpeed = expandSpeed;
        _lifetime    = lifetime;

        _startScale  = startScale;
        _maxScale    = maxScale;
    }

    private void Awake()
    {
        _col = GetComponent<SphereCollider>();
        _col.isTrigger = true;
        // Don't touch radius/scale here
    }

    private void Start()
    {
        // Now Init() has already set all fields — safe to apply them:
        _col.radius = Mathf.Max(0f, _startRadius);

        // set initial visual scale 
        float safeStartScale = Mathf.Max(0.0001f, _startScale);
        transform.localScale = Vector3.one * safeStartScale;
    }

    private void Update()
    {
        // Expand collider
        if (_col.radius < _maxRadius)
            _col.radius = Mathf.Min(_maxRadius, _col.radius + _expandSpeed * Time.deltaTime);

        // Expand visuals in step with radius
        float t = (_maxRadius > _startRadius)
            ? Mathf.InverseLerp(_startRadius, _maxRadius, _col.radius)
            : 1f;

        float targetScale = Mathf.Lerp(Mathf.Max(0.0001f, _startScale), _maxScale, t);
        transform.localScale = Vector3.one * targetScale;

        // Lifetime
        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hit.Contains(other)) return;
        _hit.Add(other);

        if (other.TryGetComponent<Health>(out var hp))
        {
            if (_damage > 0f)
                hp.TakeDamage(_damage, _damageType, _owner);

            if (_effects != null && other.TryGetComponent<StatusController>(out var status))
            {
                // apply each configured effect safely
                for (int i = 0; i < _effects.Count; i++)
                {
                    var e = _effects[i];
                    if (e.effect && e.duration > 0f)
                        status.ApplyEffect(e.effect, e.duration, e.magnitude, _owner);
                }
            }
        }
    }
}
