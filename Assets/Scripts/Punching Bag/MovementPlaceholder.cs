using UnityEngine;

public class MovementPlaceholder : MonoBehaviour
{
    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed = 0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!rb || !target) return;

        float direction = 1f;
        float dist = Vector3.Distance(transform.position, target.position);
        moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 5f) * 3f);
        if (dist < 5f) direction = -1f;

        Vector3 targetPosXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosXZ, direction * moveSpeed * Time.deltaTime);

        rb.MovePosition(nextPos);
        transform.LookAt(targetPosXZ);
    }
}
