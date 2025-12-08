using System.Collections;
using UnityEngine;
using TMPro;
public class EnemyIdol : Enemy
{
    [Header("Visuals")]
    public Material HurtMat;                 // Material shown when the bag is hit
    public Material IdleMat;                 // Default material when idle

    private Renderer rend;    // Cached renderer for color/material changes
    private Health health;    // Reference to the Health component

    public GameObject itemDrop;
    public Rigidbody itemRigid;

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

        itemDrop = null;
        if (Random.value <= 0.3f)
        {
            itemDrop = base.getRandomItem;
        }
    
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
    private Coroutine damageCoroutine;

    void OnCollisionEnter(Collision collision)
    {
        Touching = collision.gameObject.name;
        damageCoroutine = StartCoroutine(DealDamage(collision));
    }
    private IEnumerator DealDamage(Collision collision)
    {
        Touching = collision.gameObject.name;
        if (Touching == "Player")
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
        yield return null;
    }

    private void OnCollisionExit(Collision collision)
    {
        Touching = " ";
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
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
    }

    // Called whenever this object takes damage
    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
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

    private void Die()
    {
        dropItem(itemDrop);
        Destroy(gameObject);
    }
    private void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null)
        {
            Debug.Log(itemDrop.name + " has no pickup prefab assigned!");
            return;
        }

        // Spawn the item's specific prefab in place of slime
        Vector3 dropPosition = transform.position;
        dropPosition.y += 0.3f;
        Instantiate(itemDrop, dropPosition, Quaternion.identity);
    }
}
