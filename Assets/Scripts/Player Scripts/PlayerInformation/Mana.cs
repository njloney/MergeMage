using UnityEngine;
using System;

public class Mana : MonoBehaviour
{
    private float _mana;
    private RuntimePlayerStats runtimeStats;

    public event Action<float> OnManaChanged;

    private void Awake()
    {
        runtimeStats = GetComponent<RuntimePlayerStats>();

        if (runtimeStats == null)
        {
            Debug.LogError("RuntimePlayerStats not found on Player!", this);
            return;
        }

        _mana = runtimeStats.maxMana;

        // Subscribe to max mana changes from passive items
        runtimeStats.OnMaxManaChanged += OnMaxManaIncrease;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (runtimeStats != null)
        {
            runtimeStats.OnMaxManaChanged -= OnMaxManaIncrease;
        }
    }

    private void OnMaxManaIncrease(float manaIncrease)
    {
        // When max mana increases from passive items, restore by that amount
        _mana += manaIncrease;
        OnManaChanged?.Invoke(_mana);
        Debug.Log($"Max mana increased! Current: {_mana}/{maxMana}");
    }

    public float currentMana => _mana;

    public float maxMana => runtimeStats != null ? runtimeStats.maxMana : 0f;

    public void useMana(float amount)
    {
        if (amount <= 0) return;

        float old_mana = _mana;

        _mana -= amount;
        if (_mana < 0) _mana = 0;

        if (old_mana != _mana)
        {
            OnManaChanged?.Invoke(_mana);
        }
    }

    public bool hasMana(float amount)
    {
        return _mana >= amount;
    }

    public void RestoreMana(float amount)
    {
        float old_mana = _mana;

        _mana += amount;
        _mana = Mathf.Min(_mana, maxMana);

        if (old_mana != _mana)
        {
            OnManaChanged?.Invoke(_mana);
        }
    }

    void Update()
    {
        if (runtimeStats == null || _mana >= maxMana)
        {
            return;
        }

        float old_mana = _mana;

        // Use runtime mana recovery rate (can be modified by passive items)
        _mana += runtimeStats.manaRecoveryRate * Time.deltaTime;
        _mana = Mathf.Min(_mana, maxMana);

        if (old_mana != _mana)
        {
            OnManaChanged?.Invoke(_mana);
        }
    }
}