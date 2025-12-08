using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyTurret : Enemy
{
    [Header("Visuals")]
    public Material HurtMat;
    public Material IdleMat;
    private Renderer rend;
    private Health health;
    public GameObject itemDrop;
    public Rigidbody itemRigid;

    // Movement
    public Transform target;
    public float maxSpeed = 20f;
    public float moveSpeed = 0f;
    private Coroutine currRoutine;

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
        bodyT = transform;
        bodyRB = bodyT.GetComponent<Rigidbody>();
        armT = transform.Find("Arm");

        // Safely find player
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) target = playerObj.transform;

        health.OnDamaged += HandleDamageTaken;
        health.OnDied += Die;

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/CrystalPrefabs");
        itemDrop = null;

        int attempts = 0;
        while (itemDrop == null && attempts < 100)
        {
            attempts++;
            GameObject candidate = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
            ItemPickup pickup = candidate.GetComponent<ItemPickup>();

            if (pickup != null)
            {
                ItemData data = pickup.itemToGive;

                if (data.castType == SpellCastType.Projectile &&
                   (data.crystalType == CrystalType.Fire ||
                    data.crystalType == CrystalType.Ice ||
                    data.crystalType == CrystalType.Wind ||
                    data.crystalType == CrystalType.Earth ||
                    data.crystalType == CrystalType.Lightning))
                {
                    itemDrop = Instantiate(candidate);
                    itemScript = itemDrop.GetComponent<ItemPickup>();
                }
            }
        }

        if (itemDrop == null)
        {
            Destroy(gameObject);
            return;
        }

        itemDrop.transform.SetParent(armT);
        itemDrop.transform.localScale = new Vector3(.4f, .2f, .4f);
        itemDrop.transform.localPosition = new Vector3(0, 1, 0);

        animScript = itemDrop.GetComponent("SimpleGemsAnim");
        if (animScript != null) Destroy(animScript);

        itemRigid = itemDrop.GetComponent<Rigidbody>();
        if (itemRigid != null) Destroy(itemRigid);

        currRoutine = StartCoroutine(Move());
    }

    private void Update()
    {
        if (itemDrop == null && health.currentHP > 0)
        {
            health.TakeDamage(health.maxHP, DamageType.Physical, null);
        }
    }

    private IEnumerator Move()
    {
        if (!bodyRB || !target) yield break;

        bodyRB.constraints = RigidbodyConstraints.None;
        health.ignoredSourceLayers = ~0;

        float dist = Vector3.Distance(transform.position, target.position);
        while (dist > 15f)
        {
            if (target == null) yield break;
            dist = Vector3.Distance(transform.position, target.position);

            // Calculate movement speed based on distance
            moveSpeed = Mathf.Min(maxSpeed, Mathf.Abs(dist - 7f) * 3f);

            // Torque Movement
            Vector3 direction = (target.position - bodyT.position).normalized;
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, direction);
            bodyRB.AddTorque(torqueAxis * 10f);

            yield return new WaitForFixedUpdate();
        }

        bodyRB.angularVelocity = Vector3.zero;
        bodyRB.linearVelocity = Vector3.zero;
        bodyRB.constraints = RigidbodyConstraints.FreezeAll;
        health.ignoredSourceLayers = 0; // Vulnerable again
        currRoutine = StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        Vector3 endPos = bodyT.position + new Vector3(0, 0.5f, 0);

        while (armT.position.y < endPos.y)
        {
            float newY = armT.position.y + 0.5f * Time.deltaTime;
            armT.position = new Vector3(endPos.x, newY, endPos.z);
            armT.rotation = Quaternion.identity;
            yield return null;
        }
        armT.position = endPos;
        currRoutine = StartCoroutine(Attack());
    }

    private float aimUpOffset = 1.0f;
    private IEnumerator Attack()
    {
        if (target == null) yield break;

        float dist = Vector3.Distance(transform.position, target.position);

        while (dist > 2f && dist < 20f)
        {
            yield return new WaitForSeconds(3f);

            if (target == null || itemDrop == null) break;

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

        currRoutine = StartCoroutine(Hide());
    }

    private IEnumerator Hide()
    {
        Vector3 endPos = bodyT.position;

        while (armT.position.y > endPos.y)
        {
            float newY = armT.position.y - 0.5f * Time.deltaTime;
            armT.position = new Vector3(endPos.x, newY, endPos.z);
            armT.rotation = Quaternion.identity;
            yield return null;
        }
        armT.position = endPos;
        currRoutine = StartCoroutine(Move());
    }

    private Coroutine damageCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DealDamage(collision));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

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

    private void HandleDamageTaken(float amount, DamageType type, Object source)
    {
        StartCoroutine(FlashRed());
        // Hide immediately when hit
        if (currRoutine != null)
        {
            StopCoroutine(currRoutine);
            currRoutine = StartCoroutine(Hide());
        }
    }

    private IEnumerator FlashRed()
    {
        rend.material = HurtMat;
        yield return new WaitForSeconds(0.2f);
        rend.material = IdleMat;
    }

    private void Die()
    {
        if (itemDrop != null) dropItem(itemDrop);
        Destroy(gameObject);
    }

    private void dropItem(GameObject itemDrop)
    {
        if (itemDrop == null) return;

        Vector3 dropPosition = transform.position;
        GameObject dropped = Instantiate(itemDrop, dropPosition, Quaternion.identity);
        dropped.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        if (dropped.GetComponent<Rigidbody>() == null) dropped.AddComponent<Rigidbody>();
    }
}