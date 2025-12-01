using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerInteraction : MonoBehaviour


{

    private InventoryManager inventoryManager;
     
    private MergeMode mergeController;

    [SerializeField] private float interactDistance = 3f;

    [SerializeField] private GameObject pickupHint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool mergeMode;
    void Start()
    {
        inventoryManager = transform.parent.gameObject.GetComponent<InventoryManager>();
        mergeController = transform.parent.gameObject.GetComponent<MergeMode>();
        mergeController.OnMergeModeChanged += checkMergeMode;


        if (pickupHint != null)
        {
            pickupHint.SetActive(false);
        }
    }

    void checkMergeMode(bool isMerge)
    {
        mergeMode = isMerge;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (!mergeMode && Physics.Raycast(transform.position, fwd, out RaycastHit hit, interactDistance) && hit.collider.TryGetComponent<ItemPickup>(out ItemPickup script))
        {
            pickupHint.SetActive(true);
            Debug.Log("Looking at: " + script.itemToGive.itemName);

            if (Input.GetKeyDown(KeyCode.E))
            {
                bool success = inventoryManager.addItem(script.itemToGive);
                if (success)
                {
                    Destroy(hit.collider.gameObject);
                    pickupHint.SetActive(false);
                }
            }
            
        }
        else
        {
            pickupHint.SetActive(false);
        }
    }
}
