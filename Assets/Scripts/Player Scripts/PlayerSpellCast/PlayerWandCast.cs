using UnityEngine;

public class PlayerWandCast : MonoBehaviour
{
    [SerializeField] private Transform firePoint;

    [SerializeField] private InventoryManager inventoryManager;

    // The Wand stores the "Ammo" locally
    private ItemData currentCrystal;
    void Start()
    {

        inventoryManager.OnActiveItemChanged += getAmmo;

    }
    
    void getAmmo(ItemData item)
    {
        currentCrystal = item;

        if(currentCrystal == null)
        {
            Debug.Log("current crystal is null");
        }
        else
        {
            Debug.Log("current crystal is not null");
        }
    }


    private void TryCast()
    {
        if(currentCrystal != null)
        {
            var go = Instantiate(currentCrystal.projectilePrefab, firePoint.position, firePoint.rotation);

            if (go.TryGetComponent<ProjectileConfig>(out var cfg))
            {
                cfg.owner = this;
            }
        
         go.SetActive(true);
            
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
