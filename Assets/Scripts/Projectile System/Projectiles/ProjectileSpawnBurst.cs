using UnityEngine;

[RequireComponent(typeof(ProjectileConfig))]
public class ProjectileSpawnBurst : MonoBehaviour
{
    [Header("Burst Settings")]
    [Tooltip("The projectile to shoot (e.g., a small rock).")]
    [SerializeField] private GameObject pelletPrefab;

    [SerializeField] private int pelletCount = 8; // Increased default for shotgun feel

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
        // 1. Get the owner (Player) so we can pass it to the pellets
        var myConfig = GetComponent<ProjectileConfig>();
        Object owner = myConfig ? myConfig.owner : null;

        if (pelletPrefab == null)
        {
            Debug.LogWarning("[ProjectileSpawnBurst] No pellet prefab assigned!");
            return;
        }

        for (int i = 0; i < pelletCount; i++)
        {
            // 2. Calculate Random Rotation (Cone/Box Spread)
            // We multiply by 0.5 because spread is total angle (e.g., 30 deg means -15 to +15)
            float yAngle = Random.Range(-horizontalSpread, horizontalSpread) * 0.5f;
            float xAngle = Random.Range(-verticalSpread, verticalSpread) * 0.5f;

            // Create a rotation offset based on these random angles
            Quaternion spreadRotation = Quaternion.Euler(xAngle, yAngle, 0);

            // Combine with the launcher's facing direction
            Quaternion finalRotation = transform.rotation * spreadRotation;

            // 3. Instantiate
            // Because we pass 'finalRotation', the "Pointy" model will spawn facing its fly path.
            GameObject pellet = Instantiate(pelletPrefab, transform.position, finalRotation);

            // 4. Setup Config (Owner)
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