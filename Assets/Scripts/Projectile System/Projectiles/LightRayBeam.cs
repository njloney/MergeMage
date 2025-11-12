using UnityEngine;

// Hitscan beam that deals DPS while active.
[RequireComponent(typeof(LineRenderer))]
public class LightRayBeam : MonoBehaviour
{
    [SerializeField] private Transform origin;  // muzzle or camera
    [SerializeField] private float dps = 10f;
    [SerializeField] private float maxRange = 30f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float tickInterval = 0.1f; // apply in small steps

    private LineRenderer lr;
    private float time, tick;

    private void Awake() => lr = GetComponent<LineRenderer>();

    public void Activate()
    {
        time = 0f; tick = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= duration) { gameObject.SetActive(false); return; }

        // draw beam each frame
        Vector3 start = origin.position;
        Vector3 dir = origin.forward;
        Vector3 end = start + dir * maxRange;

        if (Physics.Raycast(start, dir, out var hit, maxRange, ~0, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
        }

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // apply dps on ticks
        tick += Time.deltaTime;
        while (tick >= tickInterval)
        {
            tick -= tickInterval;
            if (Physics.Raycast(start, dir, out var dmgHit, maxRange, ~0, QueryTriggerInteraction.Ignore))
            {
                if (dmgHit.collider.TryGetComponent<Health>(out var hp))
                {
                    hp.TakeDamage(dps * tickInterval, DamageType.Light, this);
                }
            }
        }
    }
}
