using UnityEngine;

public class GolemMeleeAttack : MonoBehaviour
{
    public float damage = 20f;
    public float knockbackForce = 5f;
    public float attackRadius = 3f;
    public LayerMask playerMask;

    public void DoMelee()
    {
        var hits = Physics.OverlapSphere(transform.position, attackRadius, playerMask);
        Debug.Log($"[GolemMelee] OverlapSphere hits: {hits.Length}");

        foreach (var h in hits)
        {
            Debug.Log($"[GolemMelee] Hit collider: {h.name}");

            var hp = h.GetComponentInParent<Health>();
            if (hp != null)
            {
                Debug.Log($"[GolemMelee] Applying damage {damage} to {hp.name}");
                hp.TakeDamage(damage, DamageType.Physical, this);
            }
            else
            {
                Debug.Log("[GolemMelee] No Health found on hit target");
            }

            var rb = h.GetComponentInParent<Rigidbody>();
            var controller = h.GetComponentInParent<CharacterController>();

            Vector3 dir = (h.transform.position - transform.position).normalized;
            dir.y = 0f;

            if (rb != null)
            {
                Debug.Log("[GolemMelee] Applying Rigidbody knockback");
                rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
            }
            else if (controller != null)
            {
                Debug.Log("[GolemMelee] Applying CharacterController knockback");
                controller.Move(dir * knockbackForce);
            }
            else
            {
                Debug.Log("[GolemMelee] No Rigidbody or CharacterController for knockback");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
