using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Consumable Slot")]

    [SerializeField] private InventorySlot consumableSlot;

    [Header("Crystal Slots")]
    [SerializeField] private InventorySlot crystalSlot1;
    [SerializeField] private InventorySlot crystalSlot2;

    [Header("Merge Slot")]

    [SerializeField] private InventorySlot mergeSlot;

    [Header("Spell Combination Resolver")]
    [SerializeField] private SpellCombinationResolver resolver;


    private int activeCrystalSlotIndex = 0;

    private bool mergeMode = false;

    public event Action<ItemData> OnActiveItemChanged;

    public event Action<ItemData, ItemData> OnCrystalSlotsChanged;

    public event Action<bool> OnMergeModeChanged;



    void Start()
    {
        UpdateSlotHighlights();
        OnMergeModeChanged?.Invoke(mergeMode);
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

        if (mergeMode)
        {
            itemToSend = mergeSlot.currentItem;

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

        if (itemToSend != null)
        {
            OnActiveItemChanged?.Invoke(itemToSend);
        }
        
        
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M key pressed");
            EnterMergeMode();
        }
    }

    private void EnterMergeMode()
    {
        if (crystalSlot1.currentItem == null && crystalSlot2.currentItem == null) return;
        initiateMerge();
        sendUpdates();

    }


    public void ToggleMergeMode()
    {
        mergeMode = !mergeMode;
        OnMergeModeChanged?.Invoke(mergeMode);

    }


    private void initiateMerge()
    {
        ItemData item1 = crystalSlot1.currentItem;
        ItemData item2 = crystalSlot2.currentItem;
        ItemData result = resolver.BuildComboSpell(item1, item2);
        if(result != null)
        {
            ToggleMergeMode();
            mergeSlot.AddItemToSlot(result);
            crystalSlot1.RemoveItemFromSlot();
            crystalSlot2.RemoveItemFromSlot();
        }
    }

    public void ConsumeMergeSpell()
    {
        // Clear the merge slot
        if (mergeSlot != null)
        {
            mergeSlot.RemoveItemFromSlot();
        }

        // Ensure merge mode is OFF
        if (mergeMode)
        {
            mergeMode = false;
            OnMergeModeChanged?.Invoke(mergeMode);
        }

        // Re-send updates so wand & UI know there's no longer an active unstable item
        sendUpdates();
    }

    public ItemData GetMergeItem()
    {
            if (mergeSlot != null)
            {
                return mergeSlot.currentItem;
        }

            return null;
    }


    public bool addItem(ItemData item)
    {
        ItemData oldItem = null;

        if (item.itemType == ItemType.Crystal)
        {
            if (activeCrystalSlotIndex == 0)
            {
                oldItem = crystalSlot1.currentItem;
                crystalSlot1.AddItemToSlot(item);


                if (oldItem != null)
                {
                    dropItem(oldItem);
                }

            }
            else
            {
                oldItem = crystalSlot2.currentItem;
                crystalSlot2.AddItemToSlot(item);
                if (oldItem != null)
                {
                    dropItem(oldItem);
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

        // Spawn the item's specific prefab in front of the player
        Vector3 dropPosition = transform.position + (transform.forward * 1.5f);
        Instantiate(itemDrop.pickupPrefab, dropPosition, Quaternion.identity);
    }


}
