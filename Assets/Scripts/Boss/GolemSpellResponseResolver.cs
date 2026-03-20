using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GolemSpellResponseEntry
{
    public ProjectileStats incomingSpell;
    public ProjectileStats optionA;
    public ProjectileStats optionB;
}

[CreateAssetMenu(menuName = "Combat/Golem Spell Response Resolver")]
public class GolemSpellResponseResolver : ScriptableObject
{
    [SerializeField] private List<GolemSpellResponseEntry> entries = new List<GolemSpellResponseEntry>();
    private Dictionary<ProjectileStats, GolemSpellResponseEntry> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<ProjectileStats, GolemSpellResponseEntry>();
        foreach (var e in entries)
        {
            if (e != null && e.incomingSpell != null && !lookup.ContainsKey(e.incomingSpell))
                lookup.Add(e.incomingSpell, e);
        }
    }

    public bool TryGetOptions(ProjectileStats incoming, out ProjectileStats optionA, out ProjectileStats optionB)
    {
        optionA = null;
        optionB = null;

        if (incoming == null || lookup == null)
            return false;

        if (lookup.TryGetValue(incoming, out var entry))
        {
            optionA = entry.optionA;
            optionB = entry.optionB;
            return true;
        }

        return false;
    }
}
