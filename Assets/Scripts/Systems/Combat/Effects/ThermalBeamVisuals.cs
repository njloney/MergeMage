using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThermalBeamVisuals : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color hotColor = new Color(1f, 0.2f, 0f); // Bright Red-Orange
    [SerializeField] private Color coldColor = new Color(0f, 1f, 1f);  // Bright Cyan

    [Header("Animation")]
    [Tooltip("How fast it switches between hot and cold.")]
    [SerializeField] private float flickerSpeed = 15f;

    [Tooltip("If true, color is mixed. If false, it snaps between red and blue.")]
    [SerializeField] private bool smoothTransition = false;

    [Header("Beam Jitter (Optional)")]
    [SerializeField] private float widthMultiplier = 1f;
    [SerializeField] private float jitterAmount = 0.1f;

    private LineRenderer _lr;
    private float _baseStartWidth;
    private float _baseEndWidth;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _baseStartWidth = _lr.startWidth;
        _baseEndWidth = _lr.endWidth;
    }

    private void Update()
    {
        AnimateColor();
        AnimateWidth();
    }

    private void AnimateColor()
    {
        // Generate a noise value between 0 and 1
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

        Color targetColor;

        if (smoothTransition)
        {
            // Smoothly blend between Fire and Ice
            targetColor = Color.Lerp(hotColor, coldColor, noise);
        }
        else
        {
            // Snap to either Fire OR Ice (More chaotic/binary look)
            targetColor = (noise > 0.5f) ? hotColor : coldColor;
        }

        // Apply to the beam
        // Note: Ensure your LineRenderer Material uses "Particles/Standard Unlit" 
        // or "Sprites/Default" so Vertex Colors show up!
        _lr.startColor = targetColor;
        _lr.endColor = targetColor;
    }

    private void AnimateWidth()
    {
        // Subtle vibration in width to make it look high energy
        float widthJitter = 1f + Random.Range(-jitterAmount, jitterAmount);
        _lr.startWidth = _baseStartWidth * widthMultiplier * widthJitter;
        _lr.endWidth = _baseEndWidth * widthMultiplier * widthJitter;
    }
}