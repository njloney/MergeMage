using UnityEngine;

public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public float radius = 4f;
    public float channelTime = 3f;
    public LayerMask playerMask;

    float progress;
    bool isActive = true;

    void Start()
    {
        if (bossShield != null)
            bossShield.RegisterPylon(this);
    }

    void OnDestroy()
    {
        if (bossShield != null)
            bossShield.UnregisterPylon(this);
    }

    void Update()
    {
        if (!isActive)
            return;

        bool inRange = Physics.CheckSphere(transform.position, radius, playerMask);

        if (inRange)
        {
            progress += Time.deltaTime;
            Debug.Log($"[Pylon] Channel progress {progress:F2}/{channelTime:F2}");

            if (progress >= channelTime)
                Deactivate();
        }
        else if (progress > 0f)
        {
            progress -= Time.deltaTime;
            if (progress < 0f) progress = 0f;
        }
    }

    void Deactivate()
    {
        if (!isActive)
            return;

        isActive = false;
        Debug.Log("[Pylon] Deactivated and destroyed");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
