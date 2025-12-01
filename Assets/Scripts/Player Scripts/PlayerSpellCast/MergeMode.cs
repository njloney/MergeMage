using UnityEngine;
using System;
using System.Collections;

public class MergeMode : MonoBehaviour
{
     private InventoryManager inventory;
    [SerializeField] private SpellCombinationResolver resolver;


    [Header("Player Components")]
     private Health playerHealth;
     private Mana playerMana;
     private RuntimePlayerStats playerStats;

    [Header("Settings")]
    [SerializeField] private float manaRegenRate;      
    [SerializeField] private float baseSelfDamage = 2f;     
    [SerializeField] private float damageRampUp = 2f;
    [SerializeField] private float spellPowerDecay = 0.1f;  
    

    private Coroutine mergeRoutine;

    public event Action<bool> OnMergeModeChanged;

    private bool mergeMode = false;

    private float timeInMode = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = GetComponent<InventoryManager>();
        playerHealth = GetComponent<Health>();
        playerMana = GetComponent<Mana>();
        playerStats = GetComponent<RuntimePlayerStats>();
        manaRegenRate = playerStats.manaRecoveryRate;
        inventory.OnMergeConsumed += exitMergeMode;

    }

    public void ToggleMergeMode()
    {
        mergeMode = !mergeMode;
        OnMergeModeChanged?.Invoke(mergeMode);

        if (mergeMode)
        {

           if (mergeRoutine == null) mergeRoutine = StartCoroutine(MergeModeEffects());
        }
        else
        {
            if (mergeRoutine != null) StopCoroutine(mergeRoutine);
        }
    }
    
    private void exitMergeMode()
    {
        ToggleMergeMode();
        
    }

    private void TryEnterMergeMode()
    {

        ItemData item1 = inventory.getCrystalSlot1();
        ItemData item2 = inventory.getCrystalSlot2();

        if (item1 == null || item2 == null) return;

        ItemData result = resolver.BuildComboSpell(item1, item2);
        if (result != null)
        {
            inventory.addToMergeSlot(result);
            ToggleMergeMode();

        }
    }
    
    private IEnumerator MergeModeEffects()
    {
        timeInMode = 0f;

        while (mergeMode)
        {
            timeInMode += Time.deltaTime;

            if (playerMana != null)
            {
                playerMana.RestoreMana(manaRegenRate * Time.deltaTime);
            }

            if (playerHealth != null)
            {
                float currentDamage = baseSelfDamage + (damageRampUp * timeInMode);
                playerHealth.TakeDamage(currentDamage * Time.deltaTime);
            }

             yield return null;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!mergeMode)
            {
                TryEnterMergeMode();
            }

        }
    }
}
