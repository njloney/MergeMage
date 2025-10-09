using UnityEngine;

public class Teleport : MonoBehaviour
{
    Rigidbody rb;

    bool needToJumpRight = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerExit()
    {
        // Are we falling?

        if (rb.linearVelocity.y < 0)
        {
            Vector3 position = transform.position;

            if (needToJumpRight)
            {
                position.x = position.x + 30;
            }
            else
            {
                position.x = position.x - 30;
            }

            transform.position = position;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = -velocity.y;
            rb.linearVelocity = velocity;

            needToJumpRight = !needToJumpRight;
        }
    }
}
