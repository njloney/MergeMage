using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHud : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private Health playerHealth;  //Player's Health
    [SerializeField] private Mana playerMana;  //Player's Mana

    [Header("UI Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHP;
            healthSlider.value = playerHealth.currentHP;

            healthText.text = $"{playerHealth.currentHP} / {playerHealth.maxHP}";

            playerHealth.OnDamaged += HandleDamageTaken;
            playerHealth.OnDied += HandleDeath;
        }

        if (playerMana != null)
        {
            manaSlider.maxValue = playerMana.maxMana;
            manaSlider.value = playerMana.currentMana;

            manaText.text = $"{playerMana.currentMana} / {playerMana.maxMana}";

            playerMana.OnManaChanged += HandleManaChanged;

        }
    }

    private void HandleDamageTaken(float dmgAmount, DamageType type, UnityEngine.Object source)
    {

        healthSlider.value = playerHealth.currentHP;

        healthText.text = $"{playerHealth.currentHP} / {playerHealth.maxHP}";


    }

    private void HandleManaChanged(float amount)
    {
        manaSlider.value = playerMana.currentMana;

        manaText.text = $"{playerMana.currentMana} / {playerMana.maxMana}";

        
    }



    private void HandleDeath()
    {

        Debug.Log("Player DIED! (UI is aware)");
    }
    
    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandleDamageTaken;
            playerHealth.OnDied -= HandleDeath;
        }

        if (playerMana != null)
        {
            playerMana.OnManaChanged -= HandleManaChanged;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
