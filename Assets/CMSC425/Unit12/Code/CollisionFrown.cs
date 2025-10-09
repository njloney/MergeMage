using UnityEngine;
public class CollisionFrown : MonoBehaviour
{
    public Material frown;

    Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        rend.material = frown;

        Destroy(this);
    }
}
