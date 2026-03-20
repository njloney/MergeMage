using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningJitter : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float jitterAmount = 0.5f; // How far vertices move
    [SerializeField] private float jitterInterval = 0.05f; // How fast it shakes

    private LineRenderer _lr;
    private Vector3[] _originalPositions;
    private float _timer;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        // 1. Capture the "clean" straight line shape immediately when turned on
        if (_originalPositions == null || _originalPositions.Length != _lr.positionCount)
        {
            _originalPositions = new Vector3[_lr.positionCount];
            _lr.GetPositions(_originalPositions);
        }
    }

    private void Update()
    {
        // 2. Every few milliseconds, scramble the positions
        _timer += Time.deltaTime;
        if (_timer >= jitterInterval)
        {
            _timer = 0f;
            DoJitter();
        }
    }

    private void DoJitter()
    {
        for (int i = 1; i < _lr.positionCount - 1; i++) // Skip Start (sky) and End (ground)
        {
            // Random offset
            Vector3 offset = Random.insideUnitSphere * jitterAmount;
            offset.y *= 0.2f; // Keep vertical jitter low so it doesn't stretch weirdly

            _lr.SetPosition(i, _originalPositions[i] + offset);
        }
    }
}