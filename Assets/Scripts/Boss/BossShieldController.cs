using UnityEngine;
using System;

[RequireComponent(typeof(Health))]
public class BossShieldController : MonoBehaviour
{
    Health health;
    int activePylons;

    public bool IsShieldActive => activePylons > 0;
    public event Action<bool> OnShieldStateChanged;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float amount, DamageType type, UnityEngine.Object source)
    {
        if (!IsShieldActive)
            return;

        if (amount >= 1)
            health.Heal(amount);
        Debug.Log($"[BossShield] Blocked {amount} damage while shield active");
    }

    public void RegisterPylon(BossShieldPylon pylon)
    {
        bool wasActive = IsShieldActive;
        activePylons++;
        Debug.Log($"[BossShield] Pylon registered. Active count: {activePylons}");

        if (wasActive != IsShieldActive)
            OnShieldStateChanged?.Invoke(IsShieldActive);
    }

    public void UnregisterPylon(BossShieldPylon pylon)
    {
        bool wasActive = IsShieldActive;
        activePylons = Mathf.Max(0, activePylons - 1);
        Debug.Log($"[BossShield] Pylon unregistered. Active count: {activePylons}");

        if (wasActive != IsShieldActive)
            OnShieldStateChanged?.Invoke(IsShieldActive);
    }
}
