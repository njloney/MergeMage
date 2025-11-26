using UnityEngine;

[CreateAssetMenu(fileName = "StatModifier", menuName = "Passive Items/Stat Modifier")]
public class StatModifierEffect : PassiveItemEffect
{
    [Header("Stat Modifications")]
    public StatModifier[] statModifiers;

    public override void OnAcquire(GameObject player, int stackCount)
    {
        ApplyStatModifiers(player, stackCount);
    }

    public override void OnStack(GameObject player, int newStackCount)
    {
        // When stacking, we need to recalculate from scratch
        RuntimePlayerStats runtimeStats = player.GetComponent<RuntimePlayerStats>();
        PassiveItemManager passiveManager = player.GetComponent<PassiveItemManager>();

        if (runtimeStats != null && passiveManager != null)
        {
            runtimeStats.RecalculateAllStats(passiveManager);
        }
    }

    private void ApplyStatModifiers(GameObject player, int stackCount)
    {
        RuntimePlayerStats runtimeStats = player.GetComponent<RuntimePlayerStats>();
        if (runtimeStats == null)
        {
            Debug.LogError("RuntimePlayerStats not found on player!");
            return;
        }

        foreach (var modifier in statModifiers)
        {
            float value = GetStackedValue(modifier.value, stackCount);

            switch (modifier.statType)
            {
                case StatType.MaxHealth:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatMaxHealth(value);
                    else
                        runtimeStats.AddPercentMaxHealth(value);
                    break;

                case StatType.MaxMana:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatMaxMana(value);
                    else
                        runtimeStats.AddPercentMaxMana(value);
                    break;

                case StatType.ManaRecoveryRate:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatManaRecovery(value);
                    else
                        runtimeStats.AddManaRecoveryMultiplier(value);
                    break;

                case StatType.Speed:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatSpeed(value);
                    else
                        runtimeStats.AddSpeedMultiplier(value);
                    break;

                case StatType.Jump:
                    runtimeStats.AddFlatJump(value);
                    break;

                case StatType.MeleeDamage:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatMeleeDamage(value);
                    else
                        runtimeStats.AddDamageMultiplier(value);
                    break;

                case StatType.RangeDamage:
                    if (modifier.modifierType == ModifierType.Flat)
                        runtimeStats.AddFlatRangeDamage(value);
                    else
                        runtimeStats.AddDamageMultiplier(value);
                    break;
            }
        }
    }
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;
}

public enum StatType
{
    MaxHealth,
    MaxMana,
    ManaRecoveryRate,
    Speed,
    Jump,
    MeleeDamage,
    RangeDamage
}

public enum ModifierType
{
    Flat,
    Percentage 
}