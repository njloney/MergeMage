using UnityEngine;

public class GolemRangedAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GolemSpellResponder responder;
    [SerializeField] private Transform player;
    [SerializeField] private Transform floorOfBoss;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float aimUpOffset = 1.0f;

    [Header("Visuals")]
    [SerializeField] private GameObject castVFX; // [NEW]

    void Awake()
    {
        if (responder == null)
            responder = GetComponent<GolemSpellResponder>();
    }

    public void CastRanged()
    {
        if (firePoint == null || projectilePrefab == null || responder == null)
            return;

        if (castVFX != null)
        {
            Instantiate(castVFX, firePoint.position, firePoint.rotation);
        }

        var stats = responder.GetCurrentRangedSpell();
        if (stats == null) return;

        if (stats.isWindPullSpell && stats.windPullPrefab != null) { /*...*/ return; }
        if (stats.isEarthPathSpell && stats.earthPathPrefab != null) { /*...*/ return; }
        if (stats.isGroundSpell && stats.groundSpellPrefab != null) { /*...*/ return; }
        if (stats.castType == SpellCastType.Beam || stats.isBeamSpell) return;

        if (player != null)
        {
            Vector3 targetPos = player.position;
            targetPos.y += aimUpOffset;
            Vector3 toPlayer = targetPos - firePoint.position;
            if (toPlayer.sqrMagnitude > 0.0001f)
                firePoint.rotation = Quaternion.LookRotation(toPlayer.normalized);
        }

        GameObject prefabToUse = stats.projectileOverridePrefab != null ? stats.projectileOverridePrefab : projectilePrefab;
        GameObject go = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }
        go.SetActive(true);
    }
}