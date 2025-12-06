using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Consumable Slot")]
    [SerializeField] private InventorySlot consumableSlot;

    [Header("Crystal Slots")]
    [SerializeField] private InventorySlot crystalSlot1;
    [SerializeField] private InventorySlot crystalSlot2;

    [Header("Merge Slot")]
    [SerializeField] private InventorySlot mergeSlot;

    [Header("Passive Items")]
    [SerializeField] private PassiveItemManager passiveItemManager;

    private MergeMode mergeController;

    private int activeCrystalSlotIndex = 0;
    private bool mergeMode = false;

    public event Action<ItemData> OnActiveItemChanged;
    public event Action OnMergeConsumed;
    public event Action<ItemData, ItemData> OnCrystalSlotsChanged;


    void Start()
    {
        UpdateSlotHighlights();

         if(mergeController == null)
        {
            mergeController = GetComponent<MergeMode>();
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

    private void UpdateSlotHighlights()
    {
        crystalSlot1.setSlotHighLight(activeCrystalSlotIndex == 1);
        crystalSlot2.setSlotHighLight(activeCrystalSlotIndex == 0);
    }

    private void setActiveSlot(int index)
    {
        if (activeCrystalSlotIndex == index) return;

        activeCrystalSlotIndex = index;
        UpdateSlotHighlights();
        sendUpdates();
    }

    public ItemData getCrystalSlot1()
    {
        return crystalSlot1.currentItem;
    }

    public ItemData getCrystalSlot2()
    {
        return crystalSlot2.currentItem;
    }

    private void sendUpdates()
    {
        OnCrystalSlotsChanged?.Invoke(crystalSlot1.currentItem, crystalSlot2.currentItem);

        ItemData itemToSend = null;
        Debug.Log("Merge Mode Active " + mergeMode);
        if (mergeMode)
        {
            itemToSend = mergeSlot.currentItem;
            if(mergeSlot.currentItem == null)
            {
                Debug.Log("Merge is not null??????");
            }
        }
        else
        {
            if (activeCrystalSlotIndex == 0)
            {
                itemToSend = crystalSlot1.currentItem;
            }
            else
            {
                itemToSend = crystalSlot2.currentItem;
            }
        }
            OnActiveItemChanged?.Invoke(itemToSend);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            setActiveSlot(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            setActiveSlot(1);
        }

    }

    public void ConsumeMergeSpell()
    {
        if (mergeSlot != null)
        {
            mergeSlot.RemoveItemFromSlot();
        }

        OnMergeConsumed?.Invoke();
        sendUpdates();
    }

    public void addToMergeSlot(ItemData item)
    {
        mergeSlot.AddItemToSlot(item);
        crystalSlot1.RemoveItemFromSlot();
        crystalSlot2.RemoveItemFromSlot();
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

        ItemData oldItem = null;

        if (item.itemType == ItemType.Crystal)
        {
            // Check if active slot is full and other slot is empty
            if (activeCrystalSlotIndex == 0)
            {
                // Active slot is crystalSlot1
                if (crystalSlot1.currentItem != null && crystalSlot2.currentItem == null)
                {
                    // Slot 1 is full, slot 2 is empty - add to slot 2
                    crystalSlot2.AddItemToSlot(item);
                }
                else
                {
                    // Normal behavior - add to active slot (slot 1)
                    oldItem = crystalSlot1.currentItem;
                    crystalSlot1.AddItemToSlot(item);

                    if (oldItem != null)
                    {
                        dropItem(oldItem);
                    }
                }
            }
            else
            {
                // Active slot is crystalSlot2
                if (crystalSlot2.currentItem != null && crystalSlot1.currentItem == null)
                {
                    // Slot 2 is full, slot 1 is empty - add to slot 1
                    crystalSlot1.AddItemToSlot(item);
                }
                else
                {
                    // Normal behavior - add to active slot (slot 2)
                    oldItem = crystalSlot2.currentItem;
                    crystalSlot2.AddItemToSlot(item);

                    if (oldItem != null)
                    {
                        dropItem(oldItem);
                    }
                }
            }

            sendUpdates();
            return true;
        }
        else if (item.itemType == ItemType.Consumable)
        {
            oldItem = consumableSlot.currentItem;
            consumableSlot.AddItemToSlot(item);
            if (oldItem != null)
            {
                dropItem(oldItem);
            }

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