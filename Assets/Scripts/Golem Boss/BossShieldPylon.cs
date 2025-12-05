using UnityEngine;

public class BossShieldPylon : MonoBehaviour
{
    public BossShieldController bossShield;
    public float radius = 4f;
    public float channelTime = 3f;
    public LayerMask playerMask;

    public bool IsActive { get; private set; } = true;

    float progress;

    void Start()
    {
        if (bossShield != null)
            bossShield.RegisterPylon(this);
    }

    void Update()
    {
        if (!IsActive)
            return;

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

    void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        if (bossShield != null)
            bossShield.UnregisterPylon(this);

        Debug.Log("[Pylon] Deactivated");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
