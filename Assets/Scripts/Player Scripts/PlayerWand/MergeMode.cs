using UnityEngine;
using System;
using System.Collections;

public class MergeMode : MonoBehaviour
{
     private InventoryManager inventory;


    [Header("Player Components")]
     private Health playerHealth;
     private Mana playerMana;
     private RuntimePlayerStats playerStats;

    [Header("Settings")]
    [SerializeField] private float manaRegenRate;      
    
    private Coroutine mergeRoutine;

    public event Action<bool> OnMergeModeChanged;

    private bool mergeMode = false;

    private float timeInMode = 0f;
    void Start()
    {
        inventory = GetComponent<InventoryManager>();
        playerHealth = GetComponent<Health>();
        playerMana = GetComponent<Mana>();
        playerStats = GetComponent<RuntimePlayerStats>();
        manaRegenRate = playerStats.manaRecoveryRate;
        inventory.MergesConsumed += exitMergeMode;

    }

    public void ToggleMergeMode()
    {
        mergeMode = !mergeMode;
        OnMergeModeChanged?.Invoke(mergeMode);

        if (mergeMode)
        {

           if (mergeRoutine == null) mergeRoutine = StartCoroutine(MergeModeRoutine());
        }
        else
        {
            if (mergeRoutine != null) StopCoroutine(mergeRoutine);
        }

        inventory.UpdateSlotHighlights();
    }
    
    private void exitMergeMode()
    {
        ToggleMergeMode();
        
    }

    private void TryEnterMergeMode()
    {
        ToggleMergeMode();

    }
    
    private IEnumerator MergeModeRoutine()
    {
        timeInMode = 0f;

        while (mergeMode)
        {
            timeInMode += Time.deltaTime;

            if (playerMana != null)
            {
                playerMana.RestoreMana(manaRegenRate * Time.deltaTime);
            }

            if(Input.GetMouseButtonDown(0))
            {
                inventory.TryConsumeMergeSpell();
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
