using UnityEngine;

public class GolemSpellResponder : MonoBehaviour
{
    [SerializeField] private GolemSpellResponseResolver responseResolver;
    [SerializeField] private ProjectileStats basicRangedSpell;

    public ProjectileStats StoredRangedSpell { get; private set; }
    public bool HasStoredSpell { get; private set; }

    private void Awake()
    {
        StoredRangedSpell = null;
        HasStoredSpell = false;
    }

    public void OnHitBySpell(ProjectileStats incomingStats, Object source)
    {
        if (incomingStats == null)
            return;

        ProjectileStats next = null;

        if (responseResolver != null &&
            responseResolver.TryGetOptions(incomingStats, out var a, out var b) &&
            a != null)
        {
            if (b == null || b == a)
                next = a;
            else
                next = (Random.value < 0.5f) ? a : b;
        }
        else
        {
            next = incomingStats;
        }

        StoredRangedSpell = next;
        HasStoredSpell = true;
    }

    public ProjectileStats GetCurrentRangedSpell()
    {
        if (HasStoredSpell && StoredRangedSpell != null)
            return StoredRangedSpell;

        return basicRangedSpell;
    }
}
