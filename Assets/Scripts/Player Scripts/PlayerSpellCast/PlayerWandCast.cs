using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private SpellCombinationResolver resolver;

    [Header("Projectile Casting")]
    [SerializeField] private GameObject projectilePrefab;

    private ItemData currentActive;
    private bool mergeMode = false;

    private void Start()
    {
        // Listen to inventory
        inventoryManager.OnActiveItemChanged += GetAmmoFromInventory;
        inventoryManager.OnMergeModeChanged += CheckMergeMode;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryCast();
    }

    // ------------------ EVENT HANDLERS ------------------

    private void GetAmmoFromInventory(ItemData item)
    {
        currentActive = item;

        if (item == null)
            Debug.Log("[WandCast] Active item is NULL");
        else
            Debug.Log("[WandCast] Active item: " + item.itemName);
    }

    private void CheckMergeMode(bool isMerge)
    {
        mergeMode = isMerge;
    }

    // ------------------ SPELL RESOLUTION ------------------

    private ProjectileStats ResolveSpellStats()
    {
        if (mergeMode && currentActive != null && currentActive.isSpellItem && currentActive.projectileStats!= null)
        {
            return currentActive.projectileStats;
        }

        if (!mergeMode && currentActive != null && currentActive.itemType == ItemType.Crystal && !currentActive.isSpellItem)
        {
            return resolver.GetBaseStats(currentActive.crystalType);
        }

        // No valid spell
        return null;
    }



    // ------------------ CASTING LOGIC ------------------

    private void TryCast()
    {
        if (firePoint == null)
        {
            Debug.LogError("[WandCast] firePoint missing.");
            return;
        }

        // Did we use an unstable combo spell?
        bool usingUnstableSpell =
            mergeMode &&
            currentActive != null &&
            currentActive.isSpellItem &&
            currentActive.projectileStats != null;


        ProjectileStats stats = ResolveSpellStats();
        if (stats == null)
        {
            Debug.Log("[WandCast] No spell found to cast.");
            return;
        }

        switch (stats.castType)
        {
            case SpellCastType.Projectile:
                CastProjectileSpell(stats);
                break;

            case SpellCastType.Beam:
                CastBeamSpell(stats);
                break;

            default:
                Debug.LogWarning($"Spell '{stats.name}' has invalid castType={stats.castType}");
                return;
        }

        // If we cast an unstable spell, delete it and exit merge mode
        if (usingUnstableSpell)
        {
            inventoryManager.ConsumeMergeSpell();
            currentActive = null;
        }
    }

    // ------------------ PROJECTILE SPELL ------------------

    private void CastProjectileSpell(ProjectileStats stats)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[WandCast] Missing projectilePrefab.");
            return;
        }

        GameObject prefabToUse = stats.projectileOverridePrefab != null
        ? stats.projectileOverridePrefab
        : projectilePrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("[WandCast] No projectile prefab available (override is null AND wand prefab is null).");
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

    // ------------------ BEAM SPELL ------------------

    private void CastBeamSpell(ProjectileStats stats)
    {
        if (stats.beamPrefab == null || stats.beamConfig == null)
        {
            Debug.LogWarning("[WandCast] Beam spell missing prefab or config.");
            return;
        }

        LightRayBeam beam = Instantiate(stats.beamPrefab);
        beam.Init(firePoint, this, stats.beamConfig);
    }
}
