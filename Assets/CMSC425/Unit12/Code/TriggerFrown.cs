using UnityEngine;
public class TriggerFrown : MonoBehaviour
{
    public Material frown;

    Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }
    private void OnTriggerExit()
    {
        rend.material = frown;
    }
}
