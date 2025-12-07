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

    private BoxCollider boxCol;
    private float pathLength;
    private float pathWidth;
    private bool initialized;

    public void Init(Transform boss, Transform target, float maxLength, float width)
    {
        Vector3 a = boss.position;
        Vector3 b = target.position;
        a.y = b.y;

        Vector3 dir = b - a;
        if (dir.sqrMagnitude < 0.01f)
            dir = boss.forward;
        else
            dir.Normalize();

        pathLength = maxLength > 0f ? maxLength : defaultLength;
        pathWidth = width > 0f ? width : defaultWidth;

        transform.position = a + dir * (pathLength * 0.5f);
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

            // Keep whatever local rotation the prefab has (so the quad stays flat).
            // Just recenter XZ and rescale to match width/length.
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

            SpawnSpikeSegment(worldPos);
            DoSegmentHit(worldPos);

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

    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<BoxCollider>(out var bc)) return;

        Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.25f);
        Gizmos.matrix = bc.transform.localToWorldMatrix;
        Gizmos.DrawCube(bc.center, bc.size);
    }
}
