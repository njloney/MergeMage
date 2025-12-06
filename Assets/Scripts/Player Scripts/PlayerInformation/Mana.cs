using UnityEngine;
using System;

public class Mana : MonoBehaviour
{
    private float _mana;
    private RuntimePlayerStats runtimeStats;

    public event Action<float> OnManaChanged;

    private void Start()
    {
        runtimeStats = GetComponent<RuntimePlayerStats>();

        if (runtimeStats == null)
        {
            Debug.LogError("RuntimePlayerStats not found on Player!", this);
            return;
        }

        _mana = runtimeStats.maxMana;

        // Subscribe to max mana changes from passive items
        runtimeStats.OnMaxManaChanged += RestoreMana;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (runtimeStats != null)
        {
            runtimeStats.OnMaxManaChanged -= RestoreMana;
        }
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

    
    }
}