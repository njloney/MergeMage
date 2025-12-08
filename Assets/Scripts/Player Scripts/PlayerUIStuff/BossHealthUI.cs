using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Health bossHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image borderImage;
    [SerializeField] private BossShieldController shieldController;

    [SerializeField] private Color normalBorderColor = Color.white;
    [SerializeField] private Color shieldedBorderColor = new Color(0.6f, 0f, 1f);

    void Awake()
    {
        if (bossHealth == null)
        {
            var shield = FindObjectOfType<BossShieldController>();
            if (shield != null)
            {
                shieldController = shield;
                bossHealth = shield.GetComponent<Health>();
            }
        }

        if (shieldController == null && bossHealth != null)
            shieldController = bossHealth.GetComponent<BossShieldController>();
    }

    void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDamaged += OnBossDamaged;

        if (shieldController != null)
            shieldController.OnShieldStateChanged += OnShieldStateChanged;

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

    void OnBossDamaged(float amount, DamageType type, Object source)
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

        Color c = active ? shieldedBorderColor : normalBorderColor;

        // Clamp alpha so it never exceeds 0.5
        c.a = Mathf.Min(c.a, 0.5f);

        borderImage.color = c;
    }

}
