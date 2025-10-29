// FireballExplosion.cs (patched parts)
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FireballExplosion : MonoBehaviour
{
    // --- designer toggles ---
    [Header("Tick Options")]
    [SerializeField] private bool tickOnSpawn = false;               // optional first pulse on spawn
    [SerializeField] private bool finalPulseOnDeath = true;          // ✅ fire a last tick right before destroy
    [SerializeField] private bool expandToMaxBeforeFinalPulse = true;// bump radius to max for last tick

    [Header("Hit Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;                 // what layers get hit

    // runtime params (set via Init)
    private float _damagePerTick;
    private DamageType _damageType;
    private List<EffectSpec> _effects;
    private Object _owner;

    // expansion + visuals (Init)
    private float _startRadius, _maxRadius, _expandSpeed, _lifetime;
    private float _startScale, _maxScale;

    // internals
    private SphereCollider _col;
    private float _timer;
    private float _tickElapsed;
    private float _tickInterval = 1.0f;

    public void Init(
        float damagePerTick, DamageType type, List<EffectSpec> effects, Object owner,
        float startRadius, float maxRadius, float expandSpeed, float lifetime,
        float startScale, float maxScale)
    {
        _damagePerTick = damagePerTick;
        _damageType    = type;
        _effects       = effects;   // can be null
        _owner         = owner;

        _startRadius   = startRadius;
        _maxRadius     = maxRadius;
        _expandSpeed   = expandSpeed;
        _lifetime      = lifetime;

        _startScale    = startScale;
        _maxScale      = maxScale;
    }

    private void Awake()
    {
        _col = GetComponent<SphereCollider>();
        _col.isTrigger = true;
    }

    private void Start()
    {
        // apply initial radius/scale
        _col.radius = Mathf.Max(0f, _startRadius);
        transform.localScale = Vector3.one * Mathf.Max(0.0001f, _startScale);

        // match cadence to effects (e.g., Burn)
        _tickInterval = GetIntervalFromEffects(_effects, defaultInterval: 1.0f);
        _tickElapsed  = 0f;

        // optional: tick immediately on spawn
        if (tickOnSpawn) DoTick();
    }

    private void Update()
    {
        // expand collider
        if (_col.radius < _maxRadius)
            _col.radius = Mathf.Min(_maxRadius, _col.radius + _expandSpeed * Time.deltaTime);

        // expand visuals in sync
        float t = (_maxRadius > _startRadius)
            ? Mathf.InverseLerp(_startRadius, _maxRadius, _col.radius)
            : 1f;
        transform.localScale = Vector3.one * Mathf.Lerp(Mathf.Max(0.0001f, _startScale), _maxScale, t);

        // cadence-based ticking
        _tickElapsed += Time.deltaTime;
        while (_tickElapsed >= _tickInterval)
        {
            _tickElapsed -= _tickInterval;
            DoTick();
        }

        // lifetime end → final guaranteed pulse (before destroy)
        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
        {
            if (finalPulseOnDeath)
            {
                // optional: ensure the last pulse uses the full visual radius
                if (expandToMaxBeforeFinalPulse)
                    _col.radius = _maxRadius;

                DoTick();
            }
            Destroy(gameObject);
        }
    }

    // Apply damage/effects to everything currently within the AoE radius
    private void DoTick()
    {
        var hits = Physics.OverlapSphere(
            transform.position,
            _col.radius,
            hitMask,
            QueryTriggerInteraction.Ignore
        );
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var other = hits[i];
            if (!other) continue;

            if (other.TryGetComponent<Health>(out var hp))
            {
                if (_damagePerTick > 0f)
                    hp.TakeDamage(_damagePerTick, _damageType, _owner);

                if (_effects != null && other.TryGetComponent<StatusController>(out var status))
                {
                    for (int e = 0; e < _effects.Count; e++)
                    {
                        var spec = _effects[e];
                        if (spec.effect && spec.duration > 0f)
                            status.ApplyEffect(spec.effect, spec.duration, spec.magnitude, _owner);
                    }
                }
            }
        }
    }

    // Try to match the cadence of your effects (e.g., Burn.TickInterval). Fallback if none found.
    private float GetIntervalFromEffects(List<EffectSpec> effects, float defaultInterval)
    {
        if (effects == null || effects.Count == 0) return defaultInterval;

        float best = float.MaxValue;
        bool found = false;

        for (int i = 0; i < effects.Count; i++)
        {
            var fx = effects[i].effect;
            if (fx == null) continue;

            if (fx is BurnEffect burn)
            {
                best  = Mathf.Min(best, Mathf.Max(0.01f, burn.tickInterval));
                found = true;
            }
        }

        return found ? best : defaultInterval;
    }
}
