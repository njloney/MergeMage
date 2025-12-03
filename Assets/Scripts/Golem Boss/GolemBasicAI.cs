using UnityEngine;

public class GolemBasicAI : MonoBehaviour
{
    public Transform player;
    public GolemMeleeAttack melee;
    public GolemRangedAttack ranged;
    public GolemSpellResponder responder;

    public float meleeRange = 5f;
    public float stopMovingDistance = 3f;

    public float moveSpeed = 5f;
    public float rotationSpeed = 7f;

    public float meleeCooldown = 2f;
    public float rangedCooldown = 1.2f;

    private float meleeTimer = 0f;
    private float rangedTimer = 0f;

    private void Awake()
    {
        if (responder == null)
            responder = GetComponent<GolemSpellResponder>();
        if (ranged == null)
            ranged = GetComponent<GolemRangedAttack>();
        if (melee == null)
            melee = GetComponent<GolemMeleeAttack>();
    }

    private void Update()
    {
        if (player == null)
            return;

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        float d = Vector3.Distance(transform.position, player.position);

        RotateTowardPlayer();

        if (d > stopMovingDistance)
            MoveTowardPlayer();

        if (d <= meleeRange && meleeTimer <= 0f)
        {
            Debug.Log("[GolemAI] Melee attack");
            melee.DoMelee();
            meleeTimer = meleeCooldown;
            return;
        }

        if (d > meleeRange && rangedTimer <= 0f)
        {
            if (responder.HasStoredSpell)
                Debug.Log("[GolemAI] Ranged (stored spell)");
            else
                Debug.Log("[GolemAI] Ranged (basic)");

            ranged.CastRanged();
            rangedTimer = rangedCooldown;
        }
    }

    private void MoveTowardPlayer()
    {
        Vector3 t = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dir = (t - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (Random.value < 0.01f)
            Debug.Log("[GolemAI] Moving");
    }

    private void RotateTowardPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion q = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
    }
}
