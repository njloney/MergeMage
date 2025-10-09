using UnityEngine;

public class ReportCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        print($"{gameObject.name} collided with {collision.gameObject.name}");
    }

    private void OnTriggerEnter(Collider collider)
    {
        print($"{gameObject.name} triggered by {collider.gameObject.name}");
    }
}
