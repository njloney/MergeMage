using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public Transform rayOrigin;

    [Header("Beam Shape")]
    public int pointCount = 20;
    public float maxRayDistance = 50f;
    public float width = 0.3f;

    [Header("Swirl Effect")]
    public float swirlRadius = 0.5f;
    public float swirlSpeed = 6f;
    public float spiralFrequency = 2f;

    [Header("Jaggedness")]
    public float jaggedAmount = 0.2f;
    public float jitterSpeed = 15f;

    public bool IsActive { get; private set; } = true;

    private LineRenderer line;
    private Material beamMat;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (!line) line = gameObject.AddComponent<LineRenderer>();

        line.positionCount = pointCount;
        line.useWorldSpace = true;
        line.startWidth = width;
        line.endWidth = width;
        line.textureMode = LineTextureMode.Tile;

        Shader shader = Shader.Find("Mobile/Particles/Additive");
        if (!shader) shader = Shader.Find("Sprites/Default");

        beamMat = new Material(shader);
        beamMat.color = new Color(0.6f, 0.2f, 1f, 1f);
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

        line.enabled = true;
        AnimateBeam();
    }

    void AnimateBeam()
    {
        Transform originTf = rayOrigin != null ? rayOrigin : transform;
        Vector3 start = originTf.position;
        Vector3 target = bossShield.transform.position;

        Vector3 dir = (target - start).normalized;
        float dist = Vector3.Distance(start, target);
        if (dist > maxRayDistance) dist = maxRayDistance;
        Vector3 end = start + dir * dist;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        float time = Time.time;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);

            Vector3 pos = Vector3.Lerp(start, end, t);

            if (i > 0 && i < pointCount - 1)
            {
                float angle = (time * swirlSpeed) + (t * Mathf.PI * 2f * spiralFrequency);

                float x = Mathf.Cos(angle) * swirlRadius;
                float y = Mathf.Sin(angle) * swirlRadius;

                Vector3 swirlOffset = lookRot * new Vector3(x, y, 0);

                float taper = Mathf.Sin(t * Mathf.PI);
                pos += swirlOffset * taper;

                float noise = Mathf.Sin((time * jitterSpeed) + (i * 10f));
                Vector3 jitter = lookRot * new Vector3(noise, -noise, 0) * jaggedAmount * taper;

                pos += jitter;
            }

            line.SetPosition(i, pos);
        }

        beamMat.mainTextureOffset = new Vector2(Time.time * -3f, 0);
    }

    public void Deactivate()
    {
        IsActive = false;
        line.enabled = false;
    }
}