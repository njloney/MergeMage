using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class GeyserLauncher : MonoBehaviour
{
    [Header("Lift Settings")]
    [Tooltip("Upward acceleration per second.")]
    [SerializeField] private float liftForce = 20f;
    [SerializeField] private float lifetime = 5f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem steamParticles;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        GetComponent<Collider>().isTrigger = true;
    }

    // Continuous lift while inside
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<FirstPersonController>();
            if (player != null)
            {
                player.SetGeyserState(true, liftForce);
            }
        }
    }

    // Stop lifting when they leave the volume
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<FirstPersonController>();
            if (player != null)
            {
                player.SetGeyserState(false, 0f);
            }
        }
    }

    private void OnDestroy()
    {
    }
}