using System.Collections;
using UnityEngine;
using TMPro;
public class EnemyTurret : Enemy
{
    [Header("Visuals")]
    public Material HurtMat;                 // Material shown when the bag is hit
    public Material IdleMat;                 // Default material when idle
    private Renderer rend;    // Cached renderer for color/material changes
    private Health health;    // Reference to the Health component
    [Header("Crystal Drop")]
    public GameObject itemDrop;
    private ItemPickup itemScript;
    public Rigidbody itemRigid;

    [Header("Movement")]
    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed = 0f;
    private Rigidbody bodyRB;
    private Transform bodyT;
    private Transform armT;


    private Coroutine currRoutine;
    private Component animScript;

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
        itemDrop = null;
        while (itemDrop == null)
        {
            itemDrop = Instantiate(base.getRandomCrystal());
            itemScript = (ItemPickup)itemDrop.GetComponent("ItemPickup");
            ItemData item = itemScript.itemToGive;
            if (item == null || item.castType != SpellCastType.Projectile)
            {
                itemDrop = null;
            }
        }
        itemDrop.transform.SetParent(armT);
        itemDrop.transform.localScale = new Vector3(.4f, .2f, .4f);
        itemDrop.transform.localPosition = new Vector3(0, 1, 0);
        itemRigid = itemDrop.GetComponent<Rigidbody>();
        
        Destroy(itemRigid);

        currRoutine = StartCoroutine(Move());
    }
    // Move towards until within shooting range
    private IEnumerator Move()
    {
        Debug.Log("Move");
        if (!bodyRB || !target) Debug.LogError("Can't find rigidBody or Target");
        bodyRB.constraints = RigidbodyConstraints.None;
        health.ignoredSourceLayers = ~0;

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
        health.ignoredSourceLayers = 0;
        currRoutine = StartCoroutine(Show());
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

    private void Update()
    {
        if (itemDrop == null)
        {
            Die();
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

    private void Die()
    {
        base.dropItem(itemDrop);
        Destroy(gameObject);
    }
}