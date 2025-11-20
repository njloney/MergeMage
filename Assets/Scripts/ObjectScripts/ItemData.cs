using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public Sprite itemIcon;

    public ItemType itemType;

    public GameObject pickupPrefab;

    [Header("Crystal-only Data")]
    public CrystalType crystalType;          // e.g. Fire, Ice, Wind, Earth, etc.
    public GameObject wandModelPrefab;       // visual model to attach to the wand socket
}
