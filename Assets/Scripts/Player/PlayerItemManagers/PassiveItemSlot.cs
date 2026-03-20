using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveItemSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI stackText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     
    public void setSlot(ItemData item, int count)
    {
        if (item != null)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
        }

        updateStackCount(count);
    }


    public void updateStackCount(int count)
    {
        if (count > 1)
        {
            stackText.text = count.ToString();
            stackText.enabled = true;
        }
        else
        {
            stackText.enabled = false;
        }


    }

}
