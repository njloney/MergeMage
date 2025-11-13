using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightRayBeam : MonoBehaviour
{
    private LightRayConfig _cfg;
    private Transform _origin;              // where the beam starts (muzzle/caster/camera)
    private Object _owner;                  // for damage source
    private LineRenderer _lr;

    private float _timeAlive;
    private float _tick;

    private Vector3 _fixedStart;
    private Vector3 _fixedDir;

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

        // ensure beam is visible immediately
        UpdateBeamVisuals(0f);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_cfg == null)
        {
            // No config assigned = disable to avoid errors
            gameObject.SetActive(false);
            return;
        }

        _timeAlive += Time.deltaTime;
        if (_timeAlive >= _cfg.duration)
        {
            gameObject.SetActive(false); // poolable
            return;
        }

        // Visuals update every frame for smoothness
        UpdateBeamVisuals(Time.deltaTime);

        // Damage on ticks
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

        if (Physics.Raycast(start, dir, out var hit, _cfg.maxRange, _cfg.hitMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            if (!_cfg.stopOnHit)
            {
                // draw through the hit but continue full length (purely visual)
                end = start + dir * _cfg.maxRange;
            }
        }

        _lr.positionCount = 2;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
    }

    private void ApplyDamage(float tick)
    {
        // Use the same aim as visuals this frame
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

        // We apply damage at the first hit point only (typical hitscan).
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
        var prefab = Resources.Load<LightRayBeam>("LightRayBeam"); // OPTIONAL pattern if you use Resources
        if (prefab == null)
        {
            Debug.LogError("LightRayBeam prefab not found in Resources. Prefer manual Instantiate.");
            return null;
        }
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
