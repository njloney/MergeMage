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
        if(itemManager != null)
        {
            itemManager.OnPassiveItemAcquired += HandleItemAcquired;
            itemManager.OnPassiveItemStacked += HandleItemStacked;
        
        }


        if (playerHealth != null)
        {
            Debug.Log(playerHealth.maxHP);
            healthSlider.maxValue = playerHealth.maxHP;
            healthSlider.value = playerHealth.currentHP;

            healthText.text = $"{playerHealth.currentHP:0} / {playerHealth.maxHP}";

            playerHealth.OnDamaged += HandleDamageTaken;
            playerHealth.OnDied += HandleDeath;
        }

        if (playerMana != null)
        {
            manaSlider.maxValue = playerMana.maxMana;
            manaSlider.value = playerMana.currentMana;

            manaText.text = $"{playerMana.currentMana:0} / {playerMana.maxMana}";

            playerMana.OnManaChanged += HandleManaChanged;

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


    private void HandleDamageTaken(float dmgAmount, DamageType type, UnityEngine.Object source)
    {

        healthSlider.value = playerHealth.currentHP;

        healthText.text = $"{playerHealth.currentHP:0} / {playerHealth.maxHP}";


    }

    private void HandleManaChanged(float amount)
    {
        manaSlider.value = playerMana.currentMana;

        manaText.text = $"{playerMana.currentMana:0} / {playerMana.maxMana}";

        
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
