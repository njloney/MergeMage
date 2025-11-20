using UnityEngine;

public class PlayerWand : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Spells/Effects")]
    [SerializeField] private SpellCombinationResolver resolver;

    [Header("Wand Visuals")]
    [Tooltip("Empty GameObject on the wand model for the first crystal (slot 1)")]
    [SerializeField] private Transform socketA;
    [Tooltip("Empty GameObject on the wand model for the second crystal (slot 2)")]
    [SerializeField] private Transform socketB;

    [Header("Inventory Link")]
    [Tooltip("InventoryManager that owns the crystal slots")]
    [SerializeField] private InventoryManager inventoryManager;

    // runtime spawned models
    private GameObject spawnedModelA;
    private GameObject spawnedModelB;

    // cache last items so we only refresh visuals when something changes
    private ItemData lastSlot1Item;
    private ItemData lastSlot2Item;

    private void Start()
    {
        UpdateWandVisualsFromInventory();
    }

    private void Update()
    {
        // 1) keep wand visuals in sync with inventory
        SyncVisualsIfSlotsChanged();

        // 2) casting input
        if (Input.GetMouseButtonDown(0))
            TryCast();
    }

    // ---------------------------------------------------------------------
    // VISUAL SYNC
    // ---------------------------------------------------------------------

    private void SyncVisualsIfSlotsChanged()
    {
        if (inventoryManager == null) return;

        var slot1 = inventoryManager.CrystalSlot1;
        var slot2 = inventoryManager.CrystalSlot2;

        ItemData current1 = slot1 != null ? slot1.currentItem : null;
        ItemData current2 = slot2 != null ? slot2.currentItem : null;

        if (current1 != lastSlot1Item || current2 != lastSlot2Item)
        {
            UpdateWandVisuals(current1, current2);
            lastSlot1Item = current1;
            lastSlot2Item = current2;
        }
    }

    private void UpdateWandVisualsFromInventory()
    {
        if (inventoryManager == null) return;

        var slot1 = inventoryManager.CrystalSlot1;
        var slot2 = inventoryManager.CrystalSlot2;

        UpdateWandVisuals(
            slot1 != null ? slot1.currentItem : null,
            slot2 != null ? slot2.currentItem : null
        );
    }

    private void UpdateWandVisuals(ItemData slot1Item, ItemData slot2Item)
    {
        // Clear old models
        if (spawnedModelA != null) Destroy(spawnedModelA);
        if (spawnedModelB != null) Destroy(spawnedModelB);

        if (slot1Item != null && slot1Item.itemType == ItemType.Crystal &&
            socketA != null && slot1Item.wandModelPrefab != null)
        {
            spawnedModelA = Instantiate(
                slot1Item.wandModelPrefab,
                socketA.position,
                socketA.rotation,
                socketA
            );
        }

        if (slot2Item != null && slot2Item.itemType == ItemType.Crystal &&
            socketB != null && slot2Item.wandModelPrefab != null)
        {
            spawnedModelB = Instantiate(
                slot2Item.wandModelPrefab,
                socketB.position,
                socketB.rotation,
                socketB
            );
        }
    }

    // ---------------------------------------------------------------------
    // CASTING
    // ---------------------------------------------------------------------

    private void TryCast()
    {
        if (resolver == null || projectilePrefab == null || firePoint == null || inventoryManager == null)
            return;

        var slot1 = inventoryManager.CrystalSlot1;
        var slot2 = inventoryManager.CrystalSlot2;

        ItemData item1 = slot1 != null ? slot1.currentItem : null;
        ItemData item2 = slot2 != null ? slot2.currentItem : null;

        // Need two crystals to cast
        if (item1 == null || item2 == null) return;
        if (item1.itemType != ItemType.Crystal || item2.itemType != ItemType.Crystal) return;

        CrystalType a = item1.crystalType;
        CrystalType b = item2.crystalType;

        var stats = resolver.BuildStats(a, b);
        if (stats == null)
        {
            Debug.Log("Spell fizzled (no recipe found).");
            return;
        }

        var go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (go.TryGetComponent<ProjectileConfig>(out var cfg))
        {
            cfg.stats = stats;
            cfg.owner = this;
        }

        go.SetActive(true);

        // inventoryManager.CrystalSlot1.RemoveItemFromSlot();
        // inventoryManager.CrystalSlot2.RemoveItemFromSlot();
        // (InventoryManager will not drop these because you are manually clearing)
    }
}
