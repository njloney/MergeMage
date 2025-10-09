using System.Collections;
using UnityEngine;
using TMPro;
public class PunchingBag : MonoBehaviour
{
    int totalDamageTaken = 0;
    [SerializeField] private float comboWindow = 2.0f;
    bool resetDamage;
    public Material HurtMat;
    public Material IdleMat;
    public TextMeshProUGUI damageTakenText;
    Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        GameObject otherGameObject = collision.gameObject;
        if (otherGameObject.tag == "playerAttackProjectile")
        {
            totalDamageTaken += otherGameObject.GetComponent<Stats>().attackPower;
            // Restart the countdown each hit
            CancelInvoke(nameof(ResetCombo));
            Invoke(nameof(ResetCombo), comboWindow);
        }
        print("Hit");
        Destroy(otherGameObject);
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = IdleMat;
    }
    private void ResetCombo()
    {
        totalDamageTaken = 0;
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void Update()
    {
        damageTakenText.text = totalDamageTaken.ToString();
        if (resetDamage)
        {
            totalDamageTaken = 0;
            resetDamage = false;
        }

    }
}
