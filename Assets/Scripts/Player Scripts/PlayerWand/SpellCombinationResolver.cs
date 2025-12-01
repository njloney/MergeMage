using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BaseSpellEntry
{
    [Tooltip("Crystal element this basic spell belongs to (Fire, Ice, Wind, etc.).")]
    public CrystalType element;

    [Tooltip("ProjectileStats for this element's basic spell.")]
    public ProjectileStats baseStats;
}


[System.Serializable]
public class SpellRecipe
{
    [Tooltip("First crystal element (order doesn't matter).")]
    public CrystalType element1;

    [Tooltip("Second crystal element (order doesn't matter).")]
    public CrystalType element2;

    [Header("Merged Spell Prefab")]
    [Tooltip("Prefab to spawn for the merged spell (e.g. Fireball projectile prefab).")]
    public GameObject mergedSpellPrefab;

    [Header("Unstable item visuals (for inventory UI)")]
    [Tooltip("Name shown for the unstable merged spell item.")]
    public string unstableDisplayName = "Unstable Spell";

    [Tooltip("Icon shown for the merged spell in the merge slot.")]
    public Sprite unstableIcon;
}



[CreateAssetMenu(menuName = "Combat/Spell Combination Resolver")]
public class SpellCombinationResolver : ScriptableObject
{
    [SerializeField]
    private List<SpellRecipe> spellRecipes = new List<SpellRecipe>();

    private Dictionary<(CrystalType, CrystalType), SpellRecipe> recipeLookup;

    private void OnEnable()
    {
        recipeLookup = new Dictionary<(CrystalType, CrystalType), SpellRecipe>();

        foreach (var recipe in spellRecipes)
        {
            if (recipe == null || recipe.mergedSpellPrefab == null)
                continue;

            var key = GetNormalizedKey(recipe.element1, recipe.element2);

            if (!recipeLookup.ContainsKey(key))
            {
                recipeLookup.Add(key, recipe);
            }
        }
    }

    private (CrystalType, CrystalType) GetNormalizedKey(CrystalType a, CrystalType b)
    {
        return (a > b) ? (b, a) : (a, b);
    }


    public ItemData BuildComboSpell(ItemData item1, ItemData item2)
    {
        if (item1 == null || item2 == null)
            return null;

        if (item1.itemType != ItemType.Crystal || item2.itemType != ItemType.Crystal)
            return null;

        var key = GetNormalizedKey(item1.crystalType, item2.crystalType);

        if (!recipeLookup.TryGetValue(key, out var recipe))
        {
            Debug.LogWarning($"[Resolver] No merge recipe for {item1.crystalType} + {item2.crystalType}");
            return null;
        }

        if (recipe.mergedSpellPrefab == null)
        {
            Debug.LogWarning($"[Resolver] Merge recipe {item1.crystalType}+{item2.crystalType} has no mergedSpellPrefab.");
            return null;
        }

        // Create an unstable ItemData at runtime that represents the merged spell.
        var unstable = ScriptableObject.CreateInstance<ItemData>();

        unstable.itemName = string.IsNullOrEmpty(recipe.unstableDisplayName)
            ? $"Unstable {item1.crystalType} + {item2.crystalType}"
            : recipe.unstableDisplayName;

        unstable.itemIcon = recipe.unstableIcon;
        unstable.itemType = ItemType.Crystal; // or a dedicated Spell type if you add one
        unstable.crystalType = item1.crystalType; // arbitrary, main thing is isSpellItem + spellPrefab

        unstable.isSpellItem = true;
        unstable.spellPrefab = recipe.mergedSpellPrefab;  

        unstable.pickupPrefab = null; // you don't drop this as a world pickup

        Debug.Log($"[Resolver] Built unstable combo item for {item1.crystalType}+{item2.crystalType} using prefab '{recipe.mergedSpellPrefab.name}'");

        return unstable;
    }


}