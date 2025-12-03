using System.Collections;
using UnityEngine;
using TMPro;
public class EnemyIdol : Enemy
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
    //public GameObject itemDrop;
    //public Rigidbody itemRigid;

    // Movement
    private Rigidbody rb;
    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed = 0f;

    private string Touching;
    private Coroutine currRoutine;

    private float direction;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        health = GetComponent<Health>();
        rend.material = IdleMat;
        rb = GetComponent<Rigidbody>();
        target = GameObject.Find("Player").transform;

        // Listen for damage events from the Health script
        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;

        /*GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs");
        Debug.Log(prefabs.Length);
        itemDrop = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);
        itemDrop.transform.SetParent(transform);
        itemDrop.transform.localScale = Vector3.one;
        itemRigid = itemDrop.GetComponent<Rigidbody>();
        Destroy(itemRigid);*/
        //itemDrop = ItemData.

        //start Movement
        direction = 1f;
        currRoutine = StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        direction = 1f;
        Debug.Log("Move");
        if (!rb || !target) Debug.LogError("Can't find rigidBody or Target");

        float dist = Vector3.Distance(transform.position, target.position);
        while (dist > 5f)
        {
            dist = Vector3.Distance(transform.position, target.position);
            moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 4f) * 3f);

            MoveTowards(direction, new Vector3(target.transform.position.x, target.transform.position.y + 2, target.transform.position.z));
            yield return new WaitForFixedUpdate();
        }
        currRoutine = StartCoroutine(Attack());
        yield return new WaitForFixedUpdate();
    }

    private void MoveTowards(float direction, Vector3 targetPos)
    {
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, direction * moveSpeed * Time.deltaTime);

        rb.MovePosition(nextPos);
    }


    private IEnumerator Attack()
    {
        Debug.Log("Attack");
        yield return new WaitForSeconds(3f);
        while(Touching != "Player")
        {
            moveSpeed = 5f;
            MoveTowards(direction, target.position);
            yield return new WaitForFixedUpdate();
        }
        currRoutine = StartCoroutine(Run());
        yield return new WaitForFixedUpdate();
    }

    void OnCollisionEnter(Collision collision)
    {
        Touching = collision.gameObject.name;
    }

    void OnCollisionExit(Collision collision)
    {
        Touching = " ";
    }

    private void OnDisable()
    {
        // Stop timers and unsubscribe when disabled
        CancelInvoke();
        if (health != null)
            health.OnDamaged -= HandleDamageTaken;
    }

    private void Update()
    {
        if (direction == -1f)
        {
            transform.LookAt(2 * transform.position - target.position);
        } else
        {
            transform.LookAt(target.position);
        }
        // Safely update UI text if assigned
        /*if (damageTakenText != null)
        {
            // Use SetText to avoid GC from ToString allocations in tight loops
            damageTakenText.SetText(totalDamageTaken.ToString());
        }*/

        // Manual reset if flag is triggered (optional feature)
        if (resetDamage)
        {
            totalDamageTaken = 0;
            resetDamage = false;
        }
        //itemDrop.transform.localPosition = Vector3.zero;
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
        if (currRoutine != null)
        {
            StopCoroutine(currRoutine);
            currRoutine = StartCoroutine(Run());
        }
    }

    // Quick red flash when hit
    private IEnumerator FlashRed()
    {
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        rend.material = IdleMat;
    }

    private IEnumerator Run()
    {
        direction = -1f;
        Debug.Log("Run");
        float dist = Vector3.Distance(transform.position, target.position);
        while (dist < 20f)
        {
            dist = Vector3.Distance(transform.position, target.position);
            moveSpeed = 3f;
            MoveTowards(direction, target.position);
            yield return new WaitForFixedUpdate();
        }
        currRoutine = StartCoroutine(Move());
        yield return new WaitForFixedUpdate();
    }

    // Resets the combo damage total after time runs out
    private void ResetCombo()
    {
        totalDamageTaken = 0;
    }

    private void Die()
    {
        //dropItem(itemDrop);
        Destroy(gameObject);
    }
    /*private void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null)
        {
            Debug.LogError(itemDrop.name + " has no pickup prefab assigned!");
            return;
        }

        // Spawn the item's specific prefab in place of slime
        Vector3 dropPosition = transform.position;
        dropPosition.y += 0.3f;
        Instantiate(itemDrop, dropPosition, Quaternion.identity);
    }*/
}
