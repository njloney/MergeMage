using UnityEngine;

public class PlayerWand : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Spells/Effects")]
    [SerializeField] private SpellCombinationResolver resolver;

    [Header("Wand Visuals (Sockets)")]
    [Tooltip("Empty GameObject on the wand model for the first crystal")]
    [SerializeField] private Transform socketA;
    [Tooltip("Empty GameObject on the wand model for the second crystal")]
    [SerializeField] private Transform socketB;

    [Header("Crystal Models (Visual Only)")]
    [Tooltip("Visual-only prefab for a Fire crystal")]
    [SerializeField] private GameObject fireCrystalModel;
    [Tooltip("Visual-only prefab for an Ice crystal")]
    [SerializeField] private GameObject iceCrystalModel;
    [Tooltip("Visual-only prefab for a Wind crystal")]
    [SerializeField] private GameObject windCrystalModel;
    [Tooltip("Visual-only prefab for an Earth crystal")]
    [SerializeField] private GameObject earthCrystalModel;
    [Tooltip("Visual-only prefab for a Lightning crystal")]
    [SerializeField] private GameObject lightningCrystalModel;
    [Tooltip("Visual-only prefab for a Light crystal")]
    [SerializeField] private GameObject lightCrystalModel;
    [Tooltip("Visual-only prefab for a Dark crystal")]
    [SerializeField] private GameObject darkCrystalModel;

    [Header("Dropping")]
    [Tooltip("Point where crystals are dropped (e.g., behind the player)")]
    [SerializeField] private Transform dropPoint;

    [Header("Crystal Pickup Prefabs")]
    [Tooltip("The 'CrystalPickup' prefab for Fire")]
    [SerializeField] private GameObject fireCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Ice")]
    [SerializeField] private GameObject iceCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Wind")]
    [SerializeField] private GameObject windCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Earth")]
    [SerializeField] private GameObject earthCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Lightning")]
    [SerializeField] private GameObject lightningCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Light")]
    [SerializeField] private GameObject lightCrystalPickupPrefab;
    [Tooltip("The 'CrystalPickup' prefab for Dark")]
    [SerializeField] private GameObject darkCrystalPickupPrefab;

    // --- Private State ---
    private CrystalType? slotA = null;
    private CrystalType? slotB = null;
    private GameObject spawnedModelA = null;
    private GameObject spawnedModelB = null;

    public bool HasA => slotA.HasValue;
    public bool HasB => slotB.HasValue;

    private void Start()
    {
        // Sync visuals on game start, in case we start with crystals
        UpdateWandVisuals();
    }

    public void Collect(CrystalType c)
    {
        // Fill slot A first
        if (!slotA.HasValue)
        {
            slotA = c;
            UpdateWandVisuals();
            return;
        }

        // Then slot B
        if (!slotB.HasValue)
        {
            slotB = c;
            UpdateWandVisuals();
            return;
        }

        // Both full: drop A, shift B to A, new -> B
        CrystalType crystalToDrop = slotA.Value;
        slotA = slotB;
        slotB = c;

        DropCrystal(crystalToDrop);
        UpdateWandVisuals();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryCast();
    }

    private void TryCast()
    {
        if (!slotA.HasValue || !slotB.HasValue || resolver == null || firePoint == null)
            return;

        var stats = resolver.BuildStats(slotA.Value, slotB.Value);
        if (stats == null)
        {
            Debug.Log("Spell fizzled (no recipe found).");
            return;
        }

        // Decide if this is a beam spell or projectile spell
        if (stats.isBeamSpell)
        {
            CastBeamSpell(stats);
        }
        else
        {
            CastProjectileSpell(stats);
        }

        // Consume both slots (for now)
        slotA = null;
        slotB = null;
        UpdateWandVisuals();
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

    private void DropCrystal(CrystalType typeToDrop)
    {
        if (dropPoint == null)
        {
            Debug.LogWarning("No drop point set on wand. Cannot drop crystal.");
            return;
        }

        GameObject prefabToDrop = GetPickupPrefab(typeToDrop);

        if (prefabToDrop != null)
        {
            Instantiate(prefabToDrop, dropPoint.position, dropPoint.rotation);
        }
        else
        {
            Debug.LogWarning($"No pickup prefab assigned for crystal type {typeToDrop}.");
        }
    }

    private void UpdateWandVisuals()
    {
        // Clear old models
        if (spawnedModelA != null) Destroy(spawnedModelA);
        if (spawnedModelB != null) Destroy(spawnedModelB);

        // Slot A model
        if (slotA.HasValue && socketA != null)
        {
            GameObject modelPrefab = GetModelPrefab(slotA.Value);
            if (modelPrefab != null)
            {
                spawnedModelA = Instantiate(modelPrefab, socketA.position, socketA.rotation, socketA);
            }
        }

        // Slot B model
        if (slotB.HasValue && socketB != null)
        {
            GameObject modelPrefab = GetModelPrefab(slotB.Value);
            if (modelPrefab != null)
            {
                spawnedModelB = Instantiate(modelPrefab, socketB.position, socketB.rotation, socketB);
            }
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

    private GameObject GetPickupPrefab(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.Fire: return fireCrystalPickupPrefab;
            case CrystalType.Ice: return iceCrystalPickupPrefab;
            case CrystalType.Wind: return windCrystalPickupPrefab;
            case CrystalType.Earth: return earthCrystalPickupPrefab;
            case CrystalType.Lightning: return lightningCrystalPickupPrefab;
            case CrystalType.Light: return lightCrystalPickupPrefab;
            case CrystalType.Dark: return darkCrystalPickupPrefab;
            default: return null;
        }
    }
}
