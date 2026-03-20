using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Effects/Blind")]
public class BlindEffect : StatusEffect
{
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderSpeed = 7f;
    [SerializeField] private bool randomizeYawOnApply = true;

    readonly Dictionary<EnemySlime, BlindTargetAnchor> slimeAnchors = new();
    readonly Dictionary<EnemyIdol, BlindTargetAnchor> idolAnchors = new();
    readonly Dictionary<GolemBasicAI, BlindTargetAnchor> bossAnchors = new();

    public override void OnApply(StatusController target, ref EffectRuntime runtime)
    {
        var slime = target.GetComponent<EnemySlime>();
        if (slime != null)
        {
            if (!slimeAnchors.TryGetValue(slime, out var anchor) || anchor == null)
            {
                var go = new GameObject("BlindAnchor_Slime");
                anchor = go.AddComponent<BlindTargetAnchor>();
                anchor.owner = slime.transform;
                anchor.originalTarget = slime.target;
                anchor.transform.position = GetInitialOffsetPosition(slime.transform.position);
                slime.target = anchor.transform;
                slimeAnchors[slime] = anchor;
            }
        }

        var idol = target.GetComponent<EnemyIdol>();
        if (idol != null)
        {
            if (!idolAnchors.TryGetValue(idol, out var anchor) || anchor == null)
            {
                var go = new GameObject("BlindAnchor_Idol");
                anchor = go.AddComponent<BlindTargetAnchor>();
                anchor.owner = idol.transform;
                anchor.originalTarget = idol.target;
                anchor.transform.position = GetInitialOffsetPosition(idol.transform.position);
                idol.target = anchor.transform;
                idolAnchors[idol] = anchor;
            }
        }

        var boss = target.GetComponent<GolemBasicAI>();
        if (boss != null)
        {
            if (!bossAnchors.TryGetValue(boss, out var anchor) || anchor == null)
            {
                var go = new GameObject("BlindAnchor_Boss");
                anchor = go.AddComponent<BlindTargetAnchor>();
                anchor.owner = boss.transform;
                anchor.originalTarget = boss.player;
                anchor.transform.position = GetInitialOffsetPosition(boss.transform.position);
                boss.player = anchor.transform;
                bossAnchors[boss] = anchor;
            }
        }

        if (randomizeYawOnApply)
        {
            float yaw = Random.Range(0f, 360f);
            target.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    public override void OnTick(StatusController target, ref EffectRuntime runtime, float dt)
    {
        var slime = target.GetComponent<EnemySlime>();
        if (slime != null && slimeAnchors.TryGetValue(slime, out var anchorS))
            WanderAnchor(anchorS, dt);

        var idol = target.GetComponent<EnemyIdol>();
        if (idol != null && idolAnchors.TryGetValue(idol, out var anchorI))
            WanderAnchor(anchorI, dt);

        var boss = target.GetComponent<GolemBasicAI>();
        if (boss != null && bossAnchors.TryGetValue(boss, out var anchorB))
            WanderAnchor(anchorB, dt);
    }

    Vector3 GetInitialOffsetPosition(Vector3 center)
    {
        float angle = Random.value * Mathf.PI * 2f;
        float radius = Random.Range(wanderRadius * 0.5f, wanderRadius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        return center + offset;
    }

    void WanderAnchor(BlindTargetAnchor anchor, float dt)
    {
        if (anchor == null || anchor.owner == null)
            return;

        Vector3 center = anchor.owner.position;
        Vector3 pos = anchor.transform.position;

        Vector3 fromCenter = pos - center;
        if (fromCenter.sqrMagnitude > wanderRadius * wanderRadius)
            pos = center + fromCenter.normalized * wanderRadius;

        Vector3 dir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        );

        if (dir.sqrMagnitude > 0.001f)
            dir = dir.normalized;

        pos += dir * wanderSpeed * dt;
        anchor.transform.position = pos;
    }

    public override void OnExpire(StatusController target, ref EffectRuntime runtime)
    {
        var slime = target.GetComponent<EnemySlime>();
        if (slime != null && slimeAnchors.TryGetValue(slime, out var anchorS))
        {
            if (anchorS != null && anchorS.originalTarget != null)
                slime.target = anchorS.originalTarget;

            if (anchorS != null)
                Object.Destroy(anchorS.gameObject);

            slimeAnchors.Remove(slime);
        }

        var idol = target.GetComponent<EnemyIdol>();
        if (idol != null && idolAnchors.TryGetValue(idol, out var anchorI))
        {
            if (anchorI != null && anchorI.originalTarget != null)
                idol.target = anchorI.originalTarget;

            if (anchorI != null)
                Object.Destroy(anchorI.gameObject);

            idolAnchors.Remove(idol);
        }

        var boss = target.GetComponent<GolemBasicAI>();
        if (boss != null && bossAnchors.TryGetValue(boss, out var anchorB))
        {
            if (anchorB != null && anchorB.originalTarget != null)
                boss.player = anchorB.originalTarget;

            if (anchorB != null)
                Object.Destroy(anchorB.gameObject);

            bossAnchors.Remove(boss);
        }
    }
}
