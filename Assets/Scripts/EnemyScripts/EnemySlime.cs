using System.Collections;
using UnityEngine;
using TMPro;
public class EnemySlime : Enemy
{
    [Header("Visuals")]
    public Material HurtMat;                 // Material shown when the bag is hit
    public Material IdleMat;                 // Default material when idle

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 3.0f; // Time before combo resets

    private Renderer rend;    // Cached renderer for color/material changes
    private Health health;    // Reference to the Health component

    private int totalDamageTaken = 0; // Tracks running total of recent hits
    private bool resetDamage;         // Flag for manual reset (unused but left in)
    public GameObject itemDrop;
    public Rigidbody itemRigid;

    // Movement
    private Rigidbody rb;
    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed = 0f;

    // Transparency
    private Renderer objectRenderer;
    private float transparencyValue;

    private string Touching;


    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        transparencyValue = 1f;
        rend = GetComponent<Renderer>();
        health = GetComponent<Health>();
        rend.material = IdleMat;
        rb = GetComponent<Rigidbody>();
        target = GameObject.Find("Player").transform;

        // Listen for damage events from the Health script
        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs");
        Debug.Log(prefabs.Length);
        itemDrop = null;
        if (Random.value <= 0.1f)
        {
            itemDrop = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);
            itemDrop.transform.SetParent(transform);
            itemDrop.transform.localScale = Vector3.one;
            itemRigid = itemDrop.GetComponent<Rigidbody>();
            Component animScript = itemDrop.GetComponent("SimpleGemsAnim");
            if (animScript != null) Destroy(animScript);
            Destroy(itemRigid);
        }
    }

    private void move()
    {
        if (!rb || !target) return;

        float direction = 1f;
        float dist = Vector3.Distance(transform.position, target.position);
        moveSpeed = Mathf.Min(maxSpeed, dist + 0.1f);

        Vector3 targetPosXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosXZ, direction * moveSpeed * Time.deltaTime);

        rb.MovePosition(nextPos);
        transform.LookAt(targetPosXZ);
    }

    private void OnDisable()
    {
        // Stop timers and unsubscribe when disabled
        CancelInvoke();
        if (health != null)
            health.OnDamaged -= HandleDamageTaken;
    }

    private void FixedUpdate()
    {
        move();
        UpdateTransparency();

        // Manual reset if flag is triggered (optional feature)
        if (resetDamage)
        {
            totalDamageTaken = 0;
            resetDamage = false;
        }
        itemDrop.transform.localPosition = Vector3.zero;
    }

    // Called whenever this object takes damage
    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
        // Add this hit’s damage to the running total
        totalDamageTaken += Mathf.RoundToInt(amount);

        // Restart combo reset timer
        CancelInvoke(nameof(ResetCombo));
        Invoke(nameof(ResetCombo), comboWindow);

        // Flash red to show impact
        StartCoroutine(FlashRed());
    }

    private Coroutine damageCoroutine;

    void OnCollisionEnter(Collision collision)
    {
        damageCoroutine = StartCoroutine(DealDamage(collision));
    }
    private IEnumerator DealDamage(Collision collision)
    {
        Touching = collision.gameObject.name;
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            while (true)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(10f, DamageType.Physical, null);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Touching = "";
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    // Quick red flash when hit
    private IEnumerator FlashRed()
    {
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        rend.material = IdleMat;
    }

    // Resets the combo damage total after time runs out
    private void ResetCombo()
    {
        totalDamageTaken = 0;
    }

    private void Die()
    {
        dropItem(itemDrop);
        Destroy(gameObject);
    }
    private void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null)
        {
            Debug.LogError("No pickup prefab assigned!");
            return;
        }

        // Spawn the item's specific prefab in place of slime
        Vector3 dropPosition = transform.position;
        itemDrop = Instantiate(itemDrop, dropPosition, Quaternion.identity);
        itemDrop.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    private void UpdateTransparency()
    {
        transparencyValue = health.currentHP / health.maxHP;
        Color currentColor = objectRenderer.material.color;
        currentColor.a = transparencyValue; // Set the alpha channel
        objectRenderer.material.color = currentColor; // Apply the new color
    }
}
