using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public Sprite itemIcon;

    public ItemType itemType;

    public GameObject pickupPrefab;
}
