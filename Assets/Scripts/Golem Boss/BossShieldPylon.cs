using UnityEngine;

public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public Transform rayOrigin;
    public float radius = 4f;
    public float channelTime = 3f;
    public LayerMask playerMask;

    public LayerMask obstacleMask;
    public float maxRayDistance = 50f;

    public float beamWidth = 0.15f;

    public bool IsActive { get; private set; } = true;

    float progress;
    LineRenderer line;

    void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = beamWidth;
        line.endWidth = beamWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.6f, 0f, 1f);
        line.endColor = new Color(0.6f, 0f, 1f);
        line.useWorldSpace = true;
    }

    void Start()
    {
        if (bossShield != null)
            bossShield.RegisterPylon(this);
    }

    void Update()
    {
        if (!IsActive)
        {
            line.enabled = false;
            return;
        }

        CastRayToBoss();

        bool inRange = Physics.CheckSphere(transform.position, radius, playerMask);

        if (inRange)
        {
            progress += Time.deltaTime;
            if (progress >= channelTime)
                Deactivate();
        }
        else if (progress > 0f)
        {
            progress -= Time.deltaTime;
            if (progress < 0f)
                progress = 0f;
        }
    }

    void CastRayToBoss()
    {
        if (bossShield == null)
            return;

        Transform originTf = rayOrigin != null ? rayOrigin : transform;

        Vector3 origin = originTf.position;
        Vector3 target = bossShield.transform.position;
        Vector3 direction = (target - origin).normalized;
        float distance = Mathf.Min(Vector3.Distance(origin, target), maxRayDistance);

        Ray ray = new Ray(origin, direction);

        line.enabled = true;
        line.SetPosition(0, origin);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, obstacleMask))
            line.SetPosition(1, hit.point);
        else
            line.SetPosition(1, origin + direction * distance);
    }

    void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        line.enabled = false;

        if (bossShield != null)
            bossShield.UnregisterPylon(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
