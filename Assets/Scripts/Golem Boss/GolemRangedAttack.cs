using UnityEngine;

public class GolemRangedAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GolemSpellResponder responder;

    private void Awake()
    {
        if (responder == null)
            responder = GetComponent<GolemSpellResponder>();
    }

    public void CastRanged()
    {
        if (firePoint == null || projectilePrefab == null || responder == null)
            return;

        var stats = responder.GetCurrentRangedSpell();
        if (stats == null)
            return;

        if (stats.castType == SpellCastType.Beam || stats.isBeamSpell)
            return;

        var go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }

        go.SetActive(true);
    }
}
