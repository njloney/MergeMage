using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WindPullField : MonoBehaviour
{
    [SerializeField] private float radius = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float pullForce = 40f;
    [SerializeField] private float warmupTime = 1f;
    [SerializeField] private float controllerPullSpeed = 10f;
    [SerializeField] private LayerMask playerMask = ~0;
    [SerializeField] private float pullSoundInterval = 0.4f;
    private float nextPullSoundTime;

    private Transform center;
    private SphereCollider col;
    private float timer;
    private float fixedY;

    public void Init(Transform centerTransform)
    {
        center = centerTransform;
        fixedY = transform.position.y;
        transform.rotation = Quaternion.identity;
    }

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (center != null)
        {
            Vector3 p = center.position;
            p.y = fixedY;
            transform.position = p;
        }

        transform.rotation = Quaternion.identity;

        if (timer >= warmupTime)
            DoPull();
    }

    private void DoPull()
    {
        Vector3 origin = transform.position;

        var hits = Physics.OverlapSphere(
            origin,
            radius,
            playerMask,
            QueryTriggerInteraction.Ignore
        );

        bool didPull = false;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;

            Vector3 targetPos = origin;

            Rigidbody rb = h.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                Vector3 dir = targetPos - rb.position;
                float dist = dir.magnitude;
                if (dist < 0.01f) continue;

                dir /= dist;
                rb.AddForce(dir * pullForce, ForceMode.Acceleration);
                didPull = true;
                continue;
            }

            var controller = h.GetComponentInParent<CharacterController>();
            if (controller != null)
            {
                Vector3 pos = controller.transform.position;
                Vector3 dir = targetPos - pos;
                dir.y = 0f;
                float dist = dir.magnitude;
                if (dist < 0.01f) continue;

                dir /= Mathf.Max(dist, 0.01f);
                Vector3 move = dir * controllerPullSpeed * Time.deltaTime;
                controller.Move(move);
                didPull = true;
            }
        }

        if (didPull && Time.time >= nextPullSoundTime)
        {
            var audio = GetComponent<SpellImpactAudio>();
            if (audio != null)
                audio.PlayImpactSound();

            nextPullSoundTime = Time.time + pullSoundInterval;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
