using UnityEngine;

[RequireComponent(typeof(ProjectileConfig))]
public class BlinkSpell : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Offset from the ground to prevent spawning inside the floor.")]
    [SerializeField] private Vector3 verticalOffset = new Vector3(0, 1f, 0);

    [Header("Visual Effects")]
    [Tooltip("Effect to spawn at the player's OLD position.")]
    [SerializeField] private GameObject departVFX;

    [Tooltip("Effect to spawn at the player's NEW position.")]
    [SerializeField] private GameObject arriveVFX;

    [Header("Sound")]
    [SerializeField] private AudioClip blinkSound;

    private void Start()
    {
        var config = GetComponent<ProjectileConfig>();
        if (config == null || config.owner == null)
        {
            Debug.LogWarning("[BlinkSpell] No owner found in ProjectileConfig. Cannot teleport.");
            Destroy(gameObject);
            return;
        }

        MonoBehaviour ownerScript = config.owner as MonoBehaviour;
        FirstPersonController playerController = ownerScript.GetComponentInParent<FirstPersonController>();

        if (playerController != null)
        {
            PerformTeleport(playerController);
        }
        else
        {
            Debug.LogWarning("[BlinkSpell] Owner does not have a FirstPersonController.");
        }

        Destroy(gameObject);
    }

    private void PerformTeleport(FirstPersonController player)
    {
        if (departVFX != null)
        {
            Instantiate(departVFX, player.transform.position, Quaternion.identity);
        }

        if (player.controller != null)
        {
            player.controller.enabled = false;
        }

        Vector3 targetPosition = transform.position + verticalOffset;
        player.transform.position = targetPosition;

        if (player.controller != null)
        {
            player.controller.enabled = true;
        }

        if (arriveVFX != null)
        {
            Instantiate(arriveVFX, targetPosition, Quaternion.identity);
        }

        if (blinkSound != null)
        {
            AudioSource.PlayClipAtPoint(blinkSound, targetPosition);
        }
    }
}