using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public static CrosshairUI Instance { get; private set; }

    [SerializeField] private Image crosshairImage;
    [SerializeField] private Image hitmarkerImage;
    [SerializeField] private float hitmarkerDuration = 0.12f;

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
            var c = hitmarkerImage.color;
            c.a = 0f;
            hitmarkerImage.color = c;
        }
    }

    void Update()
    {
        if (hitmarkerImage == null || timer <= 0f)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            var c = hitmarkerImage.color;
            c.a = 0f;
            hitmarkerImage.color = c;
        }
    }

    public void ShowHitmarker()
    {
        if (hitmarkerImage == null)
            return;

        timer = hitmarkerDuration;

        var c = hitmarkerImage.color;
        c.a = 1f;
        hitmarkerImage.color = c;
    }
}
