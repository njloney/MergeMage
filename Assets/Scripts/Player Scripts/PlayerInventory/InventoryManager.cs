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


    [Header("Wand Visuals")]
    [SerializeField] private WandVisuals wandVisuals;



    private int activeCrystalSlotIndex = 0;

    private bool mergeMode = false;

    public event Action<bool> OnMergeModeChanged;

    public event Action<ItemData> OnActiveItemChanged;


    void Start()
    {
        UpdateSlotHighlights();
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
        sendActiveCrystaltoWand();
    }
    
    public ItemData getCrystalSlot1()
    {
        return crystalSlot1.currentItem;
    }

    public ItemData getCrystalSlot2()
    {
        return crystalSlot2.currentItem;
    }
    

    private void sendActiveCrystaltoWand()
    {
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
            ToggleMergeMode();
        }
    }

    private void ToggleMergeMode()
    {
        if (crystalSlot1.currentItem == null && crystalSlot2.currentItem == null) return;
        mergeMode = true;
        OnMergeModeChanged?.Invoke(mergeMode);
        calcMergeResult();
        //sendActiveCrystaltoWand();
    }


    private void calcMergeResult()
    {
        ItemData item1 = crystalSlot1.currentItem;
        ItemData item2 = crystalSlot2.currentItem;

       // ItemData result = resolver.Resolve(item1.elementType, item2.elementType);
        //mergeSlot.AddItemToSlot(result);
        
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
            sendActiveCrystaltoWand();
            wandVisuals.updateVisuals();
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
