using UnityEngine;
using System;

public class Mana : MonoBehaviour
{
    private float _mana;

    private Stats playerStats;

    public event Action<float> OnManaChanged;

    private void Awake()
    {
        playerStats = Resources.Load<Stats>("PlayerResources/PlayerStats");

        if (playerStats == null)
        {
            Debug.LogError("Player Stats not assigned to Mana!", this);
            return;
        }

        _mana = playerStats.maxMana;
    }

    public float currentMana => _mana;

    public float maxMana => playerStats.maxMana;



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
        
        _mana = Mathf.Min(_mana, playerStats.maxMana);

      if (old_mana != _mana)
        {
            OnManaChanged?.Invoke(_mana);
        } // Notify UI
    }

    void Update()
    {
        if (playerStats == null || _mana >= playerStats.maxMana)
        {
            return;
        }

        float old_mana = _mana;

        _mana += playerStats.manaRecoveryRate * Time.deltaTime;

        _mana = Mathf.Min(_mana, playerStats.maxMana);



        if (old_mana != _mana)
        {
            OnManaChanged?.Invoke(_mana);
        } 



    }


}
