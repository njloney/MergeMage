using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private DamageType damageType = DamageType.Lightning;

    [Header("Visuals")]
    [SerializeField] private GameObject strikeEffectPrefab; // bolt
    [SerializeField] private GameObject impactEffectPrefab; // explosion on hit

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(Strike), delay);
        Destroy(gameObject, delay);
    }

    private void Strike()
    {
        if (strikeEffectPrefab)
        {
            Instantiate(strikeEffectPrefab, transform.position, Quaternion.identity);
        }
        // AoE damage
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, hitMask, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage, damageType, this);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
