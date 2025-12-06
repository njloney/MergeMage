using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class IcePath : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private float speedBoost = 1.0f; // +100% Speed (Double speed)
    [SerializeField] private float lifetime = 5f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem frostParticles;

    private RuntimePlayerStats activePlayerStats;
    private bool applied = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (applied) return;

        if (other.CompareTag("Player"))
        {
            var stats = other.GetComponent<RuntimePlayerStats>();
            if (stats != null)
            {
                activePlayerStats = stats;
                // Add the multiplier (e.g., +1.0 means 2x speed)
                activePlayerStats.AddSpeedMultiplier(speedBoost);
                applied = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && activePlayerStats != null)
        {
            RemoveBuff();
        }
    }

    private void OnDestroy()
    {
        // Safety check: If the path melts while player is still on it, remove the buff!
        if (applied && activePlayerStats != null)
        {
            RemoveBuff();
        }
    }

    private void RemoveBuff()
    {
        if (activePlayerStats != null)
        {
            // Remove the multiplier we added
            activePlayerStats.AddSpeedMultiplier(-speedBoost);
            activePlayerStats = null;
            applied = false;
        }
    }
}