using UnityEngine;

public class EarthSpikeVisual : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;

    private void OnEnable()
    {
        Invoke(nameof(Kill), lifetime);
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
