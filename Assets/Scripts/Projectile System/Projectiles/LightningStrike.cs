using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float delay = 0.2f; // Short delay for impact feel, or set to 0
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visual References")]
    [SerializeField] private GameObject boltVisual;    // The actual lightning bolt
    [SerializeField] private GameObject impactEffect;  // Sparks/Explosion prefab

    private void Start()
    {
        // Hide bolt initially if there is a delay
        if (boltVisual) boltVisual.SetActive(false);

        Invoke(nameof(Strike), delay);
        Destroy(gameObject, duration);
    }

    private void Strike()
    {
        // Show the bolt immediately when strike happens
        if (boltVisual) boltVisual.SetActive(true);

        if (impactEffect) Instantiate(impactEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, hitMask);
        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            if (hit.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage, DamageType.Lightning, null);
            }
        }
    }
}