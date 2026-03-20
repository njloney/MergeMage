using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Health))]
public class PunchingBag : MonoBehaviour
{
    [Header("Visuals")]
    public Material HurtMat;
    public Material IdleMat;
    public TextMeshProUGUI damageTakenText;

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 3.0f;

    [Header("Collision Filtering")]
    [SerializeField] private LayerMask ignoredDamageLayers;

    private Renderer rend;
    private Health health;

    private int totalDamageTaken = 0;
    private bool resetDamage;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        health = GetComponent<Health>();

        health.OnDamaged += HandleDamageTaken;
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (health != null)
            health.OnDamaged -= HandleDamageTaken;
    }

    private void Update()
    {
        if (damageTakenText != null)
        {
            damageTakenText.SetText(totalDamageTaken.ToString());
        }

        if (resetDamage)
        {
            totalDamageTaken = 0;
            resetDamage = false;
        }
    }

    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
        if (source is Component comp)
        {
            Transform t = comp.transform;
            while (t != null)
            {
                if (((1 << t.gameObject.layer) & ignoredDamageLayers) != 0)
                    return;

                t = t.parent;
            }
        }

        totalDamageTaken += Mathf.RoundToInt(amount);

        CancelInvoke(nameof(ResetCombo));
        Invoke(nameof(ResetCombo), comboWindow);

        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        rend.material = IdleMat;
    }

    private void ResetCombo()
    {
        totalDamageTaken = 0;
    }
}
