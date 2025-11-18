using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrystalPickup : MonoBehaviour
{
    [SerializeField]
    private CrystalType crystalType;

    [SerializeField]
    private GameObject pickupEffect; // for visual effect later

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            // Try to find the PlayerWand script on the player
            PlayerWand wand = other.GetComponentInChildren<PlayerWand>();
            if (wand == null)
            {
                wand = other.GetComponent<PlayerWand>();
            }

            if (wand != null)
            {
                // Send the crystal type to the wand
                wand.Collect(crystalType);

                // Play effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // Destroy the crystal pickup
                Destroy(gameObject);
            }
        }
    }
}