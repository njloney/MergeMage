using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathVisuals : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("Particle System prefab to spawn on death.")]
    [SerializeField] private GameObject deathVFX;

    [Tooltip("Offset from the enemy's pivot.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float volume = 1f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied += PlayEffects;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied -= PlayEffects;
    }

    private void PlayEffects()
    {
        if (deathVFX != null)
        {
            Instantiate(deathVFX, transform.position + spawnOffset, Quaternion.identity);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, volume);
        }
    }
}