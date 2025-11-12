// MovementPlaceholder.cs
using UnityEngine;

public class MovementPlaceholder : MonoBehaviour
{
    public Transform target;
    public float maxSpeed = 20f; // max speed of movement
    public float moveSpeed = 0f; // current movement speed

    void Update()
    {
        // --- compute baseline speed from distance, same as before ---
        float dist = Vector3.Distance(transform.position, target.position);
        moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 5f) * 3f);

        // --- apply optional modifiers (slow, blind, etc.) ---
        var mods = GetComponent<MovementModifiersPlaceholder>();
        float speedMul = (mods != null) ? Mathf.Clamp01(mods.speedMultiplier) : 1f;
        bool blinded = (mods != null) && mods.isBlinded;

        float finalSpeed = moveSpeed * speedMul;

        if (!blinded)
        {
            // NORMAL BEHAVIOR: move toward/away from player and face them
            float direction = (dist < 5f) ? -1f : 1f;

            // target position on XZ plane
            Vector3 targetPosXZ = new Vector3(target.position.x, transform.position.y, target.position.z);

            // move toward/away
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosXZ,
                direction * finalSpeed * Time.deltaTime
            );

            // face the player
            transform.LookAt(targetPosXZ);
        }
        else
        {

            Vector3 fwdXZ = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            transform.position += fwdXZ * finalSpeed * Time.deltaTime;

        }
    }
}
