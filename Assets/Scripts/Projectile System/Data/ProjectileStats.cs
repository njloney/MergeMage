using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Projectile Stats")]
public class ProjectileStats : ScriptableObject
{
    [Header("Travel Stats")]
    public float speed = 20f;
    public float lifetime = 5f;

    [Header("Damage Stats")]
    public int baseDamage = 10;
    public DamageType damageType = DamageType.Physical;
    public bool destroyOnHit = true;

    [Header("Effects applied on hit")]
    public List<EffectSpec> effects = new();
}