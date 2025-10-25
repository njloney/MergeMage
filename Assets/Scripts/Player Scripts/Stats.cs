using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "Scriptable Objects/Stats")]
public class Stats : ScriptableObject
{
     [Header("Movement")]
    public float speed = 12f;
    public float jump = 3f;

    [Header("Player Power")]


    [Header("Physics")]
    public float gravity = -9.81f;

    [Header("Combat")]
    public float maxHealth = 500f;

    public float maxMana = 1000f;

    public float manaRecoveryRate = 5f;

    public float meleeAttackDamage = 50f;

    public float rangeAttackDamage = 25f;


    [Header("Other")]
    public float respawnHeight = -10f;
}
