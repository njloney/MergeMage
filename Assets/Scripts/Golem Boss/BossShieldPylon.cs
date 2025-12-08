using UnityEngine;

public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public Transform rayOrigin;

    public float beamWidth = 0.15f;
    public float spinSpeed = 2f;
    public float maxRayDistance = 50f;
    public LayerMask obstacleMask;

    public bool IsActive { get; private set; } = true;

    LineRenderer line;
    Material beamMat;
    float spin;

    void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = beamWidth;
        line.endWidth = beamWidth;
        line.textureMode = LineTextureMode.Tile;
        line.useWorldSpace = true;

        beamMat = new Material(Shader.Find("Sprites/Default"));
        beamMat.color = new Color(0.6f, 0f, 1f);
        beamMat.mainTextureScale = new Vector2(1, 5);

        line.material = beamMat;
    }

    void Update()
    {
        if (!IsActive || bossShield == null)
        {
            line.enabled = false;
            return;
        }

        spin += spinSpeed * Time.deltaTime;
        beamMat.mainTextureOffset = new Vector2(spin, 0);

        CastBeam();
    }

    void CastBeam()
    {
        Transform originTf = rayOrigin != null ? rayOrigin : transform;

        Vector3 origin = originTf.position;
        Vector3 target = bossShield.transform.position;
        Vector3 dir = (target - origin).normalized;
        float dist = Mathf.Min(Vector3.Distance(origin, target), maxRayDistance);

        line.enabled = true;
        line.SetPosition(0, origin);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
            line.SetPosition(1, hit.point);
        else
            line.SetPosition(1, origin + dir * dist);
    }

    public void Deactivate()
    {
        IsActive = false;
        line.enabled = false;
    }
}
