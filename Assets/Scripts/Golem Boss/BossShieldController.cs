using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossShieldController : MonoBehaviour
{
    Health health;
    int activePylons;

    public bool IsShieldActive => activePylons > 0;

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

    void HandleDamaged(float amount, DamageType type, Object source)
    {
        if (!IsShieldActive)
            return;

        health.Heal(amount);
        Debug.Log($"[BossShield] Blocked {amount} damage while shield active");
    }

    public void RegisterPylon(BossShieldPylon pylon)
    {
        activePylons++;
        Debug.Log($"[BossShield] Pylon registered. Active count: {activePylons}");
    }

    public void UnregisterPylon(BossShieldPylon pylon)
    {
        activePylons = Mathf.Max(0, activePylons - 1);
        Debug.Log($"[BossShield] Pylon unregistered. Active count: {activePylons}");
    }
}
