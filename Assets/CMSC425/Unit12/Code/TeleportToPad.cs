using UnityEngine;

public class TeleportToPad : MonoBehaviour
{
    public Transform leftPad;
    public Transform rightPad;

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
            Vector3 position;

            if (needToJumpRight)
            {
                position = rightPad.position;
                position.y =    rightPad.position.y
                             - (leftPad.position.y - transform.position.y);
            }
            else
            {
                position = leftPad.position;
                position.y = leftPad.position.y
                             - (rightPad.position.y - transform.position.y);
            }

            transform.position = position;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = -velocity.y;
            rb.linearVelocity = velocity;

            needToJumpRight = !needToJumpRight;
        }
    }
}
