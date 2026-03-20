using System.Collections;
using UnityEngine;

public class GolemBasicAI : MonoBehaviour
{
    public Transform player;
    public GolemMeleeAttack melee;
    public GolemRangedAttack ranged;
    public GolemSpellResponder responder;
    MovementModifiersPlaceholder mods;

    [Header("Combat Ranges")]
    public float meleeRange = 5f;
    public float stopMovingDistance = 3f;

    [Header("Line of Sight")]
    public float detectionRange = 30f;
    public LayerMask losMask;
    public float losLoseDelay = 0.2f;
    public float losGainDelay = 0.05f;
    float losTimer;
    bool losState;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 7f;
    float speedMult = 1f;

    [Header("Cooldowns")]
    public float meleeCooldown = 2f;
    public float rangedCooldown = 1.2f;

    [Header("Wander")]
    public float wanderRadius = 10f;
    public float wanderMoveSpeed = 2f;
    public float wanderMinPause = 1f;
    public float wanderMaxPause = 3f;

    [Header("Animation")]
    public Animator animator;
    public string walkBoolName = "IsWalking";
    public string meleeTriggerName = "Melee";
    public string castTriggerName = "Cast";

    [Tooltip("Seconds from the start of the melee animation when the actual hit should occur.")]
    public float meleeAttackDelay = 0.3f;

    [Tooltip("Seconds from the start of the cast animation when the projectile should be fired.")]
    public float rangedAttackDelay = 0.4f;

    float meleeTimer;
    float rangedTimer;

    Vector3 startPosition;
    Vector3 wanderTarget;
    bool hasWanderTarget;
    float wanderPauseTimer;
    bool lastHasLos;

    bool isAttacking;
    Coroutine attackRoutine;

    void Awake()
    {
        if (responder == null)
            responder = GetComponent<GolemSpellResponder>();
        if (ranged == null)
            ranged = GetComponent<GolemRangedAttack>();
        if (melee == null)
            melee = GetComponent<GolemMeleeAttack>();

        startPosition = transform.position;
        mods = GetComponent<MovementModifiersPlaceholder>();
    }

    void Update()
    {
        if (player == null)
            return;

        float multFromMods = mods != null ? mods.speedMultiplier : 1f;
        bool frozen = multFromMods == 0f;

        if (frozen)
        {
            speedMult = 0f;
            if (animator && !isAttacking)
                animator.SetBool(walkBoolName, false);
            return;
        }

        speedMult = multFromMods;

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
        float groundDist = GroundDistanceToPlayer();
        if (groundDist > detectionRange)
            return false;

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
        if (isAttacking)
        {
            RotateToward(player.position);
            if (animator)
                animator.SetBool(walkBoolName, false);
            return;
        }

        float dist = GroundDistanceToPlayer();

        RotateToward(player.position);

        if (dist > stopMovingDistance)
        {
            MoveToward(player.position, moveSpeed * speedMult);
            if (animator)
                animator.SetBool(walkBoolName, true);
        }
        else
        {
            if (animator)
                animator.SetBool(walkBoolName, false);
        }

        if (dist <= meleeRange && meleeTimer <= 0f)
        {
            Debug.Log("[GolemAI] Melee attack");
            StartMeleeAttack();
            return;
        }

        if (dist > meleeRange && rangedTimer <= 0f)
        {
            if (responder != null && responder.HasStoredSpell)
                Debug.Log("[GolemAI] Ranged (stored spell)");
            else
                Debug.Log("[GolemAI] Ranged (basic)");

            StartRangedAttack();
        }
    }

    void Wander()
    {
        if (isAttacking)
        {
            if (animator)
                animator.SetBool(walkBoolName, false);
            return;
        }

        if (wanderPauseTimer > 0f)
        {
            wanderPauseTimer -= Time.deltaTime;
            if (animator)
                animator.SetBool(walkBoolName, false);
            return;
        }

        if (!hasWanderTarget || Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 basePos = startPosition;
            wanderTarget = new Vector3(basePos.x + circle.x, transform.position.y, basePos.z + circle.y);
            hasWanderTarget = true;
            wanderPauseTimer = Random.Range(wanderMinPause, wanderMaxPause);
            Debug.Log("[GolemAI] New wander target");
        }

        RotateToward(wanderTarget);
        MoveToward(wanderTarget, wanderMoveSpeed * speedMult);

        if (animator)
            Debug.Log("[GolemAI] Setting walk animation to true while Wandering");
            animator.SetBool(walkBoolName, true);

        if (Random.value < 0.01f)
            Debug.Log("[GolemAI] Wandering");
    }

    void MoveToward(Vector3 targetPos, float speed)
    {
        if (speed <= 0f)
            return;

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

    void StartMeleeAttack()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(MeleeAttackRoutine());
    }

    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        meleeTimer = meleeCooldown;

        if (animator)
        {
            animator.SetBool(walkBoolName, false);
            animator.ResetTrigger(castTriggerName);
            animator.SetTrigger(meleeTriggerName);
        }

        float delay = Mathf.Max(0f, meleeAttackDelay);
        yield return new WaitForSeconds(delay);

        if (melee != null)
            melee.DoMelee();

        isAttacking = false;
    }

    void StartRangedAttack()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(RangedAttackRoutine());
    }

    IEnumerator RangedAttackRoutine()
    {
        isAttacking = true;
        rangedTimer = rangedCooldown;

        if (animator)
        {
            animator.SetBool(walkBoolName, false);
            animator.ResetTrigger(meleeTriggerName);
            animator.SetTrigger(castTriggerName);
        }

        float delay = Mathf.Max(0f, rangedAttackDelay);
        yield return new WaitForSeconds(delay);

        if (ranged != null)
            ranged.CastRanged();

        isAttacking = false;
    }

    float GroundDistanceToPlayer()
    {
        if (!player) return Mathf.Infinity;

        Vector3 a = transform.position;
        Vector3 b = player.position;

        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }
}
