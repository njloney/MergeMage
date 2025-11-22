using UnityEngine;

public class WandVisuals : MonoBehaviour
{
    
    [SerializeField] private InventoryManager inventoryManager;
    
    [Header("Sockets")]
    [SerializeField] private Transform socketA;
    [SerializeField] private Transform socketB;

    private GameObject modelA;
    private GameObject modelB;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void updateVisuals()
    {
        if (inventoryManager != null)
        {
            ItemData crystal1 = inventoryManager.getCrystalSlot1();
            ItemData crystal2 = inventoryManager.getCrystalSlot2();

            updateSocket(socketA, crystal1, ref modelA);
            updateSocket(socketB, crystal2, ref modelB);

        }

    }

    public void updateSocket(Transform socket, ItemData item, ref GameObject currentModel)
    {
        Destroy(currentModel);
        currentModel = null;
        
        if (item != null && item.wandModelPrefab != null)
        {
            currentModel = Instantiate(item.wandModelPrefab, socket);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }
        
    }
}
