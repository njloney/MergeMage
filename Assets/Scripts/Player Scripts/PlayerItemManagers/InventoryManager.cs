using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Consumable Slot")]
    [SerializeField] private InventorySlot consumableSlot;

    [Header("Slots")]

    [SerializeField] private List<InventorySlot> crystalSlots = new List<InventorySlot>(); 
    [SerializeField] private List<InventorySlot> mergeSlots = new List<InventorySlot>();   

    [Header("Passive Items")]
    [SerializeField] private PassiveItemManager passiveItemManager;

    [SerializeField] private SpellCombinationResolver resolver;
    private MergeMode mergeController;

    private int activeSlotIndex = 0;

    [Header("Passive Items")]
    private bool mergeMode = false;
    private float breakChance = 0.20f;


    public event Action<ItemData> OnActiveItemChanged;
    public event Action MergesConsumed;
    public event Action<ItemData, ItemData> OnCrystalSlotsChanged;


    void Start()
    {
        UpdateSlotHighlights();

         if(mergeController == null)
        {
            mergeController = GetComponent<MergeMode>();
        }
        if(resolver == null)
        {
            resolver = GetComponent<SpellCombinationResolver>();
        }
        // Ensure PassiveItemManager exists
        if (passiveItemManager == null)
        {
            passiveItemManager = GetComponent<PassiveItemManager>();
            if (passiveItemManager == null)
            {
                passiveItemManager = gameObject.AddComponent<PassiveItemManager>();
            }
        }

         mergeController.OnMergeModeChanged += onMerge;
    }
    

    public void onMerge(bool merge)
    {
        mergeMode = merge;

        if (merge)
        {
            sendUpdates();
        }
    }

    public void UpdateSlotHighlights()
    {
        if (mergeMode)
        {
            for(int i = 0; i < mergeSlots.Count; i++)
            {
            mergeSlots[i].setSlotHighLight(i == activeSlotIndex);
            }
        }
        else
        {
            for(int i = 0; i < crystalSlots.Count; i++)
            {
            crystalSlots[i].setSlotHighLight(i == activeSlotIndex);
            }
        }
    }
    private void setActiveSlot(int index)
    {
        if (activeSlotIndex == index) return;

        activeSlotIndex = index;
        UpdateSlotHighlights();
        sendUpdates();
    }

    public ItemData getConsumableSlot()
    {
        return consumableSlot.currentItem;
    }

    private void sendUpdates()
    {
        OnCrystalSlotsChanged?.Invoke(crystalSlots[0].currentItem, crystalSlots[1].currentItem);

        ItemData itemToSend = null;

        if (mergeMode)
        {
            itemToSend = mergeSlots[activeSlotIndex].currentItem;
        }
        else
        {
            itemToSend = crystalSlots[activeSlotIndex].currentItem;
        }

            OnActiveItemChanged?.Invoke(itemToSend);
        
    }

    public void TryConsumeMergeSpell()
    {
        if(!mergeMode) return;

        InventorySlot currentSlot = mergeSlots[activeSlotIndex];

        if (currentSlot.currentItem == null) return;

        //get random value between. 0 and 1
        float roll = UnityEngine.Random.value;

        //if roll is < break chance then we break this UnstableSpell
        bool isBroken = roll < breakChance;

        if (isBroken)
        {
            currentSlot.RemoveItemFromSlot();

            StartCoroutine(AttemptAutoMergeRoutine());


            if (!MergeSpellsLeft())
            {
                MergesConsumed?.Invoke();
            }

        }

        sendUpdates();
    }


    public bool MergeSpellsLeft()
    {
        for(int i = 0; i < mergeSlots.Count; i++)
        {
            if(mergeSlots[i].currentItem != null)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator AttemptAutoMergeRoutine()
    {
        if (crystalSlots[0].currentItem == null || crystalSlots[1].currentItem == null) yield break;

        ItemData item1 = crystalSlots[0].currentItem;
        ItemData item2 = crystalSlots[1].currentItem;

        ItemData result = resolver.BuildComboSpell(item1, item2);

        if (result == null) yield break;

        yield return new WaitForSeconds(1.0f);

        if (crystalSlots[0].currentItem != item1 || crystalSlots[1].currentItem != item2) yield break;

        for(int i = 0; i < mergeSlots.Count; i++)
        {
            if (mergeSlots[i].currentItem == null)
            {
                mergeSlots[i].AddItemToSlot(result);
                crystalSlots[0].RemoveItemFromSlot();
                crystalSlots[1].RemoveItemFromSlot();
                break;
            }
        }

        sendUpdates();
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) setActiveSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) setActiveSlot(1);
    }

    public void addToMergeSlot(ItemData item)
    {
        for(int i = 0; i < crystalSlots.Count; i++)
            {            
            crystalSlots[i].RemoveItemFromSlot();
            }

        for(int i = 0; i < mergeSlots.Count; i++)
        {
            if(mergeSlots[i].currentItem == null)
            {
                mergeSlots[i].AddItemToSlot(item);
                break;
            }
        }
        sendUpdates();
    }

    public bool addItem(ItemData item)
    {
        // Handle passive items differently - they're collected, not equipped
        if (item.itemType == ItemType.Passive)
        {
            if (passiveItemManager != null)
            {
                passiveItemManager.AddPassiveItem(item);
                return true;
            }
            else
            {
                Debug.LogError("PassiveItemManager not found!");
                return false;
            }
        }

        if (item.itemType == ItemType.Consumable)
        {
            consumableSlot.AddItemToSlot(item);
            return true;
        }

        if (item.itemType == ItemType.Crystal)
        {
             ItemData oldItem = null;

            if(crystalSlots[activeSlotIndex].currentItem == null)
            {
                crystalSlots[activeSlotIndex].AddItemToSlot(item);

            }
            else
            {
                int emptySlot = -1;
                for(int i = 0; i < crystalSlots.Count; i++)
                {
                    if(crystalSlots[i].currentItem == null)
                    {
                        emptySlot = i;
                        break;
                    }
                }

                if(emptySlot != -1)
                {
                    crystalSlots[emptySlot].AddItemToSlot(item);
                }
                else
                {
                    oldItem = crystalSlots[activeSlotIndex].currentItem;
                    crystalSlots[activeSlotIndex].AddItemToSlot(item);
                    if (oldItem != null)
                    {
                        dropItem(oldItem);
                    }
                }
            }

            StartCoroutine(AttemptAutoMergeRoutine());
            sendUpdates();
            return true;
        }

        return false;
    }

    private void dropItem(ItemData itemDrop)
    {
        if (itemDrop.pickupPrefab == null)
        {
            Debug.LogError(itemDrop.name + " has no pickup prefab assigned!");
            return;
        }

        Vector3 dropPosition = transform.position + (transform.forward * 1.5f);

        GameObject droppedObject = Instantiate(itemDrop.pickupPrefab, dropPosition, Quaternion.identity);
        droppedObject.transform.localScale = itemDrop.pickupPrefab.transform.localScale;
    }
}