using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Slot Type")]
    public ItemType allowedSlotType;


    [Header("Slot State")]

    public ItemData currentItem;


    [Header("Slot Showing")]

    public Image itemIcon;

    public GameObject highlightBorder;

    public void setSlotHighLight(bool isHighLight)
    {
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(isHighLight);
        }
          
    }


    public void AddItemToSlot(ItemData item)
    {
        currentItem = item;
        UpdateSlotUI();
    }

    public void RemoveItemFromSlot()
    {
        currentItem = null;
        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {
        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.itemIcon;
            itemIcon.enabled = true;

        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

    }


}
