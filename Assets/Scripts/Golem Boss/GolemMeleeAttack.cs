using UnityEngine;
using System.Collections;

public class GolemMeleeAttack : MonoBehaviour
{
    [Header("Combat Stats")]
    public float damage = 20f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float attackRadius = 3f;
    public LayerMask playerMask;

    [Header("Visuals")]
    [Tooltip("Particle effect spawned at the hit point")]
    public GameObject smashVFX;
    [Tooltip("Where to spawn the VFX. Defaults to Golem center if null.")]
    public Transform hitPoint;

    public void DoMelee()
    {
        // 1. Spawn Visuals
        if (smashVFX != null)
        {
            Vector3 spawnPos = hitPoint != null ? hitPoint.position : transform.position + transform.forward * 2f;
            // Ensure it's on the ground
            spawnPos.y = transform.position.y;
            Instantiate(smashVFX, spawnPos, Quaternion.identity);
        }

        // 2. Detect & Damage
        var hits = Physics.OverlapSphere(transform.position, attackRadius, playerMask);

        foreach (var h in hits)
        {
            var hp = h.GetComponentInParent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage, DamageType.Physical, this);
            }

            var rb = h.GetComponentInParent<Rigidbody>();
            var controller = h.GetComponentInParent<CharacterController>();

            Vector3 dir = (h.transform.position - transform.position).normalized;
            dir.y = 0f;

            if (rb != null)
            {
                rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
            }
            else if (controller != null)
            {
                StartCoroutine(DoControllerKnockback(controller, dir));
            }
        }
    }

    private IEnumerator DoControllerKnockback(CharacterController controller, Vector3 dir)
    {
        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            controller.Move(dir * (knockbackForce / knockbackDuration) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}