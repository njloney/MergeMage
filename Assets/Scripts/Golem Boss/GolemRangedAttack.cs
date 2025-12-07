using UnityEngine;

public class GolemRangedAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GolemSpellResponder responder;
    [SerializeField] private Transform player;
    [SerializeField] private Transform bossCenter;
    [SerializeField] private LayerMask groundMask = ~0;

    [SerializeField] private float aimUpOffset = 1.0f;

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

        Debug.Log($"[GolemRanged] CastRanged with stats: {stats.name}");

        if (stats.isWindPullSpell && stats.windPullPrefab != null)
        {
            Debug.Log("[GolemRanged] Wind spell branch entered");

            Transform center = bossCenter != null ? bossCenter : transform;
            Vector3 centerPos = center.position;
            Vector3 spawnPos = centerPos;

            RaycastHit[] hits = Physics.RaycastAll(
                centerPos + Vector3.up * 10f,
                Vector3.down,
                50f,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            Debug.Log($"[GolemRanged] RaycastAll hit count: {hits.Length}");

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h.collider == null) continue;

                Debug.Log($"[GolemRanged] Hit #{i}: {h.collider.name} (layer {h.collider.gameObject.layer})");

                if (h.collider.transform == center || h.collider.transform.IsChildOf(center))
                    continue;

                spawnPos = h.point;
                break;
            }

            GameObject fieldObj = Instantiate(stats.windPullPrefab, spawnPos, Quaternion.identity);

            if (fieldObj.TryGetComponent<WindPullField>(out var field))
                field.Init(center);

            Debug.Log($"[GolemRanged] Spawned wind field at {spawnPos}");

            return;
        }

        if (stats.castType == SpellCastType.Beam || stats.isBeamSpell)
            return;

        if (player != null)
        {
            Vector3 targetPos = player.position;
            targetPos.y += aimUpOffset;

            Vector3 toPlayer = targetPos - firePoint.position;
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
