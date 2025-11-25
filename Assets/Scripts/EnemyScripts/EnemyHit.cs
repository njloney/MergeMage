using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Health))]
public class EnemySlime : MonoBehaviour
{
    [Header("Visuals")]
    public Material HurtMat;                 // Material shown when the bag is hit
    public Material IdleMat;                 // Default material when idle
    public TextMeshProUGUI damageTakenText;  // UI text showing total damage taken

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 3.0f; // Time before combo resets

    private Renderer rend;    // Cached renderer for color/material changes
    private Health health;    // Reference to the Health component

    private int totalDamageTaken = 0; // Tracks running total of recent hits
    private bool resetDamage;         // Flag for manual reset (unused but left in)

    private void Start()
    {
        rend = GetComponent<Renderer>();
        health = GetComponent<Health>();

        // Listen for damage events from the Health script
        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;
    }

    private void OnDisable()
    {
        // Stop timers and unsubscribe when disabled
        CancelInvoke();
        if (health != null)
            health.OnDamaged -= HandleDamageTaken;
    }

    private void Update()
    {
        Debug.Log(health._hp);
        
        // Safely update UI text if assigned
        if (damageTakenText != null)
        {
            // Use SetText to avoid GC from ToString allocations in tight loops
            damageTakenText.SetText(totalDamageTaken.ToString());
        }

        // Manual reset if flag is triggered (optional feature)
        if (resetDamage)
        {
            totalDamageTaken = 0;
            resetDamage = false;
        }
    }

    // Called whenever this object takes damage
    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
        // Add this hit’s damage to the running total
        totalDamageTaken += Mathf.RoundToInt(amount);

        // Restart combo reset timer
        CancelInvoke(nameof(ResetCombo));
        Invoke(nameof(ResetCombo), comboWindow);

        // Flash red to show impact
        StartCoroutine(FlashRed());
    }

    // Quick red flash when hit
    private IEnumerator FlashRed()
    {
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        rend.material = IdleMat;
    }

    // Resets the combo damage total after time runs out
    private void ResetCombo()
    {
        totalDamageTaken = 0;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
