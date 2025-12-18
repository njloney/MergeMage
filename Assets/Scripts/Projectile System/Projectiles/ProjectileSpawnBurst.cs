using UnityEngine;

[RequireComponent(typeof(ProjectileConfig))]
public class ProjectileSpawnBurst : MonoBehaviour
{
    [Header("Burst Settings")]
    [Tooltip("The projectile to shoot (e.g., a small rock).")]
    [SerializeField] private GameObject pelletPrefab;

    [SerializeField] private int pelletCount = 8;

    [Header("Spread Configuration")]
    [Tooltip("Randomness angle left-to-right.")]
    [SerializeField] private float horizontalSpread = 35f;

    [Tooltip("Randomness angle up-and-down.")]
    [SerializeField] private float verticalSpread = 15f;

    [Tooltip("Randomness added to forward speed.")]
    [SerializeField] private float speedJitter = 4f;

    private void Start()
    {
        SpawnShrapnel();
        Destroy(gameObject); // Remove the launcher immediately
    }

    private void SpawnShrapnel()
    {
        var myConfig = GetComponent<ProjectileConfig>();
        Object owner = myConfig ? myConfig.owner : null;

        if (pelletPrefab == null)
        {
            Debug.LogWarning("[ProjectileSpawnBurst] No pellet prefab assigned!");
            return;
        }

        for (int i = 0; i < pelletCount; i++)
        {
            float yAngle = Random.Range(-horizontalSpread, horizontalSpread) * 0.5f;
            float xAngle = Random.Range(-verticalSpread, verticalSpread) * 0.5f;

            Quaternion spreadRotation = Quaternion.Euler(xAngle, yAngle, 0);

            Quaternion finalRotation = transform.rotation * spreadRotation;

            GameObject pellet = Instantiate(pelletPrefab, transform.position, finalRotation);

            if (pellet.TryGetComponent<ProjectileConfig>(out var pCfg))
            {
                pCfg.owner = owner;
            }

            if (speedJitter > 0 && pellet.TryGetComponent<Rigidbody>(out var rb))
            {
                float extraSpeed = Random.Range(-speedJitter, speedJitter);
                rb.linearVelocity += pellet.transform.forward * extraSpeed;
            }
        }
    }
}