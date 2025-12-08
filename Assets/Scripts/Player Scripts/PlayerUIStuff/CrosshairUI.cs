using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public static CrosshairUI Instance { get; private set; }

    [Header("Images")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Image hitmarkerImage;

    [Header("Timing")]
    [SerializeField] private float hitmarkerDuration = 0.12f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitmarkerClip;
    [SerializeField] private float hitmarkerVolume = 0.6f;

    float timer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (hitmarkerImage != null)
        {
            Color c = hitmarkerImage.color;
            c.a = 0f;
            hitmarkerImage.color = c;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (hitmarkerImage == null || timer <= 0f)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Color c = hitmarkerImage.color;
            c.a = 0f;
            hitmarkerImage.color = c;
        }
    }

    public void ShowHitmarker()
    {
        if (hitmarkerImage == null)
            return;

        timer = hitmarkerDuration;

        Color c = hitmarkerImage.color;
        c.a = 1f;
        hitmarkerImage.color = c;

        if (hitmarkerClip != null && audioSource != null)
            audioSource.PlayOneShot(hitmarkerClip, hitmarkerVolume);
    }
}
