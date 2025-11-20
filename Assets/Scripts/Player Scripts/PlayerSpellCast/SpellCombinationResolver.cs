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
    public ItemData BuildComboSpell(ItemData Crystala, ItemData Crystalb)

    {
        // Normalize the input to get the correct key
        var key = GetNormalizedKey(Crystala.elementType, Crystalb.elementType);


        if (recipeLookup.TryGetValue(key, out ProjectileStats stats))
        {
            ItemData newComboSpell = Resources.Load<ItemData>("ObjectResources/UnstableComboSpell");
            if (newComboSpell != null && newComboSpell.projectilePrefab != null)
            {
                ProjectileConfig config = newComboSpell.projectilePrefab.GetComponent<ProjectileConfig>();
                Renderer rend = newComboSpell.projectilePrefab.GetComponent<Renderer>();
                if (config != null && rend != null)
                {
                        config.stats = stats;



                        Color A = getColorFromCrystal(Crystala);
                        Color B = getColorFromCrystal(Crystalb);
                        Color fusedColor = getFusedColor(A, B);
                        rend.sharedMaterial.color = fusedColor;



                        return newComboSpell;

                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }

        }

        Debug.LogWarning($"No spell recipe found for combination {Crystala.elementType} + {Crystalb.elementType}");
        return null;
    }


    private Color getColorFromCrystal(ItemData crystal)
    {
        if (crystal == null || crystal.projectilePrefab == null)
        {
            return Color.white;

        }

        Renderer rend = crystal.projectilePrefab.GetComponent<Renderer>();
        
        if(rend != null)
        {
            return rend.sharedMaterial.color;

        }
        

        return Color.white;
    }

    private Color getFusedColor(Color a, Color b)
    {

        if (a == b)
        {
            return Color.Lerp(a, Color.grey, 0.5f);

        }
        
        return Color.Lerp(a, b, 0.5f);

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