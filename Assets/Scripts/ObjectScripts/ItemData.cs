using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("General Info")]
    public string itemName;

    public Sprite itemIcon;

    public ItemType itemType;

    [Header("Crystal Specifics")]
    [Tooltip("Leave this as 'None' if this item is a Consumable")]
    public CrystalType elementType;

    public GameObject projectilePrefab;

    public GameObject pickupPrefab;

    public GameObject wandModelPrefab;


}
