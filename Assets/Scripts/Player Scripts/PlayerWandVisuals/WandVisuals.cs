using UnityEngine;

public class WandVisuals : MonoBehaviour
{
    
    [SerializeField] private InventoryManager inventoryManager;
    
    [Header("Sockets")]
    [SerializeField] private Transform socketA;
    [SerializeField] private Transform socketB;

    [SerializeField] private Transform crsytalRotater;

    [Header("Visual Effects")]
    [SerializeField] private float crystalRotationSpeed = 50f;

    private GameObject modelA;
    private GameObject modelB;

    public void Start()
    {
        inventoryManager.OnCrystalSlotsChanged += updateVisuals;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void updateVisuals(ItemData crystal1, ItemData crystal2)
    {
        Debug.Log("updating visuals");
        if (inventoryManager != null)
        {
            
            updateSocket(socketA, crystal1, ref modelA);
            updateSocket(socketB, crystal2, ref modelB);

        }

    }

    public void updateSocket(Transform socket, ItemData item, ref GameObject currentModel)
    {
        Destroy(currentModel);
        currentModel = null;

        if (item != null && item.pickupPrefab != null)
        {
            currentModel = Instantiate(item.pickupPrefab, socket);

            var col = currentModel.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var script = currentModel.GetComponent<ItemPickup>();
            if (script != null) Destroy(script);

            Component animScript = currentModel.GetComponent("SimpleGemsAnim");
            if (animScript != null) Destroy(animScript);

            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }

    }

    public void Update()
    {
        //Rotate the model around the wand

        if (crsytalRotater != null)
        {
            crsytalRotater.Rotate(Vector3.up * crystalRotationSpeed * Time.deltaTime);
        }
    }
}
