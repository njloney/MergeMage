using UnityEngine;
using System;
using System.Collections;

public class ConsumableManager : MonoBehaviour
{
     private InventoryManager inventory;
     private Health playerHealth;
     private Mana playerMana;

    private float cooldowntime = 0f;

    public event Action<float> CooldownUpdate;

    private bool cooldown = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = GetComponent<InventoryManager>();
        playerHealth = GetComponent<Health>();
        playerMana = GetComponent<Mana>();
        if(inventory != null)
        {
            Debug.Log("Is not null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !cooldown)
        {
            Debug.Log("Q pressed");
            TryUseConsumable();
        }
    }


    private void TryUseConsumable()
    {
        ItemData item = inventory.getConsumableSlot();
        if (item == null) return;
        if (item.consumableEffect == null) return;
        cooldowntime = item.consumableEffect.cooldown;
        if (ApplyEffect(item)){
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        cooldown = true;
        while (cooldowntime > 0)
        {
            cooldowntime -= Time.deltaTime;
            CooldownUpdate?.Invoke(cooldowntime);
            yield return null;
        }
        CooldownUpdate?.Invoke(0);
        cooldown = false;
    }

    private bool ApplyEffect(ItemData item)
    {
        switch (item.consumableEffect.type)
        {
            case ConsumableType.restoreHealth:
                playerHealth.Heal(item.consumableEffect.amount);
                break;
            case ConsumableType.restoreMana:
                playerMana.RestoreMana(item.consumableEffect.amount);
                break;
            case ConsumableType.restoreMaxHealth:
                playerHealth.Heal(playerHealth.maxHP);
                break;
            case ConsumableType.restoreMaxMana:
                playerMana.RestoreMana(playerMana.maxMana);
                break;
            default:
                return false;
        
        }
        
        return true; 
    }


}
