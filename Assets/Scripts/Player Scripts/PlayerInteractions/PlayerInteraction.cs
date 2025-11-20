using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerInteraction : MonoBehaviour


{

     private InventoryManager inventoryManager;

    [SerializeField] private float interactDistance = 3f;

    [SerializeField] private GameObject pickupHint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryManager = transform.parent.gameObject.GetComponent<InventoryManager>();

        if (pickupHint != null)
        {
            pickupHint.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.TryGetComponent<ItemPickup>(out ItemPickup script))
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
        else
        {
            pickupHint.SetActive(false);
        }
    }
}
