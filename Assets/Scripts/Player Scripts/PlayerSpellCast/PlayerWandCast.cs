using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [SerializeField] private Transform firePoint;

    [SerializeField] private InventoryManager inventoryManager;

    // The Wand stores the "Ammo" locally
    private ItemData currentActive;

    private bool mergeMode;

    void Start()
    {

        inventoryManager.OnActiveItemChanged += getAmmo;
        inventoryManager.OnMergeModeChanged += checkMergeMode;

    }

    void checkMergeMode(bool isMerge)
    {
        mergeMode = isMerge;
    }
    
    void getAmmo(ItemData item)
    {
        currentActive = item;

        if(currentActive == null)
        {
            Debug.Log("current active is null");
        }
        else
        {
            Debug.Log("current active is not null");
        }
    }


    private void TryCast()
    {
        if(currentActive != null)
        {
            var go = Instantiate(currentActive.projectilePrefab, firePoint.position, firePoint.rotation);

            if (go.TryGetComponent<ProjectileConfig>(out var cfg))
            {
                cfg.owner = this;
            }

            go.SetActive(true);

            if (mergeMode)
            {
                inventoryManager.ToggleMergeMode();

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            TryCast();
        }
        
    }
}
