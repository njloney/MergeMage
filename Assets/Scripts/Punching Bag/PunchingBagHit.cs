using System.Collections;
using UnityEngine;

public class PunchingBag : MonoBehaviour
{

    public Material HurtMat;
    public Material IdleMat;
    Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        GameObject otherGameObject = collision.gameObject;
        print("Hit");
        Destroy(otherGameObject);
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = IdleMat;
        
    }
}
