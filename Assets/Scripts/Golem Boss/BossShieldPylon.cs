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

    [SerializeField] AudioClip[] chargeStepClips;
    [SerializeField] float chargeVolume = 0.9f;
    [SerializeField] float baseChargePitch = 1f;
    [SerializeField] float pitchIncreasePerStep = 0.05f;

    [SerializeField] AudioClip destroyClip;
    [SerializeField] float destroyVolume = 1f;

    public bool IsActive { get; private set; } = true;

    float progress;
    LineRenderer line;

    int lastStep = -1;
    int playCount = 0;

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
