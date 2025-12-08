using UnityEngine;

public class GolemDeathSpawnChunks : MonoBehaviour
{
    [System.Serializable]
    public class ChunkEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [SerializeField] private Health health;
    [SerializeField] private Transform[] rockParts;
    [SerializeField] private Animator animator;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Chunk Prefabs")]
    [SerializeField] private ChunkEntry[] chunkPrefabs;
    [SerializeField] private GameObject fallbackChunkPrefab;

    [Header("Chunk Spawn Settings")]
    [SerializeField] private int maxChunks = 50;
    [SerializeField] private int chunksPerPoint = 3;
    [SerializeField] private float spawnJitterRadius = 0.5f;
    [SerializeField] private Vector2 chunkScaleRange = new Vector2(0.3f, 0.6f);
    [SerializeField] private float chunkLifetime = 8f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 200f;
    [SerializeField] private float explosionRadius = 5f;

    [Header("Cleanup")]
    [SerializeField] private float destroyBossDelay = 1.0f;

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDied += OnDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= OnDied;
    }

    private void OnDied()
    {
        Debug.Log("[GolemDeathSpawnChunks] OnDied for " + name);

        if (animator) animator.enabled = false;

        foreach (var s in scriptsToDisable)
        {
            if (s) s.enabled = false;
        }

        if ((chunkPrefabs == null || chunkPrefabs.Length == 0) && fallbackChunkPrefab == null)
        {
            Debug.LogWarning("[GolemDeathSpawnChunks] No chunk prefabs assigned.");
            return;
        }

        if (rockParts == null || rockParts.Length == 0)
        {
            Debug.LogWarning("[GolemDeathSpawnChunks] No rockParts assigned.");
            return;
        }

        int spawned = 0;

        foreach (var part in rockParts)
        {
            if (!part) continue;
            if (spawned >= maxChunks) break;

            var skin = part.GetComponent<SkinnedMeshRenderer>();
            if (skin) skin.enabled = false;

            for (int i = 0; i < chunksPerPoint && spawned < maxChunks; i++)
            {
                GameObject prefab = PickChunkPrefab();
                if (prefab == null) break;

                Vector3 basePos = part.position;
                Vector2 jitter2D = Random.insideUnitCircle * spawnJitterRadius;
                Vector3 spawnPos = basePos + new Vector3(jitter2D.x, 0f, jitter2D.y);

                var chunk = Instantiate(prefab, spawnPos, Random.rotation);

                float scale = Random.Range(chunkScaleRange.x, chunkScaleRange.y);
                chunk.transform.localScale = Vector3.one * scale;

                var rb = chunk.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddExplosionForce(
                        explosionForce,
                        transform.position + Vector3.up,
                        explosionRadius
                    );
                }

                if (chunkLifetime > 0f)
                    Destroy(chunk, chunkLifetime);

                spawned++;
            }
        }

        Debug.Log($"[GolemDeathSpawnChunks] Spawned {spawned} chunks");

        if (destroyBossDelay >= 0f)
            Destroy(gameObject, destroyBossDelay);
    }

    private GameObject PickChunkPrefab()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
            return fallbackChunkPrefab;

        float total = 0f;
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            var entry = chunkPrefabs[i];
            if (entry != null && entry.prefab != null && entry.weight > 0f)
                total += entry.weight;
        }

        if (total <= 0f)
            return fallbackChunkPrefab;

        float r = Random.value * total;
        float accum = 0f;

        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            var entry = chunkPrefabs[i];
            if (entry == null || entry.prefab == null || entry.weight <= 0f)
                continue;

            accum += entry.weight;
            if (r <= accum)
                return entry.prefab;
        }

        return fallbackChunkPrefab;
    }
}
