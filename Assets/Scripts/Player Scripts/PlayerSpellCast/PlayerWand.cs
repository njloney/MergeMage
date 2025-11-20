using UnityEngine;

public class PlayerWand : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Spells/Effects")]
    [SerializeField] private SpellCombinationResolver resolver;

    [Header("Wand Visuals (Sockets)")]
    [Tooltip("Empty GameObject on the wand model for the first crystal (inventory crystal slot 1)")]
    [SerializeField] private Transform socketA;
    [Tooltip("Empty GameObject on the wand model for the second crystal (inventory crystal slot 2)")]
    [SerializeField] private Transform socketB;

    [Header("Crystal Models (Visual Only, per CrystalType)")]
    [SerializeField] private GameObject fireCrystalModel;
    [SerializeField] private GameObject iceCrystalModel;
    [SerializeField] private GameObject windCrystalModel;
    [SerializeField] private GameObject earthCrystalModel;
    [SerializeField] private GameObject lightningCrystalModel;
    [SerializeField] private GameObject lightCrystalModel;
    [SerializeField] private GameObject darkCrystalModel;

    [Header("Inventory Link")]
    [Tooltip("InventoryManager that owns the crystal slots")]
    [SerializeField] private InventoryManager inventoryManager;

    // runtime spawned models attached to the wand
    private GameObject spawnedModelA;
    private GameObject spawnedModelB;

    // cached last types so we only refresh visuals when something actually changes
    private CrystalType? lastTypeA = null;
    private CrystalType? lastTypeB = null;

    private void Start()
    {
        RefreshFromInventory();
    }

    private void Update()
    {
        SyncVisualsIfSlotsChanged();

        if (Input.GetMouseButtonDown(0))
            TryCast();
    }

    // -------------------------------------------------------------
    // INVENTORY WAND VISUALS
    // -------------------------------------------------------------

    private void SyncVisualsIfSlotsChanged()
    {
        if (inventoryManager == null) return;

        var (typeA, typeB) = GetCrystalTypesFromInventory();

        if (typeA != lastTypeA || typeB != lastTypeB)
        {
            UpdateWandVisuals(typeA, typeB);
            lastTypeA = typeA;
            lastTypeB = typeB;
        }
    }

    private void RefreshFromInventory()
    {
        if (inventoryManager == null) return;

        var (typeA, typeB) = GetCrystalTypesFromInventory();
        UpdateWandVisuals(typeA, typeB);
        lastTypeA = typeA;
        lastTypeB = typeB;
    }

    private (CrystalType?, CrystalType?) GetCrystalTypesFromInventory()
    {
        CrystalType? typeA = null;
        CrystalType? typeB = null;

        if (inventoryManager != null)
        {
            var slot1 = inventoryManager.CrystalSlot1;
            var slot2 = inventoryManager.CrystalSlot2;

            ItemData item1 = slot1 != null ? slot1.currentItem : null;
            ItemData item2 = slot2 != null ? slot2.currentItem : null;

            if (item1 != null && item1.itemType == ItemType.Crystal)
                typeA = item1.crystalType;

            if (item2 != null && item2.itemType == ItemType.Crystal)
                typeB = item2.crystalType;
        }

        return (typeA, typeB);
    }

    private void UpdateWandVisuals(CrystalType? typeA, CrystalType? typeB)
    {
        if (spawnedModelA != null) Destroy(spawnedModelA);
        if (spawnedModelB != null) Destroy(spawnedModelB);

        if (typeA.HasValue && socketA != null)
        {
            var prefabA = GetModelPrefab(typeA.Value);
            if (prefabA != null)
            {
                spawnedModelA = Instantiate(prefabA, socketA.position, socketA.rotation, socketA);
                MakeWandSafe(spawnedModelA);
            }
        }

        if (typeB.HasValue && socketB != null)
        {
            var prefabB = GetModelPrefab(typeB.Value);
            if (prefabB != null)
            {
                spawnedModelB = Instantiate(prefabB, socketB.position, socketB.rotation, socketB);
                MakeWandSafe(spawnedModelB);
            }
        }
    }

    private void MakeWandSafe(GameObject modelRoot)
    {
        if (modelRoot == null) return;

        foreach (var rb in modelRoot.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (var col in modelRoot.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }

    private GameObject GetModelPrefab(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.Fire: return fireCrystalModel;
            case CrystalType.Ice: return iceCrystalModel;
            case CrystalType.Wind: return windCrystalModel;
            case CrystalType.Earth: return earthCrystalModel;
            case CrystalType.Lightning: return lightningCrystalModel;
            case CrystalType.Light: return lightCrystalModel;
            case CrystalType.Dark: return darkCrystalModel;
            default: return null;
        }
    }

    // -------------------------------------------------------------
    // CASTING
    // -------------------------------------------------------------

    private void TryCast()
    {
        if (resolver == null || firePoint == null || inventoryManager == null)
            return;

        var (typeA, typeB) = GetCrystalTypesFromInventory();

        if (!typeA.HasValue || !typeB.HasValue)
            return;

        var stats = resolver.BuildStats(typeA.Value, typeB.Value);
        if (stats == null)
        {
            Debug.Log("Spell fizzled (no recipe found).");
            return;
        }

        if (stats.isBeamSpell)
            CastBeamSpell(stats);
        else
            CastProjectileSpell(stats);
    }

    private void CastProjectileSpell(ProjectileStats stats)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectilePrefab assigned on PlayerWand, cannot fire projectile.");
            return;
        }

        var go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }

        go.SetActive(true);
    }

    private void CastBeamSpell(ProjectileStats stats)
    {
        if (stats.beamPrefab == null || stats.beamConfig == null)
        {
            Debug.LogWarning("Beam spell selected but beamPrefab or beamConfig is not assigned in ProjectileStats.", stats);
            return;
        }

        var beam = Instantiate(stats.beamPrefab);
        Transform origin = firePoint;
        beam.Init(origin, this, stats.beamConfig);
    }
}
