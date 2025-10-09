using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform teleporter;

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        Transform otherTransform = other.transform;

        // Are we falling?

        if (rb.linearVelocity.y < 0)
        {
            Vector3 position;

            position = teleporter.position;
            position.y = teleporter.position.y
                         - (transform.position.y - otherTransform.position.y);
            otherTransform.position = position;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = -velocity.y;
            rb.linearVelocity = velocity;
        }
    }
}
