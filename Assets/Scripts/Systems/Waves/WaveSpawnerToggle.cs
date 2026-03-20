using UnityEngine;

public class WaveSpawnerToggle : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.P;
    [SerializeField] private WaveSpawner waveSpawner;

    void Update()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;

        if (waveSpawner == null)
        {
            Debug.LogWarning("[WaveSpawnerToggle] WaveSpawner reference missing.");
            return;
        }

        waveSpawner.enabled = !waveSpawner.enabled;

        Debug.Log($"[WaveSpawnerToggle] WaveSpawner {(waveSpawner.enabled ? "ENABLED" : "DISABLED")}");
    }
}
