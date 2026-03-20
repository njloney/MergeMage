using System.Collections.Generic;
using UnityEngine;

// Attach to a projectile config (or call as utility) to chain on first target hit.
public class ChainLightningOnHit : MonoBehaviour
{
    [SerializeField] private int maxChains = 3;          // how many jumps after the first
    [SerializeField] private float chainRadius = 6f;     // search radius for next target
    [SerializeField] private float damagePerJump = 8f;   // damage per target
    [SerializeField] private LayerMask enemyMask = ~0;

    // Call this from Projectile when first hit happens.
    public void DoChain(Vector3 startPos, Collider firstTarget, DamageType type, Object owner)
    {
        var visited = new HashSet<Collider>();
        Collider current = firstTarget;
        visited.Add(current);

        for (int i = 0; i < maxChains; i++)
        {
            if (current == null) break;

            // deal damage to current
            if (current.TryGetComponent<Health>(out var hp))
                hp.TakeDamage(damagePerJump, type, owner);

            // find next target near current
            Collider next = FindNext(current.transform.position, visited);
            if (next == null) break;
            visited.Add(next);
            current = next;
        }
    }

    private Collider FindNext(Vector3 from, HashSet<Collider> visited)
    {
        var hits = Physics.OverlapSphere(from, chainRadius, enemyMask, QueryTriggerInteraction.Ignore);
        float bestDist = float.MaxValue;
        Collider best = null;

        foreach (var h in hits)
        {
            if (!h || visited.Contains(h)) continue;
            float d = (h.transform.position - from).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = h; }
        }
        return best;
    }
}
