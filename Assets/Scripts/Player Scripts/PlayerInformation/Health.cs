using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;   // For non-player entities
    private float _hp;
    private RuntimePlayerStats runtimeStats;

    [Header("Damage Filtering")]
    [SerializeField] private LayerMask ignoredSourceLayers; // sources on these layers won't deal damage

    public event Action<float, DamageType, UnityEngine.Object> OnDamaged;
    public event Action OnDied;

    private void Start()
    {
        if (gameObject.CompareTag("Player"))
        {
            runtimeStats = GetComponent<RuntimePlayerStats>();

            if (runtimeStats != null)
            {
                _hp = runtimeStats.maxHealth;
                Debug.Log("Hp " + runtimeStats.maxHealth);

                // Subscribe to max health changes from passive items
                runtimeStats.OnMaxHealthChanged += Heal;
            }
            else
            {
                Debug.LogError("Health script on Player is missing RuntimePlayerStats component!");
                _hp = maxHealth;
            }
        }
        else
        {
            // Non-player entities use local maxHealth
            _hp = maxHealth;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (runtimeStats != null)
        {
            runtimeStats.OnMaxHealthChanged -= Heal;
        }
    }

    public float currentHP => _hp;

    public float maxHP
    {
        get
        {
            if (gameObject.CompareTag("Player") && runtimeStats != null)
            {
                return runtimeStats.maxHealth;
            }
            return maxHealth;
        }
    }

    public void TakeDamage(float amount, DamageType type = DamageType.Physical, UnityEngine.Object source = null)
    {
        // Ignore damage from certain sources / layers
        if (source != null)
        {
            Transform srcTransform = null;

            if (source is Component comp)
                srcTransform = comp.transform;
            else if (source is GameObject go)
                srcTransform = go.transform;

            if (srcTransform != null)
            {
                // Ignore self or any of our own children (e.g. our own hitboxes / attacks)
                if (srcTransform == transform || srcTransform.IsChildOf(transform))
                    return;

                // Ignore if the source (or any of its parents) is on an ignored layer
                Transform t = srcTransform;
                while (t != null)
                {
                    if (((1 << t.gameObject.layer) & ignoredSourceLayers) != 0)
                        return;

                    t = t.parent;
                }
            }
        }

        float damageToTake = Mathf.Max(0f, amount);

        _hp = Mathf.Max(0f, _hp - damageToTake);

        OnDamaged?.Invoke(amount, type, source);

        if (_hp == 0f) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        _hp += amount;
        _hp = Mathf.Min(_hp, maxHP); // Cap at max health

        Debug.Log($"Healed {amount} HP. Current: {_hp}/{maxHP}");
    }

    private void Die()
    {
        OnDied?.Invoke();
        // Placeholder for death/reset logic
    }
}
