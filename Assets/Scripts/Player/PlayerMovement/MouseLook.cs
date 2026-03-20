using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float xRotation = 0f;

    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private float shakeDamping = 1.0f; // How fast the shake settles

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Call this from your Projectile or Explosion scripts
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float xShake = 0f;
        float yShake = 0f;

        if (shakeDuration > 0)
        {
            xShake = Random.Range(-1f, 1f) * shakeMagnitude;
            yShake = Random.Range(-1f, 1f) * shakeMagnitude;

            shakeDuration -= Time.deltaTime * shakeDamping;
        }
        else
        {
            shakeDuration = 0f;
            xShake = 0f;
            yShake = 0f;
        }
        transform.localRotation = Quaternion.Euler(xRotation + xShake, yShake, 0f); // Add yShake to Y axis here (Camera roll/yaw)

        playerBody.Rotate(Vector3.up * (mouseX)); // Keep body rotation separate so shake doesn't turn the player
    }
}