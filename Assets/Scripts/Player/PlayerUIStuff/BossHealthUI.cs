using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Health bossHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image borderImage;
    [SerializeField] private BossShieldController shieldController;

    [SerializeField] private Color shieldedBorderColor = new Color(0.6f, 0f, 1f);
    [SerializeField] private float shieldAlpha = 0.5f;

    void Start()
    {
        StartCoroutine(DelayedUIKick());
    }

    System.Collections.IEnumerator DelayedUIKick()
    {
        yield return null;
        yield return null;

        if (bossHealth == null)
            yield break;

        if (bossHealth.currentHP > 1f)
        {
            bossHealth.TakeDamage(1f, DamageType.Physical, this);
            bossHealth.Heal(1f);
        }
    }

    void Awake()
    {
        if (shieldController == null && bossHealth != null)
            shieldController = bossHealth.GetComponent<BossShieldController>();

        if (bossHealth == null && shieldController != null)
            bossHealth = shieldController.GetComponent<Health>();
    }

    void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDamaged += OnBossDamaged;

        if (shieldController != null)
            shieldController.OnShieldStateChanged += OnShieldStateChanged;

        if (borderImage != null)
        {
            Color c = borderImage.color;
            c.a = 0f;
            borderImage.color = c;
        }

        RefreshHealth();

        if (shieldController != null)
            OnShieldStateChanged(shieldController.IsShieldActive);
    }

    void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDamaged -= OnBossDamaged;

        if (shieldController != null)
            shieldController.OnShieldStateChanged -= OnShieldStateChanged;
    }

    void OnBossDamaged(float amount, DamageType type, UnityEngine.Object source)
    {
        RefreshHealth();
    }

    void RefreshHealth()
    {
        if (bossHealth == null || healthSlider == null)
            return;

        healthSlider.maxValue = bossHealth.maxHP;
        healthSlider.value = bossHealth.currentHP;
    }

    void OnShieldStateChanged(bool active)
    {
        if (borderImage == null)
            return;

        Color c = borderImage.color;

        if (active)
        {
            c = shieldedBorderColor;
            c.a = shieldAlpha;
        }
        else
        {
            c.a = 0f;
        }

        borderImage.color = c;
    }
}
