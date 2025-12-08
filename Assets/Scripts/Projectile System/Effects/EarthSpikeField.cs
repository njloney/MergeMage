using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EarthSpikeField : MonoBehaviour
{
    [Header("Path Shape")]
    [SerializeField] private float defaultLength = 10f;
    [SerializeField] private float defaultWidth = 3f;
    [SerializeField] private int segments = 6;

    [Header("Timing")]
    [SerializeField] private float telegraphTime = 1.0f;
    [SerializeField] private float segmentDelay = 0.2f;
    [SerializeField] private float lifeAfterLastSpike = 1.0f;

    [Header("Damage")]
    [SerializeField] private float damagePerSegment = 15f;
    [SerializeField] private DamageType damageType = DamageType.Earth;
    [SerializeField] private float segmentHitRadius = 1.5f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visuals")]
    [SerializeField] private GameObject telegraphVisual;
    [SerializeField] private GameObject spikeSegmentPrefab;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundRaycastUp = 5f;
    [SerializeField] private float groundRaycastDown = 20f;

    private BoxCollider boxCol;
    private float pathLength;
    private float pathWidth;
    private bool initialized;

    public void Init(Transform boss, Transform target, float maxLength, float width)
    {
        // Project only the boss to ground to get base height
        Vector3 bossGround = ProjectToGround(boss.position);

        // Direction in XZ from boss to player, ignore Y entirely
        Vector3 dir = target.position - boss.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            dir = boss.forward;
            dir.y = 0f;
        }
        dir.Normalize();

        pathLength = maxLength > 0f ? maxLength : defaultLength;
        pathWidth = width > 0f ? width : defaultWidth;

        // Center the box along this direction, at boss ground height
        Vector3 center = bossGround + dir * (pathLength * 0.5f);

        transform.position = center;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        UpdateColliderAndTelegraph();

        if (!initialized)
        {
            initialized = true;
            StartCoroutine(DoSequence());
        }
    }

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        if (!initialized)
        {
            pathLength = defaultLength;
            pathWidth = defaultWidth;
            UpdateColliderAndTelegraph();
            StartCoroutine(DoSequence());
        }
    }

    private void UpdateColliderAndTelegraph()
    {
        boxCol.center = Vector3.zero;
        boxCol.size = new Vector3(pathWidth, 1f, pathLength);

        if (telegraphVisual != null)
        {
            var t = telegraphVisual.transform;
            t.localPosition = new Vector3(0f, t.localPosition.y, 0f);

            Vector3 s = t.localScale;
            t.localScale = new Vector3(pathWidth, s.y, pathLength);

            telegraphVisual.SetActive(true);
        }
    }

    private IEnumerator DoSequence()
    {
        if (telegraphVisual != null)
            telegraphVisual.SetActive(true);

        yield return new WaitForSeconds(telegraphTime);

        float segmentSpacing = pathLength / Mathf.Max(segments, 1);
        float halfLen = pathLength * 0.5f;

        for (int i = 0; i < segments; i++)
        {
            float localZ = -halfLen + (i + 0.5f) * segmentSpacing;
            Vector3 localPos = new Vector3(0f, 0f, localZ);
            Vector3 worldPos = transform.TransformPoint(localPos);

            // Snap spike and damage center to ground
            Vector3 spikePos = ProjectToGround(worldPos);

            SpawnSpikeSegment(spikePos);
            DoSegmentHit(spikePos);

            yield return new WaitForSeconds(segmentDelay);
        }

        yield return new WaitForSeconds(lifeAfterLastSpike);
        Destroy(gameObject);
    }

    private void SpawnSpikeSegment(Vector3 worldPos)
    {
        if (spikeSegmentPrefab == null)
            return;

        Instantiate(spikeSegmentPrefab, worldPos, transform.rotation);
    }

    private void DoSegmentHit(Vector3 worldPos)
    {
        var hits = Physics.OverlapSphere(worldPos, segmentHitRadius, hitMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            var hp = h.GetComponentInParent<Health>();
            if (hp != null && damagePerSegment > 0f)
            {
                hp.TakeDamage(damagePerSegment, damageType, this);
            }
        }
    }

    private Vector3 ProjectToGround(Vector3 pos)
    {
        Vector3 start = pos + Vector3.up * groundRaycastUp;
        float dist = groundRaycastUp + groundRaycastDown;

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, dist, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<BoxCollider>(out var bc)) return;

        Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.25f);
        Gizmos.matrix = bc.transform.localToWorldMatrix;
        Gizmos.DrawCube(bc.center, bc.size);
    }
}
