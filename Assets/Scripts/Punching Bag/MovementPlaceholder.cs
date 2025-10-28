// MovementPlaceholder.cs
// Placeholder: stand-in for your real movement script
using UnityEngine;

public class MovementPlaceholder : MonoBehaviour
{
    void Start()
    {
        
    }
    public Transform target;
    public float maxSpeed = 20f; // max speed of movement
    public float moveSpeed = 0f; // current movement speed
    void Update()
    {
        // determines if moving forward or back
        float direction = 1f;
        // determines speed, staying 5 units away from target/increasing speed when further away from 5 units
        moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(Vector3.Distance(this.transform.position, target.position) - 5) * 3);
        // reverse direction if too close
        if (Vector3.Distance(this.transform.position, target.position) < 5)
        {
            direction = -1;
        }
        // get target position on XZ plane
        Vector3 targetPosXZ = new Vector3(target.position.x, this.transform.position.y, target.position.z);
        // move this towards/away from target based on moveSpeed
        transform.position = Vector3.MoveTowards(this.transform.position, targetPosXZ, direction * moveSpeed * Time.deltaTime);
        // have this look towards target on XZ plane
        transform.LookAt(targetPosXZ); 
    }
}
