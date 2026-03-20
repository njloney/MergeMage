using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableEffect", menuName = "Scriptable Objects/ConsumableEffect")]
public class ConsumableEffect : ScriptableObject
{
    
[Header("Effect Info")]
    public string effectName;
    public float amount;
    public float cooldown;
    public ConsumableType type;
}

public enum ConsumableType
{
restoreHealth,
restoreMana,
restoreMaxHealth,
restoreMaxMana,
giveShield

}

