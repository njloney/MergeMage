using UnityEngine;
using System.Collections.Generic;

// Stationary zone that applies damage when the player stays inside.
public class DamageOverTimeZone : MonoBehaviour
{
    private float damagePerTick = 10f;
    private float tickInterval = 1f;
    private DamageType damageType = DamageType.Physical;
    private HashSet<Health> targetsInside = new HashSet<Health>();
    private float tickTimer;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set to Is Trigger. DamageOverTimeZone will not function correctly.", this);
        }
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            DoTick();
        }
    }

    private void DoTick()
    {
        // Apply damage to every target currently tracked inside the zone
        var currentTargets = new List<Health>(targetsInside);

        foreach (var hp in currentTargets)
        {
            if (hp != null)
            {
                hp.TakeDamage(damagePerTick, damageType, this);
                Debug.Log($"{hp.gameObject.name} took {damagePerTick} damage from {gameObject.name}.");
            }
            else
            {
                targetsInside.Remove(hp);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<Health>(out var playerHealth))
            {
                targetsInside.Add(playerHealth);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<Health>(out var playerHealth))
            {
                targetsInside.Remove(playerHealth);
            }
        }
    }
}