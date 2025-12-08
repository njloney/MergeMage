using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public Transform rayOrigin;
    public float radius = 4f;
    public float channelTime = 3f;
    public LayerMask playerMask;

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

    public LayerMask obstacleMask;
    public float beamWidth = 0.15f;

    [SerializeField] AudioClip[] chargeStepClips;
    [SerializeField] float chargeVolume = 0.9f;
    [SerializeField] float baseChargePitch = 1f;
    [SerializeField] float pitchIncreasePerStep = 0.05f;

    [SerializeField] AudioClip destroyClip;
    [SerializeField] float destroyVolume = 1f;
    float progress;

    int lastStep = -1;
    int playCount = 0;

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
        CastRayToBoss();

        bool inRange = Physics.CheckSphere(transform.position, radius, playerMask);

        if (inRange)
        {
            progress += Time.deltaTime;
            if (progress >= channelTime)
            {
                progress = channelTime;
                Deactivate();
            }
        }
        else if (progress > 0f)
        {
            progress -= Time.deltaTime;
            if (progress < 0f)
                progress = 0f;
        }

        HandleChargeSound();
    }

    void HandleChargeSound()
    {
        if (channelTime <= 0f)
            return;

        float t = Mathf.Clamp01(progress / channelTime);

        if (t <= 0f)
        {
            lastStep = -1;
            playCount = 0;
            return;
        }

        int step = Mathf.Clamp(Mathf.FloorToInt(t * 10f) - 1, -1, 9);

        if (step <= lastStep)
            return;

        for (int i = lastStep + 1; i <= step; i++)
        {
            if (i >= 0)
                PlayChargeStep(i);
        }

        lastStep = step;
    }

    void PlayChargeStep(int index)
    {
        if (chargeStepClips == null || chargeStepClips.Length == 0)
            return;

        int clipIndex = Mathf.Clamp(index, 0, chargeStepClips.Length - 1);
        var clip = chargeStepClips[clipIndex];
        if (clip == null)
            return;

        float pitch = baseChargePitch + playCount * pitchIncreasePerStep;
        playCount++;

        PlayClip(clip, chargeVolume, pitch);
    }

    void CastRayToBoss()
    {
        if (bossShield == null)
            return;

        Transform t = rayOrigin != null ? rayOrigin : transform;

        Vector3 o = t.position;
        Vector3 d = (bossShield.transform.position - o).normalized;
        float dist = Mathf.Min(Vector3.Distance(o, bossShield.transform.position), maxRayDistance);

        line.enabled = true;
        line.SetPosition(0, o);

        if (Physics.Raycast(o, d, out RaycastHit hit, dist, obstacleMask))
            line.SetPosition(1, hit.point);
        else
            line.SetPosition(1, o + d * dist);
    }

    void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        line.enabled = false;

        if (destroyClip != null)
            PlayClip(destroyClip, destroyVolume, 1f);

        if (bossShield != null)
            bossShield.UnregisterPylon(this);
    }

    void PlayClip(AudioClip clip, float volume, float pitch)
    {
        var go = new GameObject("PylonAudio");
        go.transform.position = transform.position;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;
        src.volume = volume;
        src.pitch = pitch;
        src.minDistance = 5f;
        src.maxDistance = 40f;
        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.1f, pitch));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}