using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerInteraction : MonoBehaviour


{

    private InventoryManager inventoryManager;

    [SerializeField] private float interactDistance = 3f;

    [SerializeField] private TextMeshProUGUI pickupHintText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool mergeMode;
    void Start()
    {
        inventoryManager = transform.parent.gameObject.GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out RaycastHit hit, interactDistance) && hit.collider.TryGetComponent<ItemPickup>(out ItemPickup script))
        {
            pickupHintText.text = "[E] to pick up " + "\n" + script.itemToGive.itemName;
            pickupHintText.gameObject.SetActive(true);
            Debug.Log("Looking at: " + script.itemToGive.itemName);

            if (Input.GetKeyDown(KeyCode.E))
            {
                bool success = inventoryManager.addItem(script.itemToGive);
                if (success)
                {
                    Destroy(hit.collider.gameObject);
                    pickupHintText.gameObject.SetActive(false);
                }
            }
            
        }
        else
        {
            pickupHintText.gameObject.SetActive(false);
        }
    }
}
