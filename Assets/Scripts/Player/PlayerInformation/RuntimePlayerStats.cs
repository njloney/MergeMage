using UnityEngine;
using System;

public class RuntimePlayerStats : MonoBehaviour
{
    [Header("Base Stats Reference")]
    [SerializeField] private Stats baseStats;

    private float _speed;
    private float _jump;
    private float _gravity;
    private float _maxHealth;
    private float _maxMana;
    private float _manaRecoveryRate;
    private float _meleeAttackDamage;
    private float _rangeAttackDamage;

    private float _speedMultiplier = 1f;
    private float _damageMultiplier = 1f;
    private float _manaRegenMultiplier = 1f;

    public event Action OnStatsChanged;
    public event Action<float> OnMaxHealthChanged;
    public event Action<float> OnMaxManaChanged;   

    private bool _suppressMaxEvents;

    private void Awake()
    {
        if (baseStats == null)
        {
            baseStats = Resources.Load<Stats>("PlayerResources/PlayerStats");
            if (baseStats == null)
            {
                Debug.LogError("RuntimePlayerStats: Could not find PlayerStats asset!");
                return;
            }
        }

        ResetToBaseStats();
    }

    public void ResetToBaseStats()
    {
        _speed = baseStats.speed;
        _jump = baseStats.jump;
        _gravity = baseStats.gravity;
        _maxHealth = baseStats.maxHealth;
        _maxMana = baseStats.maxMana;
        _manaRecoveryRate = baseStats.manaRecoveryRate;
        _meleeAttackDamage = baseStats.meleeAttackDamage;
        _rangeAttackDamage = baseStats.rangeAttackDamage;

        _speedMultiplier = 1f;
        _damageMultiplier = 1f;
        _manaRegenMultiplier = 1f;

        OnStatsChanged?.Invoke();
    }

    public float speed => _speed * _speedMultiplier;
    public float jump => _jump;
    public float gravity => _gravity;
    public float maxHealth => _maxHealth;
    public float maxMana => _maxMana;
    public float manaRecoveryRate => _manaRecoveryRate * _manaRegenMultiplier;
    public float meleeAttackDamage => _meleeAttackDamage * _damageMultiplier;
    public float rangeAttackDamage => _rangeAttackDamage * _damageMultiplier;
    public float respawnHeight => baseStats.respawnHeight;

    public void AddFlatSpeed(float amount)
    {
        _speed += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddSpeedMultiplier(float multiplier)
    {
        _speedMultiplier += multiplier;
        OnStatsChanged?.Invoke();
    }

    public void AddFlatMaxHealth(float amount)
    {
        float oldMax = _maxHealth;
        _maxHealth += amount;

        if (!_suppressMaxEvents)
            OnMaxHealthChanged?.Invoke(_maxHealth - oldMax);

        OnStatsChanged?.Invoke();
    }

    public void AddPercentMaxHealth(float percent)
    {
        float oldMax = _maxHealth;
        _maxHealth *= (1f + percent);

        if (!_suppressMaxEvents)
            OnMaxHealthChanged?.Invoke(_maxHealth - oldMax);

        OnStatsChanged?.Invoke();
    }

    public void AddFlatMaxMana(float amount)
    {
        float oldMax = _maxMana;
        _maxMana += amount;

        if (!_suppressMaxEvents)
            OnMaxManaChanged?.Invoke(_maxMana - oldMax);

        OnStatsChanged?.Invoke();
    }

    public void AddPercentMaxMana(float percent)
    {
        float oldMax = _maxMana;
        _maxMana *= (1f + percent);

        if (!_suppressMaxEvents)
            OnMaxManaChanged?.Invoke(_maxMana - oldMax);

        OnStatsChanged?.Invoke();
    }

    public void AddFlatManaRecovery(float amount)
    {
        _manaRecoveryRate += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddManaRecoveryMultiplier(float multiplier)
    {
        _manaRegenMultiplier += multiplier;
        OnStatsChanged?.Invoke();
    }

    public void AddFlatMeleeDamage(float amount)
    {
        _meleeAttackDamage += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddFlatRangeDamage(float amount)
    {
        _rangeAttackDamage += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddDamageMultiplier(float multiplier)
    {
        _damageMultiplier += multiplier;
        OnStatsChanged?.Invoke();
    }

    public void AddFlatJump(float amount)
    {
        _jump += amount;
        OnStatsChanged?.Invoke();
    }

    public void RecalculateAllStats(PassiveItemManager passiveManager)
    {
        float oldMaxHP = _maxHealth;
        float oldMaxMana = _maxMana;

        _suppressMaxEvents = true;

        ResetToBaseStats();

        var allItems = passiveManager.GetAllPassiveItems();
        foreach (var kvp in allItems)
        {
            ItemData item = kvp.Key;
            int stackCount = kvp.Value;

            if (item.passiveEffect != null)
                item.passiveEffect.OnAcquire(gameObject, stackCount);
        }

        _suppressMaxEvents = false;

        float hpDelta = _maxHealth - oldMaxHP;
        float manaDelta = _maxMana - oldMaxMana;

        if (Mathf.Abs(hpDelta) > 0.0001f) OnMaxHealthChanged?.Invoke(hpDelta);
        if (Mathf.Abs(manaDelta) > 0.0001f) OnMaxManaChanged?.Invoke(manaDelta);

        OnStatsChanged?.Invoke();
    }
}
