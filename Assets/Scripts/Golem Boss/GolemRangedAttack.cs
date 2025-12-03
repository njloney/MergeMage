using UnityEngine;

public class GolemRangedAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GolemSpellResponder responder;
    [SerializeField] private Transform player;

    void Awake()
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

        if (player != null)
        {
            Vector3 toPlayer = player.position - firePoint.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.0001f)
                firePoint.rotation = Quaternion.LookRotation(toPlayer.normalized);
        }

        GameObject go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }

        go.SetActive(true);
    }
}
