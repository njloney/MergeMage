using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private MergeMode mergeController;
    [SerializeField] private Mana manaPool;

    [Header("Targeting")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundMask;

    [Header("Projectile Casting")]
    [SerializeField] private GameObject projectilePrefab;
    public bool debugDisableConsumption = false;

    private ItemData currentActive;
    private bool mergeMode = false;

    // Ground Targeting State
    private GameObject currentGhost;
    private bool validTargetFound;
    private Vector3 currentTargetPoint;
    private Quaternion currentTargetRotation;


    private void Start()
    {
        if (inventoryManager != null) inventoryManager.OnActiveItemChanged += OnActiveItemChanged;
        if (mergeController != null) mergeController.OnMergeModeChanged += OnMergeModeChanged;
    }

    private void OnDestroy()
    {
        if (inventoryManager != null) inventoryManager.OnActiveItemChanged -= OnActiveItemChanged;
        if (mergeController != null) mergeController.OnMergeModeChanged -= OnMergeModeChanged;
    }

    private void OnActiveItemChanged(ItemData item)
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
        currentActive = item;
    }

    private void OnMergeModeChanged(bool isMerge)
    {
        mergeMode = isMerge;
    }

    private void Update()
    {
        HandleGroundTargeting();
        if (Input.GetMouseButtonDown(0))
        {
            TryCast();
        }
    }

    private void HandleGroundTargeting()
    {
        if (currentActive == null || currentActive.projectileStats == null || !currentActive.projectileStats.isGroundSpell)
        {
            if (currentGhost != null) currentGhost.SetActive(false);
            return;
        }

        ProjectileStats stats = currentActive.projectileStats;

        if (currentGhost == null && stats.ghostIndicatorPrefab != null)
        {
            currentGhost = Instantiate(stats.ghostIndicatorPrefab);
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, stats.maxCastDistance, groundMask))
        {
            validTargetFound = true;
            currentTargetPoint = hit.point;

            Vector3 forward = hit.point - transform.position;

            Vector3 forwardOnGround = Vector3.ProjectOnPlane(forward, hit.normal).normalized;

            if (forwardOnGround != Vector3.zero)
            {
                currentTargetRotation = Quaternion.LookRotation(forwardOnGround, hit.normal);
            }
            else
            {
                currentTargetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            if (currentGhost != null)
            {
                currentGhost.SetActive(true);
                currentGhost.transform.position = hit.point + Vector3.up * 0.1f;
                currentGhost.transform.rotation = currentTargetRotation; // Apply calculation
            }
        }
        else
        {
            validTargetFound = false;
            if (currentGhost != null) currentGhost.SetActive(false);
        }
    }

    private void TryCast()
    {
        if (firePoint == null || currentActive == null || !currentActive.isSpellItem) return;

        ProjectileStats stats = currentActive.projectileStats;
        if (stats == null) return;

        if (stats.isGroundSpell || currentActive.castType == SpellCastType.Ground)
        {
            if (validTargetFound)
            {
                CastGroundSpell(stats, currentTargetPoint, currentTargetRotation);
            }
        }
        else if (stats.isBeamSpell || currentActive.castType == SpellCastType.Beam)
        {
            CastBeamSpell(stats);
        }
        else
        {
            CastProjectileSpell(stats);
        }
    }

    // [UPDATED ARGUMENTS] Added rotation parameter
    private void CastGroundSpell(ProjectileStats stats, Vector3 location, Quaternion rotation)
    {
        if (manaPool != null)
        {
            if (!manaPool.hasMana(stats.manaCost)) return;
            if(!mergeMode) manaPool.useMana(stats.manaCost);
        }

        if (stats.groundSpellPrefab != null)
        {
            // Use the rotation we calculated
            Instantiate(stats.groundSpellPrefab, location, rotation);
        }
    }

    private void CastProjectileSpell(ProjectileStats stats)
    {
        if (manaPool != null)
        {
            if (!manaPool.hasMana(stats.manaCost)) return;
            if(!mergeMode) manaPool.useMana(stats.manaCost);
        }

        GameObject prefabToUse = stats.projectileOverridePrefab != null ? stats.projectileOverridePrefab : projectilePrefab;

        if (prefabToUse != null)
        {
            var go = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);
            if (go.TryGetComponent<ProjectileConfig>(out var cfg))
            {
                cfg.stats = stats;
                cfg.owner = this;
            }
            go.SetActive(true);
        }
    }

    private void CastBeamSpell(ProjectileStats stats)
    {
        if (manaPool != null)
        {
            if (!manaPool.hasMana(stats.manaCost)) return;
            if(!mergeMode) manaPool.useMana(stats.manaCost);
        }

        if (stats.beamPrefab != null && stats.beamConfig != null)
        {
            LightRayBeam beam = Instantiate(stats.beamPrefab);
            beam.Init(firePoint, this, stats.beamConfig);
        }
    }
}