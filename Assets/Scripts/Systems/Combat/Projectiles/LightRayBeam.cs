using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightRayBeam : MonoBehaviour
{
    private LightRayConfig _cfg;
    private Transform _origin;
    private Object _owner;
    private LineRenderer _lr;

    private float _timeAlive;
    private float _tick;

    private Vector3 _fixedStart;
    private Vector3 _fixedDir;

    // [NEW] Track the instantiated visual
    private GameObject _activeHitEffect;

    public void Init(Transform origin, Object owner, LightRayConfig config)
    {
        _origin = origin;
        _owner = owner;
        _cfg = config;

        _lr = GetComponent<LineRenderer>();
        if (_lr == null) _lr = gameObject.AddComponent<LineRenderer>();

        _timeAlive = 0f;
        _tick = 0f;

        if (!_cfg.followOrigin && _origin != null)
        {
            _fixedStart = _origin.position;
            _fixedDir = _origin.forward;
        }

        // [NEW] Instantiate the hit effect if one exists
        if (_cfg.hitEffectPrefab != null)
        {
            _activeHitEffect = Instantiate(_cfg.hitEffectPrefab);
            _activeHitEffect.SetActive(false); // Hide until we hit something
        }

        UpdateBeamVisuals(0f);
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (_activeHitEffect != null)
        {
            Destroy(_activeHitEffect);
        }
    }

    private void Update()
    {
        if (_cfg == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _timeAlive += Time.deltaTime;
        if (_timeAlive >= _cfg.duration)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateBeamVisuals(Time.deltaTime);

        _tick += Time.deltaTime;
        while (_tick >= _cfg.tickInterval)
        {
            _tick -= _cfg.tickInterval;
            ApplyDamage(_cfg.tickInterval);
        }
    }

    private void UpdateBeamVisuals(float dt)
    {
        if (_lr == null) return;

        Vector3 start, dir;
        if (_cfg.followOrigin && _origin != null)
        {
            start = _origin.position;
            dir = _origin.forward;
        }
        else
        {
            start = _fixedStart;
            dir = _fixedDir;
        }

        Vector3 end = start + dir * _cfg.maxRange;
        bool hitSomething = false;

        // Raycast
        if (Physics.Raycast(start, dir, out var hit, _cfg.maxRange, _cfg.hitMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            hitSomething = true;

            // [NEW] Move the hit effect to the wall
            if (_activeHitEffect != null)
            {
                if (!_activeHitEffect.activeSelf) _activeHitEffect.SetActive(true);
                _activeHitEffect.transform.position = hit.point;
                _activeHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            }

            if (!_cfg.stopOnHit)
            {
                // If penetrating, visual beam might go further, but hit effect stays on first hit
                end = start + dir * _cfg.maxRange;
            }
        }
        else
        {
            if (_activeHitEffect != null && _activeHitEffect.activeSelf)
            {
                _activeHitEffect.SetActive(false);
            }
        }

        _lr.positionCount = 2;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
    }

    private void ApplyDamage(float tick)
    {
        Vector3 start, dir;
        if (_cfg.followOrigin && _origin != null)
        {
            start = _origin.position;
            dir = _origin.forward;
        }
        else
        {
            start = _fixedStart;
            dir = _fixedDir;
        }

        if (Physics.Raycast(start, dir, out var hit, _cfg.maxRange, _cfg.hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(_cfg.dps * tick, _cfg.damageType, _owner);
            }
        }
    }

    public static LightRayBeam Spawn(Transform origin, Object owner, LightRayConfig cfg, Vector3? overrideDir = null)
    {
        // (Your existing Spawn method remains unchanged)
        var prefab = Resources.Load<LightRayBeam>("LightRayBeam");
        if (prefab == null) return null;
        var beam = Instantiate(prefab);
        if (overrideDir.HasValue && cfg != null && !cfg.followOrigin && origin != null)
        {
            beam._fixedStart = origin.position;
            beam._fixedDir = overrideDir.Value.normalized;
        }
        beam.Init(origin, owner, cfg);
        return beam;
    }
}