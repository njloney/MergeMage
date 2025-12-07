using System.Collections;
using UnityEngine;
using TMPro;
public class EnemyTurret : Enemy
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

    private string Touching;
    private Coroutine currRoutine;

    private float direction;

    private Rigidbody bodyRB;

    private Transform bodyT;
    private Transform armT;
    private Component animScript;

    private ItemPickup itemScript;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        health = GetComponent<Health>();
        rend.material = IdleMat;
        bodyT = transform;//.Find("Body");
        bodyRB = bodyT.GetComponent<Rigidbody>();
        armT = transform.Find("Arm");
        target = GameObject.Find("Player").transform;
        

        // Listen for damage events from the Health script
        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs");
        Debug.Log(prefabs.Length);
        itemDrop = null;
        while (itemDrop == null)
        {
            itemDrop = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);
            itemScript = (ItemPickup)itemDrop.GetComponent("ItemPickup");
            ItemData item = itemScript.itemToGive;
            if (item.castType != SpellCastType.Projectile)
            {
                itemDrop = null;
            }
        }
        itemDrop.transform.SetParent(armT);
        itemDrop.transform.localScale = new Vector3(.4f, .2f, .4f);
        itemDrop.transform.localPosition = new Vector3(0, 1, 0);
        animScript = itemDrop.GetComponent("SimpleGemsAnim");
        if (animScript != null) Destroy(animScript);
        itemRigid = itemDrop.GetComponent<Rigidbody>();
        itemScript = (ItemPickup)itemDrop.GetComponent("ItemPickup");
        
        Destroy(itemRigid);
        //itemDrop = ItemData.

        currRoutine = StartCoroutine(Move());
    }
    // Move towards until within shooting range
    public float moveTorque = 10f;   // Rolling force
    private IEnumerator Move()
    {
        Debug.Log("Move");
        if (!bodyRB || !target) Debug.LogError("Can't find rigidBody or Target");
        bodyRB.constraints = RigidbodyConstraints.None;

        float dist = Vector3.Distance(transform.position, target.position);
        while (dist > 5f)
        {
            dist = Vector3.Distance(transform.position, target.position);
            moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 7f) * 3f);

            // Compute direction to player
            Vector3 direction = (target.position - bodyT.position).normalized;

            // Compute torque axis (perpendicular to forward direction)
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, direction);

            // Apply torque
            bodyRB.AddTorque(torqueAxis * 10f);


            yield return new WaitForFixedUpdate();
        }
        bodyRB.angularVelocity = Vector3.zero;
        bodyRB.linearVelocity = Vector3.zero;
        bodyRB.constraints = RigidbodyConstraints.FreezeAll;
        currRoutine = StartCoroutine(Show());
    }

    private void move()
    {
        Vector3 lookDirection = target.position - armT.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            armT.rotation = Quaternion.Slerp(armT.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    private IEnumerator Show()
    {
        Vector3 startPos = armT.position;
        Vector3 endPos = bodyT.position + new Vector3(0, 0.5f, 0);

        while (armT.position.y < endPos.y)
        {
            // Move upward by riseSpeed * deltaTime
            float newY = armT.position.y + 0.5f * Time.deltaTime;
            armT.position = new Vector3(endPos.x, newY, endPos.z);
            armT.rotation = Quaternion.identity;
            yield return null;
        }
        armT.position = endPos;
        currRoutine = StartCoroutine(Attack());
    }

    private void moveArm(float direction, Vector3 endPos)
    {
        // Move upward by riseSpeed * deltaTime
        float newY = armT.position.y + direction * 0.5f * Time.deltaTime;
        armT.position = new Vector3(endPos.x, newY, endPos.z);
        armT.rotation = Quaternion.identity;
    }

    private IEnumerator Hide()
    {
        Vector3 startPos = armT.position;
        Vector3 endPos = bodyT.position;
        Debug.Log("hide");

        while (armT.position.y > endPos.y)
        {
            // Move downward by riseSpeed * deltaTime
            float newY = armT.position.y - 0.5f * Time.deltaTime;
            armT.position = new Vector3(endPos.x, newY, endPos.z);
            armT.rotation = Quaternion.identity;
            yield return null;
        }
        armT.position = endPos;
        currRoutine = StartCoroutine(Move());
    }

    private float aimUpOffset = 1.0f;
    private IEnumerator Attack()
    {
        Debug.Log("Attack");
        float dist = Vector3.Distance(transform.position, target.position);
        while (dist > 2f && dist < 10f)
        {
            yield return new WaitForSeconds(3f);
            dist = Vector3.Distance(transform.position, target.position);
            ItemData item = itemScript.itemToGive;
            Transform firePoint = itemDrop.transform;
            Vector3 firePosition = firePoint.position + new Vector3(0f, 1f, 0f);

            var stats = item.projectileStats;

            if (stats.castType == SpellCastType.Beam || stats.isBeamSpell)
                yield break;

            if (target != null)
            {
                Vector3 targetPos = target.position;
                targetPos.y += aimUpOffset;

                Vector3 toPlayer = targetPos - firePosition;
                if (toPlayer.sqrMagnitude > 0.0001f)
                    firePoint.rotation = Quaternion.LookRotation(toPlayer.normalized);
            }

            GameObject go = Instantiate(item.spellPrefab, firePosition, firePoint.rotation);

            if (go.TryGetComponent<ProjectileConfig>(out var cfg))
            {
                cfg.stats = stats;
                cfg.owner = this;
            }

            go.SetActive(true);
        }
        Debug.Log("check here");
        currRoutine = StartCoroutine(Hide());
    }
    private Coroutine damageCoroutine;
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

    private void OnDisable()
    {
        // Stop timers and unsubscribe when disabled
        CancelInvoke();
        if (health != null)
            health.OnDamaged -= HandleDamageTaken;
    }

    private void Update()
    {
        if (itemDrop == null)
        {
            Die();
        }
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
            currRoutine = StartCoroutine(Hide());
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
        if (itemDrop != null) dropItem(itemDrop);
        Destroy(gameObject);
    }
    private void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null)
        {
            Debug.LogError(itemDrop.name + " has no pickup prefab assigned!");
            return;
        }

        // Spawn the item's specific prefab in place of slime
        Vector3 dropPosition = transform.position;
        itemDrop = Instantiate(itemDrop, dropPosition, Quaternion.identity);
        itemDrop.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }


   /*private IEnumerator Move()
    {
        direction = 1f;
        Debug.Log("Move");
        if (!bodyT || !target) Debug.LogError("Can't find rigidBody or Target");

        float dist = Vector3.Distance(transform.position, target.position);
        while (dist > 5f)
        {
            moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 4f) * 3f);

            Vector3 force = direction * (target.position - transform.position).normalized * moveSpeed;

            Velocity(direction, force);
            yield return new WaitForFixedUpdate();
        }
        currRoutine = StartCoroutine(Attack());
        yield return new WaitForFixedUpdate();
    }

    private void Velocity(float direction, Vector3 targetPos)
    {
        bodyRB.AddForce(targetPos);
    }


    private IEnumerator Attack()
    {
        Debug.Log("Attack");
        yield return new WaitForSeconds(3f);
        /*while( Touching != "Player")
        {
            moveSpeed = 5f;
            MoveTowards(direction, target.position);
            yield return new WaitForFixedUpdate();
        }
        currRoutine = StartCoroutine(Move());
        yield return new WaitForFixedUpdate();
    }
    private Coroutine damageCoroutine;

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
            Velocity(direction, target.position);
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