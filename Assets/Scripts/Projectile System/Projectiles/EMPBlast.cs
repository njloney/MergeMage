using UnityEngine;

public class EMPBlast : MonoBehaviour
{
    [Header("Blast Settings")]
    [SerializeField] private float blastRadius = 8f;
    [SerializeField] private float knockbackForce = 1500f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float upwardLift = 3.0f; // Pops enemies into the air

    [Header("Visuals")]
    [SerializeField] private ParticleSystem shockwaveParticles;
    [SerializeField] private GameObject sphereVisual; // Optional: A rapidly expanding sphere

    [SerializeField] private LayerMask hitMask = ~0;

    private void Start()
    {
        // Execute logic immediately
        Explode();

        Destroy(gameObject, 2.0f);
    }

    private void Explode()
    {
        // Play Particles
        if (shockwaveParticles != null) shockwaveParticles.Play();

        // Optional: Animate a sphere expanding (simple tween)
        if (sphereVisual != null) StartCoroutine(AnimateSphere());

        // Find Targets
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius, hitMask);

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue; // Ignore triggers

            // Deal Damage
            if (hit.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage, DamageType.Lightning, this);
            }

            // Apply Knockback
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.AddExplosionForce(knockbackForce, transform.position, blastRadius, upwardLift);
                }
            }
        }
    }

    private System.Collections.IEnumerator AnimateSphere()
    {
        float timer = 0f;
        float duration = 0.3f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * (blastRadius * 2f); // Diameter

        sphereVisual.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            // EaseOut curve for a "Pop" feel
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            sphereVisual.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade out if possible (requires material with transparency)
            // renderer.material.color.a = Mathf.Lerp(1, 0, t);

            yield return null;
        }
        sphereVisual.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}