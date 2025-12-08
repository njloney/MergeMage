using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float _hp;
    private RuntimePlayerStats runtimeStats;

    [Header("Damage Filtering")]
    [SerializeField] public LayerMask ignoredSourceLayers;

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
                Debug.Log($"[Health] {name} initialized with HP: {_hp}");
                OnDamaged?.Invoke(_hp, DamageType.Physical, null);
                runtimeStats.OnMaxHealthChanged += Heal;
            }
            else
            {
                Debug.LogError("[Health] Player missing RuntimePlayerStats!");
                _hp = maxHealth;
            }
        }
        else
        {
            _hp = maxHealth;
            Debug.Log($"[Health] {name} initialized with HP: {_hp}");
        }
    }

    private void OnDestroy()
    {
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
                return runtimeStats.maxHealth;
            return maxHealth;
        }
    }

    public void TakeDamage(float amount, DamageType type = DamageType.Physical, UnityEngine.Object source = null)
    {
        bool isPlayer = gameObject.CompareTag("Player");

        if (source != null)
        {
            Transform srcTransform = null;

            if (source is Component comp)
                srcTransform = comp.transform;
            else if (source is GameObject go)
                srcTransform = go.transform;

            if (srcTransform != null)
            {
                if (srcTransform == transform || srcTransform.IsChildOf(transform))
                {
                    Debug.Log($"[Health] {name} ignored self-damage from {srcTransform.name}");
                    return;
                }

                Transform t = srcTransform;
                while (t != null)
                {
                    if (!isPlayer && ((1 << t.gameObject.layer) & ignoredSourceLayers) != 0)
                    {
                        Debug.Log($"[Health] {name} ignored damage from layer {LayerMask.LayerToName(t.gameObject.layer)}");
                        return;
                    }

                    t = t.parent;
                }
            }
        }

        float damageToTake = Mathf.Max(0f, amount);
        float before = _hp;

        _hp = Mathf.Max(0f, _hp - damageToTake);

        Debug.Log($"[Health] {name} took {damageToTake} {type} damage from {(source ? source.name : "Unknown")}. HP: {before} -> {_hp}");

        OnDamaged?.Invoke(amount, type, source);

        int layer = gameObject.layer;

        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            if (CrosshairUI.Instance != null)
                CrosshairUI.Instance.ShowHitmarker();
        }
        else if (layer == LayerMask.NameToLayer("Player"))
        {
            if (ScreenDamageFlashUI.Instance != null)
                ScreenDamageFlashUI.Instance.Flash();
        }

        if (_hp == 0f)
        {
            Debug.Log($"[Health] {name} DIED");
            Die();
        }
    }


    public void Heal(float amount)
    {
        if (amount <= 0) return;

        float before = _hp;
        _hp += amount;
        _hp = Mathf.Min(_hp, maxHP);

        Debug.Log($"[Health] {name} healed {amount}. HP: {before} -> {_hp}");
    }

    private void Die()
    {
        Debug.Log($"[Health] {name} OnDied event invoked");
        OnDied?.Invoke();
    }
}
