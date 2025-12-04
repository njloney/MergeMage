using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public int cost = 1;
}

public class WaveSpawner : MonoBehaviour
{
    public Transform player;
    public EnemySpawnEntry[] enemies;

    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 40f;

    public float waveInterval = 10f;
    public int baseBudget = 5;
    public float budgetGrowthPerSecond = 1f;

    public LayerMask spawnBlockingMask;
    public float spawnCheckRadius = 1f;

    float timeElapsed;
    float waveTimer;
    int waveIndex;

    void Start()
    {
        waveTimer = waveInterval;
    }

    void Update()
    {
        if (player == null)
            return;

        float dt = Time.deltaTime;
        timeElapsed += dt;
        waveTimer -= dt;

        if (waveTimer <= 0f)
        {
            waveTimer = waveInterval;
            SpawnWave();
        }
    }

    void SpawnWave()
    {
        waveIndex++;

        if (enemies == null || enemies.Length == 0)
            return;

        int budget = baseBudget + Mathf.FloorToInt(timeElapsed * budgetGrowthPerSecond);
        if (budget <= 0)
            return;

        int minCost = int.MaxValue;
        foreach (var e in enemies)
        {
            if (e != null && e.prefab != null)
                minCost = Mathf.Min(minCost, e.cost);
        }
        if (minCost == int.MaxValue)
            return;

        Debug.Log($"[WaveSpawner] Wave {waveIndex} start, budget {budget}");

        int safety = 1000;
        while (budget >= minCost && safety-- > 0)
        {
            var entry = GetRandomAffordableEnemy(budget);
            if (entry == null)
                break;

            Vector3 pos;
            if (!TryGetSpawnPosition(out pos))
            {
                Debug.LogWarning("[WaveSpawner] Failed to find spawn position; stopping wave");
                break;
            }

            Instantiate(entry.prefab, pos, Quaternion.identity);
            budget -= entry.cost;
            Debug.Log($"[WaveSpawner] Spawned {entry.prefab.name}, cost {entry.cost}, remaining {budget}");
        }
    }

    EnemySpawnEntry GetRandomAffordableEnemy(int budget)
    {
        var list = new List<EnemySpawnEntry>();
        foreach (var e in enemies)
        {
            if (e != null && e.prefab != null && e.cost <= budget)
                list.Add(e);
        }

        if (list.Count == 0)
            return null;

        int idx = Random.Range(0, list.Count);
        return list[idx];
    }

    bool TryGetSpawnPosition(out Vector3 pos)
    {
        pos = Vector3.zero;
        if (player == null)
            return false;

        const int attempts = 20;
        for (int i = 0; i < attempts; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

            Vector3 candidate = player.position + offset;

            if (spawnBlockingMask.value != 0)
            {
                if (Physics.CheckSphere(candidate, spawnCheckRadius, spawnBlockingMask))
                    continue;
            }

            pos = candidate;
            return true;
        }

        return false;
    }
}
