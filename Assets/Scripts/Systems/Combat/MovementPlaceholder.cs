using UnityEngine;

public class MovementPlaceholder : MonoBehaviour
{
    public enum MovementMode
    {
        Chase,
        Flee
    }

    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed;
    public float desiredDistance = 5f;
    public MovementMode mode = MovementMode.Chase;

    Rigidbody rb;
    MovementModifiersPlaceholder mods;

    Vector3 blindTarget;
    float blindRetargetTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mods = GetComponent<MovementModifiersPlaceholder>();
    }

    void Update()
    {
        if (!rb || !target)
            return;

        float speedMult = mods != null ? mods.speedMultiplier : 1f;
        if (speedMult <= 0f)
            return;

        bool blinded = mods != null && mods.isBlinded;
        if (blinded)
        {
            UpdateBlinded(speedMult);
            return;
        }

        Vector3 targetPosXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
        float dist = Vector3.Distance(transform.position, targetPosXZ);

        float direction = 0f;
        if (mode == MovementMode.Chase)
        {
            direction = dist > desiredDistance ? 1f : -1f;
        }
        else
        {
            if (dist < desiredDistance)
                direction = -1f;
            else
                direction = 0f;
        }

        if (direction == 0f)
        {
            moveSpeed = 0f;
            return;
        }

        moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - desiredDistance) * 3f) * speedMult;

        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosXZ, moveSpeed * Time.deltaTime);
        rb.MovePosition(nextPos);
        transform.LookAt(targetPosXZ);
    }

    void UpdateBlinded(float speedMult)
    {
        blindRetargetTimer -= Time.deltaTime;

        if (blindRetargetTimer <= 0f || Vector3.Distance(transform.position, blindTarget) < 0.5f)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float radius = Random.Range(2f, 6f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            blindTarget = transform.position + offset;
            blindRetargetTimer = Random.Range(0.5f, 1.5f);
        }

        Vector3 targetPosXZ = new Vector3(blindTarget.x, transform.position.y, blindTarget.z);
        float dist = Vector3.Distance(transform.position, targetPosXZ);
        moveSpeed = Mathf.Min(maxSpeed, dist + 0.1f) * speedMult;

        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosXZ, moveSpeed * Time.deltaTime);
        rb.MovePosition(nextPos);
        transform.LookAt(targetPosXZ);
    }
}
