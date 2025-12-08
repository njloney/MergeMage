using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float damage = 20f;
    // [UPDATED] Replaced radius with box dimensions (Width, Height, Depth)
    [Tooltip("Size of the hit box. Y is height.")]
    [SerializeField] private Vector3 boxSize = new Vector3(2f, 10f, 2f);

    [SerializeField] private float delay = 0.2f;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visual References")]
    [SerializeField] private GameObject boltVisual;
    [SerializeField] private GameObject impactEffect;

    private void Start()
    {
        if (boltVisual) boltVisual.SetActive(false);

        Invoke(nameof(Strike), delay);
        Destroy(gameObject, duration);
    }

    private void Strike()
    {
        if (boltVisual) boltVisual.SetActive(true);

        var impactAudio = GetComponent<SpellImpactAudio>();
        if (impactAudio != null)
            impactAudio.PlayImpactSound();

        if (impactEffect) Instantiate(impactEffect, transform.position, Quaternion.identity);

        Vector3 center = transform.position + (Vector3.up * boxSize.y * 0.5f);
        Vector3 halfExtents = boxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, hitMask);

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;

            if (hit.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage, DamageType.Lightning, null);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.3f); // Transparent Yellow
        Vector3 center = transform.position + (Vector3.up * boxSize.y * 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, boxSize);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}