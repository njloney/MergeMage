using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Projectile Casting")]
    [Tooltip("Fallback projectile prefab if the spell's stats don't override it.")]
    [SerializeField] private GameObject projectilePrefab;

    private ItemData currentActive;
    private ItemData currentInactive;
    private bool mergeMode = false;

    private void Start()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnActiveItemChanged += OnActiveItemChanged;
            inventoryManager.OnMergeModeChanged += OnMergeModeChanged;
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnActiveItemChanged -= OnActiveItemChanged;
            inventoryManager.OnMergeModeChanged -= OnMergeModeChanged;
        }
    }

    private void OnActiveItemChanged(ItemData item)
    {
        currentActive = item;

        if (item == null)
            Debug.Log("[WandCast] Active item is NULL");
        else
            Debug.Log("[WandCast] Active item set to: " + item.itemName);
    }

    private void OnMergeModeChanged(bool isMerge)
    {
        mergeMode = isMerge;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryCast();
        }
    }

    private void TryCast()
    {
        if (firePoint == null)
        {
            Debug.LogError("[WandCast] firePoint missing.");
            return;
        }

        if (currentActive == null || !currentActive.isSpellItem || currentActive.spellPrefab == null)
        {
            Debug.Log("[WandCast] No valid spell on active item.");
            return;
        }

        // Check if we are in merge mode
        if (IsMergeModeActive())
        {
            Debug.Log("[WandCast] In merge mode, using merged spell data.");
            ItemData mergedSpell = inventoryManager.GetMergeItem();

            if (mergedSpell != null)
            {
                CastMergedSpell(mergedSpell);
                inventoryManager.ConsumeMergeSpell();
            }
            else
            {
                Debug.LogError("[WandCast] No merged spell available.");
            }
            return;
        }

        // Directly cast the spell based on the current active item
        if (currentActive.castType == SpellCastType.Beam)
        {
            Debug.Log("[WandCast] Attempting to cast beam spell.");
            CastBeamSpell(currentActive.projectileStats);
        }
        else if (currentActive.castType == SpellCastType.Projectile)
        {
            Debug.Log("[WandCast] Attempting to cast projectile spell.");
            CastProjectileSpell(currentActive.projectileStats);
        }
        else
        {
            Debug.LogError("[WandCast] Invalid spell type for casting.");
        }
    }

    // Check if merge mode is active
    private bool IsMergeModeActive()
    {
        return mergeMode; // Adjust this according to your merge mode logic
    }

    // Method to cast the merged spell
    private void CastMergedSpell(ItemData mergedSpell)
    {
        GameObject spellPrefab = Instantiate(mergedSpell.spellPrefab, firePoint.position, firePoint.rotation);
        Debug.Log($"Cast merged spell: {mergedSpell.itemName}");
    }

    // ---------- PROJECTILE SPELLS ----------

    private void CastProjectileSpell(ProjectileStats stats)
    {
        // Optional override per spell
        GameObject prefabToUse = stats.projectileOverridePrefab != null
            ? stats.projectileOverridePrefab
            : projectilePrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("[WandCast] No projectile prefab available.");
            return;
        }

        var go = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }

        go.SetActive(true);
    }

    // ---------- BEAM SPELLS ----------

    private void CastBeamSpell(ProjectileStats stats)
    {
        if (stats.beamPrefab == null || stats.beamConfig == null)
        {
            Debug.LogWarning($"[WandCast] Beam spell '{stats.name}' missing prefab or config.");
            return;
        }

        LightRayBeam beam = Instantiate(stats.beamPrefab);
        beam.Init(firePoint, this, stats.beamConfig);
    }
}
