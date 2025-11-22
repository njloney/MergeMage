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
    [Tooltip("The first element (order doesn't matter).")]
    public CrystalType element1;

    [Tooltip("The second element (order doesn't matter).")]
    public CrystalType element2;

    [Tooltip("ProjectileStats asset that defines damage, effects, AND castType (Projectile/Beam).")]
    public ProjectileStats resultingStats;

    [Header("Unstable Combo (optional)")]
    [Tooltip("If set, merged crystals will create a temporary unstable item using this name.")]
    public string unstableDisplayName = "Unstable Spell";

    [Tooltip("Icon used for the unstable combo item (optional).")]
    public Sprite unstableIcon;
}


[CreateAssetMenu(menuName = "Combat/Spell Combination Resolver")]
public class SpellCombinationResolver : ScriptableObject
{
    [Header("Basic Single-Crystal Spells")]
    [SerializeField] private List<BaseSpellEntry> baseSpells = new List<BaseSpellEntry>();

    [Header("Combo Recipes (Merge-Only)")]
    [SerializeField] private List<SpellRecipe> spellRecipes = new List<SpellRecipe>();

    private Dictionary<CrystalType, ProjectileStats> baseLookup;
    private Dictionary<(CrystalType, CrystalType), SpellRecipe> recipeLookup;

    private void OnEnable()
    {
        baseLookup = new Dictionary<CrystalType, ProjectileStats>();
        foreach (var entry in baseSpells)
        {
            if (entry == null || entry.baseStats == null) continue;
            if (!baseLookup.ContainsKey(entry.element))
                baseLookup.Add(entry.element, entry.baseStats);
        }

        recipeLookup = new Dictionary<(CrystalType, CrystalType), SpellRecipe>();
        foreach (var recipe in spellRecipes)
        {
            if (recipe == null || recipe.resultingStats == null) continue;
            var key = GetNormalizedKey(recipe.element1, recipe.element2);
            if (!recipeLookup.ContainsKey(key))
                recipeLookup.Add(key, recipe);
        }
    }

    private (CrystalType, CrystalType) GetNormalizedKey(CrystalType a, CrystalType b)
    {
        return (a > b) ? (b, a) : (a, b);
    }

    public ProjectileStats GetBaseStats(CrystalType element)
    {
        if (baseLookup != null && baseLookup.TryGetValue(element, out var stats))
            return stats;

        Debug.LogWarning($"[Resolver] No base spell defined for element {element}");
        return null;
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
            Debug.LogWarning($"[Resolver] No combo recipe for merge {item1.crystalType} + {item2.crystalType}");
            return null;
        }

        if (recipe.resultingStats == null)
            return null;

        var unstable = ScriptableObject.CreateInstance<ItemData>();

        unstable.itemName = string.IsNullOrEmpty(recipe.unstableDisplayName)
            ? $"Unstable {item1.crystalType} + {item2.crystalType}"
            : recipe.unstableDisplayName;

        unstable.itemIcon = recipe.unstableIcon;
        unstable.itemType = ItemType.Crystal;
        unstable.crystalType = item1.crystalType; 

        unstable.isSpellItem = true;
        unstable.projectileStats = recipe.resultingStats;
        unstable.pickupPrefab = null;

        return unstable;
    }
}