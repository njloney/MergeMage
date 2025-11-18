using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Consumable Slot")]

    [SerializeField] private InventorySlot consumableSlot;

    [Header("Crystal Hot-Swap Slots")]
    [SerializeField] private InventorySlot crystalSlot1;
    [SerializeField] private InventorySlot crystalSlot2;

    private int activeCrystalSlotIndex = 0;

    void Start()
    {
        UpdateSlotHighlights();
    }

    private void UpdateSlotHighlights()
    {
        crystalSlot1.setSlotHighLight(activeCrystalSlotIndex == 0);
        crystalSlot2.setSlotHighLight(activeCrystalSlotIndex == 1);
     
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeCrystalSlotIndex = 0; //Set Active Slot to 1
            UpdateSlotHighlights();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activeCrystalSlotIndex = 1; //Set Active Slot to 2
            UpdateSlotHighlights();
        }
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

                return true;
            }
            else
            {
                oldItem = crystalSlot2.currentItem;
                crystalSlot2.AddItemToSlot(item);
                if (oldItem != null)
                {
                    dropItem(oldItem);
                }

                return true;
            }

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
