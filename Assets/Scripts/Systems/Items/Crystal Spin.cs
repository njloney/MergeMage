using UnityEngine;

public class CrystalSpin : MonoBehaviour
{
    public float spinSpeed = 100f;
    public Vector3 spinAxis = Vector3.up;

    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1.5f;

    BossShieldPylon pylon;
    Rigidbody rb;
    Vector3 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        pylon = GetComponentInParent<BossShieldPylon>();
    }

    void FixedUpdate()
    {
        if (pylon == null || !pylon.IsActive)
        {
            Destroy(gameObject);
            return;
        }

        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(spinSpeed * Time.fixedDeltaTime, spinAxis));

        float y = startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 pos = new Vector3(startPos.x, y, startPos.z);
        rb.MovePosition(pos);
    }
}
