using UnityEngine;

public class PassiveItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("The passive item this pickup represents")]
    public ItemData itemData;

    [Header("Pickup Settings")]
    [Tooltip("If true, automatically pick up on touch. If false, player must press E")]
    public bool autoPickup = true;

    [Tooltip("Key to press for manual pickup (if autoPickup is false)")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("Visual Feedback")]
    [Tooltip("Show 'Press E to pick up' message")]
    public bool showPickupPrompt = true;

    [Tooltip("Optional: UI text to show pickup prompt (assign from scene)")]
    public GameObject pickupPromptUI;

    [Header("Effects")]
    [Tooltip("Optional: Particle effect when picked up")]
    public GameObject pickupEffect;

    [Tooltip("Optional: Sound effect when picked up")]
    public AudioClip pickupSound;

    [Header("Rotation (Optional)")]
    [Tooltip("Make the item rotate slowly")]
    public bool rotateItem = true;

    [Tooltip("Rotation speed")]
    public float rotationSpeed = 50f;

    private bool playerInRange = false;
    private GameObject playerInRangeObject = null;
    private AudioSource audioSource;

    private void Start()
    {
        // Validate that this is actually a passive item
        if (itemData != null && itemData.itemType != ItemType.Passive)
        {
            Debug.LogWarning($"PassiveItemPickup on {gameObject.name} has a non-passive ItemData assigned! Item type is {itemData.itemType}");
        }

        // Get or add AudioSource for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Hide prompt at start
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }

        // Validate item data
        if (itemData == null)
        {
            Debug.LogError($"PassiveItemPickup on {gameObject.name} has no ItemData assigned!", this);
        }
    }

    private void Update()
    {
        // Rotate the item if enabled
        if (rotateItem)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Manual pickup check
        if (!autoPickup && playerInRange && Input.GetKeyDown(pickupKey))
        {
            AttemptPickup(playerInRangeObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInRangeObject = other.gameObject;

            if (autoPickup)
            {
                // Automatic pickup
                AttemptPickup(other.gameObject);
            }
            else
            {
                // Show prompt for manual pickup
                ShowPickupPrompt(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInRangeObject = null;
            ShowPickupPrompt(false);
        }
    }

    private void AttemptPickup(GameObject player)
    {
        if (itemData == null)
        {
            Debug.LogError("Cannot pick up item - no ItemData assigned!");
            return;
        }

        InventoryManager inventory = player.GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("Player does not have an InventoryManager component!");
            return;
        }

        // Try to add item to inventory
        bool success = inventory.addItem(itemData);

        if (success)
        {
            OnPickupSuccess(player);
        }
        else
        {
            Debug.Log($"Could not pick up {itemData.itemName}");
        }
    }

    private void OnPickupSuccess(GameObject player)
    {
        ShowPickupPrompt(false);

        Vector3 pos = transform.position;

        Destroy(gameObject);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, pos);
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, pos, Quaternion.identity);
        }

        Debug.Log($"Picked up passive item: {itemData.itemName}");
    }

    private void ShowPickupPrompt(bool show)
    {
        if (!showPickupPrompt) return;

        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(show);
        }
        else if (show)
        {
            Debug.Log($"Press {pickupKey} to pick up {itemData.itemName}");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a wire sphere to show trigger radius
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.84f, 0f, 0.5f); // Gold color

            if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.position, sphereCol.radius * transform.localScale.x);
            }
            else if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
        }
    }
}