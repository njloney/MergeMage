using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpellRecipe
{
    [Tooltip("The first element (order doesn't matter). For pure combos, set both to the same type.")]
    public CrystalType element1;
    [Tooltip("The second element (order doesn't matter).")]
    public CrystalType element2;
    public ProjectileStats resultingStats;
}

[CreateAssetMenu(menuName = "Combat/Spell Combination Resolver")]
public class SpellCombinationResolver : ScriptableObject
{
    [SerializeField]
    private List<SpellRecipe> spellRecipes = new List<SpellRecipe>();

    private Dictionary<(CrystalType, CrystalType), ProjectileStats> recipeLookup;

    private void OnEnable()
    {
        recipeLookup = new Dictionary<(CrystalType, CrystalType), ProjectileStats>();
        foreach (var recipe in spellRecipes)
        {
            if (recipe.resultingStats == null) continue;

            // Normalize the key: always store the "lesser" enum value first
            var key = GetNormalizedKey(recipe.element1, recipe.element2);

            if (!recipeLookup.ContainsKey(key))
            {
                recipeLookup.Add(key, recipe.resultingStats);
            }
        }
    }

    // This is the method your PlayerWand.cs calls
    public ProjectileStats BuildStats(CrystalType a, CrystalType b)
    {
        // Normalize the input to get the correct key
        var key = GetNormalizedKey(a, b);

        if (recipeLookup.TryGetValue(key, out ProjectileStats stats))
        {
            return stats;
        }

        Debug.LogWarning($"No spell recipe found for combination {a} + {b}");
        return null;
    }

    private (CrystalType, CrystalType) GetNormalizedKey(CrystalType a, CrystalType b)
    {
        // Remove ordering by always returning the tuple in sorted order
        if (a > b)
        {
            return (b, a);
        }
        return (a, b);
    }
}