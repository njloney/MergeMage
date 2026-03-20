using UnityEngine;

public class GolemSpellResponder : MonoBehaviour
{
    [SerializeField] private GolemSpellResponseResolver responseResolver;
    [SerializeField] private ProjectileStats basicRangedSpell;
    [SerializeField] private float storedSpellDuration = 8f;

    public ProjectileStats StoredRangedSpell { get; private set; }
    public bool HasStoredSpell { get; private set; }

    float storedTimer;

    void Awake()
    {
        StoredRangedSpell = null;
        HasStoredSpell = false;
        storedTimer = 0f;
    }

    void Update()
    {
        if (!HasStoredSpell)
            return;

        storedTimer -= Time.deltaTime;

        if (storedTimer <= 0f)
        {
            StoredRangedSpell = null;
            HasStoredSpell = false;
            Debug.Log("[GolemResponder] Stored spell expired reverting to basic");
        }
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
        storedTimer = storedSpellDuration;

        Debug.Log($"[GolemResponder] New stored spell: {StoredRangedSpell.name}");
    }

    public ProjectileStats GetCurrentRangedSpell()
    {
        if (HasStoredSpell && StoredRangedSpell != null)
            return StoredRangedSpell;

        return basicRangedSpell;
    }
}
