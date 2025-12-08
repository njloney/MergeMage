using UnityEngine;
using UnityEngine.UI;

public class ScreenDamageFlashUI : MonoBehaviour
{
    public static ScreenDamageFlashUI Instance { get; private set; }

    [SerializeField] private Image overlayImage;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float maxAlpha = 0.5f;

    float timer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
        }
    }

    void Update()
    {
        if (overlayImage == null || timer <= 0f)
            return;

        timer -= Time.deltaTime;
        float t = Mathf.Clamp01(timer / flashDuration);

        Color c = overlayImage.color;
        c.a = t * maxAlpha;
        overlayImage.color = c;
    }

    public void Flash()
    {
        if (overlayImage == null)
            return;

        timer = flashDuration;

        Color c = overlayImage.color;
        c.a = maxAlpha;
        overlayImage.color = c;
    }
}
