using UnityEngine;
using System;

// Handles health and damage for any object that can take hits.
// Other scripts (like projectiles or effects) call TakeDamage().
// The OnDamaged event lets other scripts (like PunchingBag) react to damage visually.
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;   // Maximum health value
    private float _hp;                                 // Current health (private)

    // Event triggered whenever this object takes damage.
    // Parameters: (damage amount, damage type, who caused it)
    public event Action<float, DamageType, UnityEngine.Object> OnDamaged;

    // Set health to full at the start
    private void Awake() => _hp = maxHealth;

    // Apply damage to this object
    public void TakeDamage(float amount, DamageType type = DamageType.Physical, UnityEngine.Object source = null)
    {
        // Reduce health but never below zero instantly
        _hp -= Mathf.Max(0f, amount);

        // Notify any listeners (e.g., UI, visual effects, punching bag counter)
        OnDamaged?.Invoke(amount, type, source);

        // If health hits zero, handle death/reset
        if (_hp <= 0f) Die();
    }

    // Called when HP reaches zero — customize for enemies, destructibles, etc.
    private void Die()
    {
        // Placeholder for death/reset logic
    }
}
