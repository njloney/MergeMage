using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private InventoryManager inventoryManager; 

    private ItemData currentActive;

    private bool mergeMode;

    private void Start()
    {
        inventoryManager.OnActiveItemChanged += GetAmmoFromInventory;
        inventoryManager.OnMergeModeChanged += CheckMergeMode;
    }

    private void CheckMergeMode(bool isMerge)
    {
        mergeMode = isMerge;
    }

    private void GetAmmoFromInventory(ItemData item)
    {
        currentActive = item;

        if (currentActive == null)
        {
            Debug.Log("[WandCast] Current active item is NULL.");
        }
        else
        {
            Debug.Log($"[WandCast] Active item set to: {currentActive.itemName}");
        }
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
        if (currentActive == null)
        {
            Debug.Log("[WandCast] No active spell item to cast.");
            return;
        }

        // Decide spell behavior by castType
        switch (currentActive.castType)
        {
            case SpellCastType.Projectile:
                CastProjectileSpell();
                break;

            case SpellCastType.Beam:
                CastBeamSpell();
                break;

            case SpellCastType.None:
            default:
                Debug.LogWarning($"[WandCast] Item '{currentActive.itemName}' cannot be cast (castType = {currentActive.castType}).");
                break;
        }

        if (mergeMode)
        {
            inventoryManager.ToggleMergeMode();
        }
    }

    private void CastProjectileSpell()
    {
        if (currentActive.projectilePrefab == null)
        {
            Debug.LogWarning($"[WandCast] Item '{currentActive.itemName}' has castType = Projectile but no projectilePrefab assigned.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("[WandCast] FirePoint is not assigned on PlayerWandCast.");
            return;
        }

        var go = Instantiate(currentActive.projectilePrefab, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.owner = this;
        }

        go.SetActive(true);
    }

    private void CastBeamSpell()
    {
        if (currentActive.beamPrefab == null || currentActive.beamConfig == null)
        {
            Debug.LogWarning(
                $"[WandCast] Item '{currentActive.itemName}' has castType = Beam but beamPrefab or beamConfig is not assigned."
            );
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("[WandCast] FirePoint is not assigned on PlayerWandCast.");
            return;
        }

        LightRayBeam beam = Instantiate(currentActive.beamPrefab);

        beam.Init(firePoint, this, currentActive.beamConfig);
    }
}
