using UnityEngine;

public class RigidbodyUpright : MonoBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;

        Vector3 rot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rot.y, 0f);
    }
}
