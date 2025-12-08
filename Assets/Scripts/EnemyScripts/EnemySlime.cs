using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Health))]
public class EnemySlime : Enemy
{
    [Header("Hopping Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float forwardForce = 3f;
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float jumpChargeTime = 0.5f;
    [SerializeField] private float groundedDrag = 5f;
    [SerializeField] private float airDrag = 0f;

    // [FIX 1] Re-added maxSpeed. 
    // We treat 1.0 as "100% speed". Slow/Freeze scripts will modify this value.
    [HideInInspector] public float maxSpeed = 1f;

    [Header("Visuals")]
    public Material HurtMat;
    public Material IdleMat;
    [SerializeField] private Transform modelTransform;

    [Header("Loot")]
    public GameObject itemDrop;

    // Components
    private Rigidbody rb;
    private Health health;
    private Renderer rend;

    // [FIX 2] Changed from private to public so Blind.cs can access it
    public Transform target;

    // State
    private float jumpTimer;
    private bool isGrounded;
    private bool isCharging;

    // Damage Flash
    private Coroutine damageCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        rend = GetComponent<Renderer>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        if (IdleMat != null) rend.material = IdleMat;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) target = playerObj.transform;

        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;

        InitializeLoot();

        jumpTimer = jumpInterval;
    }

    private void Update()
    {
        UpdateTransparency();
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        CheckGrounded();

        if (isGrounded && !isCharging)
        {
            rb.linearDamping = groundedDrag;
            RotateTowardsPlayer();

            // Only countdown jump timer if we aren't frozen (maxSpeed > 0)
            if (maxSpeed > 0.01f)
            {
                jumpTimer -= Time.fixedDeltaTime;
                if (jumpTimer <= 0f)
                {
                    StartCoroutine(PerformHop());
                }
            }
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.fixedDeltaTime);
        }
    }

    private IEnumerator PerformHop()
    {
        isCharging = true;

        // Squash Animation
        Vector3 originalScale = transform.localScale;
        Vector3 squashScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.7f, originalScale.z * 1.2f);

        float elapsed = 0f;
        while (elapsed < jumpChargeTime)
        {
            // If we get frozen mid-charge, stop
            if (maxSpeed <= 0.01f)
            {
                transform.localScale = originalScale;
                isCharging = false;
                yield break;
            }

            transform.localScale = Vector3.Lerp(originalScale, squashScale, elapsed / jumpChargeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        Vector3 dir = (target.position - transform.position).normalized;

        // [FIX 3] Apply maxSpeed as a multiplier. 
        // If maxSpeed is 0.5 (Slowed), jump is weaker. If 0 (Frozen), jump force is 0.
        Vector3 finalForce = (Vector3.up * jumpForce + dir * forwardForce) * maxSpeed;

        rb.AddForce(finalForce, ForceMode.Impulse);

        isCharging = false;
        jumpTimer = jumpInterval;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.5f);
    }

    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        if (HurtMat != null) rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        if (IdleMat != null) rend.material = IdleMat;
    }

    private void Die()
    {
        if (itemDrop != null) DropItem();
        Destroy(gameObject);
    }

    private void InitializeLoot()
    {
        if (itemDrop == null)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs");
            if (prefabs.Length > 0 && Random.value <= 0.25f)
            {
                itemDrop = prefabs[Random.Range(0, prefabs.Length)];
            }
        }
    }

    private void DropItem()
    {
        if (itemDrop == null) return;

        GameObject dropped = Instantiate(itemDrop, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        dropped.transform.localScale = Vector3.one * 0.5f;

        if (!dropped.GetComponent<Rigidbody>()) dropped.AddComponent<Rigidbody>();
    }

    private void UpdateTransparency()
    {
        if (rend == null) return;

        float alpha = Mathf.Clamp01(health.currentHP / health.maxHP);
        Color c = rend.material.color;
        c.a = Mathf.Max(0.3f, alpha);
        rend.material.color = c;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHp = collision.gameObject.GetComponent<Health>();
            if (playerHp != null)
            {
                playerHp.TakeDamage(10f, DamageType.Physical, this);

                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDir * 5f, ForceMode.Impulse);
                }
            }
        }
    }
}