using UnityEngine;

public class GolemBasicAI : MonoBehaviour
{
    public Transform player;
    public GolemMeleeAttack melee;
    public GolemRangedAttack ranged;
    public GolemSpellResponder responder;

    public float meleeRange = 5f;
    public float stopMovingDistance = 3f;

    public float detectionRange = 30f;
    public LayerMask losMask;
    public float losLoseDelay = 0.2f;
    public float losGainDelay = 0.05f;
    float losTimer;
    bool losState;


    public float moveSpeed = 5f;
    public float rotationSpeed = 7f;

    public float meleeCooldown = 2f;
    public float rangedCooldown = 1.2f;

    public float wanderRadius = 10f;
    public float wanderMoveSpeed = 2f;
    public float wanderMinPause = 1f;
    public float wanderMaxPause = 3f;

    float meleeTimer;
    float rangedTimer;

    Vector3 startPosition;
    Vector3 wanderTarget;
    bool hasWanderTarget;
    float wanderPauseTimer;
    bool lastHasLos;

    void Awake()
    {
        if (responder == null)
            responder = GetComponent<GolemSpellResponder>();
        if (ranged == null)
            ranged = GetComponent<GolemRangedAttack>();
        if (melee == null)
            melee = GetComponent<GolemMeleeAttack>();

        startPosition = transform.position;
    }

    void Update()
    {
        if (player == null)
            return;

        float dt = Time.deltaTime;
        meleeTimer -= dt;
        rangedTimer -= dt;

        bool rawLos = HasLineOfSight();

        if (rawLos)
            losTimer += dt;
        else
            losTimer -= dt;

        losTimer = Mathf.Clamp(losTimer, -losLoseDelay, losGainDelay);

        bool newLosState = losState;
        if (!losState && losTimer >= losGainDelay)
            newLosState = true;
        else if (losState && losTimer <= -losLoseDelay)
            newLosState = false;

        if (newLosState && !losState)
            Debug.Log("[GolemAI] Gained line of sight");
        if (!newLosState && losState)
            Debug.Log("[GolemAI] Lost line of sight");

        losState = newLosState;

        if (losState)
            ChaseAndAttack();
        else
            Wander();
    }

    bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        Vector3 center = player.position;
        center.y += 0.9f;

        Debug.DrawLine(origin, center, Color.red);

        Vector3 dir = center - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, losMask))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return false;
    }


    void ChaseAndAttack()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        RotateToward(player.position);

        if (dist > stopMovingDistance)
            MoveToward(player.position, moveSpeed);

        if (dist <= meleeRange && meleeTimer <= 0f)
        {
            Debug.Log("[GolemAI] Melee attack");
            melee.DoMelee();
            meleeTimer = meleeCooldown;
            return;
        }

        if (dist > meleeRange && rangedTimer <= 0f)
        {
            if (responder != null && responder.HasStoredSpell)
                Debug.Log("[GolemAI] Ranged (stored spell)");
            else
                Debug.Log("[GolemAI] Ranged (basic)");

            ranged.CastRanged();
            rangedTimer = rangedCooldown;
        }
    }

    void Wander()
    {
        if (wanderPauseTimer > 0f)
        {
            wanderPauseTimer -= Time.deltaTime;
            return;
        }

        if (!hasWanderTarget || Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 basePos = startPosition;
            wanderTarget = new Vector3(basePos.x + circle.x, transform.position.y, basePos.z + circle.y);
            hasWanderTarget = true;
            wanderPauseTimer = 0f;
            Debug.Log("[GolemAI] New wander target");
        }

        RotateToward(wanderTarget);
        MoveToward(wanderTarget, wanderMoveSpeed);

        if (Random.value < 0.01f)
            Debug.Log("[GolemAI] Wandering");
    }

    void MoveToward(Vector3 targetPos, float speed)
    {
        Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        Vector3 dir = (flatTarget - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    void RotateToward(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return;
        Quaternion q = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
    }
}
