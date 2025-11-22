using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Base Item Info")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public GameObject pickupPrefab;

    [Header("Spell Casting")]
    [Tooltip("How this item is cast when used as an active spell.")]
    public SpellCastType castType = SpellCastType.None;

    [Tooltip("Projectile prefab to spawn when castType = Projectile.")]
    public GameObject projectilePrefab;

    [Tooltip("Beam prefab to spawn when castType = Beam (e.g. Light Ray).")]
    public LightRayBeam beamPrefab;

    [Tooltip("Config passed into the beam on Activate/Init.")]
    public LightRayConfig beamConfig;

    [Header("Crystal Specifics")]
    [Tooltip("Leave this as 'None' if this item is a Consumable")]
    public CrystalType elementType;

    public GameObject wandModelPrefab;


}
