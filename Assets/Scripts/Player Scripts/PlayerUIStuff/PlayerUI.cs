using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private Health playerHealth;  //Player's Health
    [SerializeField] private Mana playerMana;  //Player's Mana

    [SerializeField] private RuntimePlayerStats playerStats;

    [Header("UI Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    [Header("Passive Items UI")]

    [SerializeField] private PassiveItemManager itemManager;
    [SerializeField] private Transform passiveItemsPanel;   
    [SerializeField] private GameObject passiveIconPrefab;

    private Dictionary<ItemData, PassiveItemSlot> passiveSlots = new Dictionary<ItemData, PassiveItemSlot>();


    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerStats != null && playerHealth != null && playerMana != null && itemManager != null)
        {
            //Update at the Start
            updateHealthUI();
            updateManaUI();

            //Connect when damaged
            playerHealth.OnDamaged += HandleDamage;

            //Connect when mana used
            playerMana.OnManaChanged += HandleManaChange;

            //Connect when maxHealth Changes
            playerStats.OnMaxHealthChanged += HandleStatsChange;

            //Connect when maxMana Changes
            playerStats.OnMaxManaChanged += HandleStatsChange;

            //Connect when Death
            playerHealth.OnDied += HandleDeath;
            
            //Connecting to ItemManager when Passive Items are acquired and Stacked
            itemManager.OnPassiveItemAcquired += HandleItemAcquired;
            itemManager.OnPassiveItemStacked += HandleItemStacked;

        }
        
    }

    private void HandleItemAcquired(ItemData item, int count)
    {
        GameObject passiveItemIcon = Instantiate(passiveIconPrefab, passiveItemsPanel);
        PassiveItemSlot passiveSlot = passiveItemIcon.GetComponent<PassiveItemSlot>();

        if(passiveSlot != null)
        {
            passiveSlot.setSlot(item, count);
            passiveSlots.Add(item, passiveSlot);

        }
        
    }

     private void HandleItemStacked(ItemData item, int count)
    {
        if(passiveSlots.ContainsKey(item))
        {
            passiveSlots[item].updateStackCount(count);
        }
        
    }

    private void HandleDamage(float amount, DamageType type, UnityEngine.Object source)
    {
        updateHealthUI();
    }


    private void HandleManaChange(float amount)
    {

        updateManaUI();

    }


    private void HandleStatsChange(float amount)
    {

        updateHealthUI();
        updateManaUI();

    }

    private void updateHealthUI()
    {
        if (playerHealth == null || playerStats == null) return;
        healthSlider.maxValue = playerStats.maxHealth;
        healthSlider.value = playerHealth.currentHP;
        healthText.text = $"{playerHealth.currentHP:0} / {playerStats.maxHealth:0}";
    }

    private void updateManaUI()
    {
        if (playerMana == null || playerStats == null) return;
        manaSlider.maxValue = playerStats.maxMana;
        manaSlider.value = playerMana.currentMana;
        manaText.text = $"{playerMana.currentMana:0} / {playerStats.maxMana}";
    }



    private void HandleDeath()
    {

        Debug.Log("Player DIED! (UI is aware)");
    }
    
    private void OnDisable()
    {
        if (playerHealth != null && playerMana != null && itemManager != null && playerStats != null)
        {
            playerHealth.OnDamaged -= HandleDamage;
            playerHealth.OnDied -= HandleDeath;
            playerMana.OnManaChanged -= HandleManaChange;
            playerStats.OnMaxHealthChanged -= HandleStatsChange;
            playerStats.OnMaxManaChanged -= HandleStatsChange;
            itemManager.OnPassiveItemAcquired -= HandleItemAcquired;
            itemManager.OnPassiveItemStacked -= HandleItemStacked;

        }

    }
}
