using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A requirement that gates an action (or the start of a group). Add your own
/// by inheriting from this; it will show up automatically in the Inspector's
/// SerializeReference dropdown. Leaving a condition empty means "no requirement".
/// </summary>
[System.Serializable]
public abstract class NPCCondition
{
    public abstract bool IsMet(NPC_All npc);

    /// <summary>One-line description for the Inspector list. Runtime-safe.</summary>
    public virtual string Summary => GetType().Name;
}

/// <summary>True when the player is within (or, if inverted, outside) a horizontal distance.</summary>
[System.Serializable]
public class PlayerWithinXCondition : NPCCondition
{
    [Min(0f)] public float distance = 3f;
    [Tooltip("When true, the condition is met while the player is FARTHER than 'distance'.")]
    public bool invert = false;

    public override bool IsMet(NPC_All npc)
    {
        var player = (PlayerManager.Instance != null) ? PlayerManager.Instance.player : null;
        if (player == null) return invert; // no player: treat "near" as false, "far" as true

        float dx = Mathf.Abs(player.transform.position.x - npc.transform.position.x);
        bool near = dx <= distance;
        return invert ? !near : near;
    }

    public override string Summary => invert ? $"player > {distance:0.##}" : $"player \u2264 {distance:0.##}";
}

/// <summary>True when the NPC is currently blocked by a wall ahead (or, inverted, when the path is clear).</summary>
[System.Serializable]
public class PathBlockedCondition : NPCCondition
{
    public bool invert = false; // invert => "path is clear"

    public override bool IsMet(NPC_All npc)
    {
        bool blocked = npc.IsWallDetected() || !npc.IsGroundDetected();
        return invert ? !blocked : blocked;
    }

    public override string Summary => invert ? "path clear" : "path blocked";
}

/// <summary>Rolls a random chance the first time it is checked, then sticks with that result.</summary>
[System.Serializable]
public class ChanceCondition : NPCCondition
{
    [Range(0f, 1f)] public float probability = 0.5f;

    private bool rolled = false;
    private bool result = false;

    public override bool IsMet(NPC_All npc)
    {
        if (!rolled)
        {
            result = Random.value <= probability;
            rolled = true;
        }
        return result;
    }

    public override string Summary => $"{Mathf.RoundToInt(probability * 100f)}% chance";
}

[System.Serializable]
public class RandomIntervalCondition : NPCCondition
{
    [Min(0f)] public float minSeconds = 2f;
    [Min(0f)] public float maxSeconds = 5f;

    [System.NonSerialized] private float timer = -1f;

    public override bool IsMet(NPC_All npc)
    {
        if (timer < 0f) timer = Random.Range(minSeconds, maxSeconds); // first arm

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = Random.Range(minSeconds, maxSeconds); // re-arm for next time
            return true;                                  // single-frame pulse
        }
        return false;
    }

    public override string Summary => $"every {minSeconds:0.#}\u2013{maxSeconds:0.#}s (random)";
}

/// <summary>
/// A flat random chance, evaluated continuously. 'ratePerSecond' is the average
/// number of times per second it pulses true (frame-rate independent). Good for
/// "occasionally, unpredictably" rather than a tidy interval.
/// </summary>
[System.Serializable]
public class RandomChancePerSecondCondition : NPCCondition
{
    [Tooltip("Average number of pulses per second.")]
    [Min(0f)] public float ratePerSecond = 0.4f;

    public override bool IsMet(NPC_All npc)
    {
        return Random.value < ratePerSecond * Time.deltaTime;
    }

    public override string Summary => $"~{ratePerSecond:0.##}/s (random)";
}

[System.Serializable]
public class EntitiesDeadCondition : NPCCondition
{
    public enum Need { All, Any }

    [Tooltip("Entities to watch. Assign in the inspector.")]
    public List<Entity> targets = new List<Entity>();

    [Tooltip("All = met when every target is dead. Any = met when at least one is dead.")]
    public Need requires = Need.All;

    [Tooltip("Invert the result (met while NOT yet dead).")]
    public bool invert = false;

    // ---- runtime (not serialized) ----
    [System.NonSerialized] private bool inited;
    [System.NonSerialized] private List<Entity> watched;
    [System.NonSerialized] private HashSet<Entity> dead;

    private void EnsureInit()
    {
        if (inited) return;
        inited = true;
        watched = new List<Entity>();
        dead = new HashSet<Entity>();
        if (targets == null) return;

        foreach (Entity e in targets)
        {
            if (e == null) continue;        // skip empty inspector slots
            watched.Add(e);
            if (e.IsDead) dead.Add(e);      // already dead when we started watching
            else e.OnDied += HandleDied;    // latch the death when it happens
        }
    }

    private void HandleDied(Entity e)
    {
        dead.Add(e);
        e.OnDied -= HandleDied;             // one-shot: stop holding the subscription
    }

    public override bool IsMet(NPC_All npc)
    {
        EnsureInit();
        if (watched.Count == 0) return invert; // nothing watched: "dead" is vacuously false

        int deadCount = 0;
        for (int i = 0; i < watched.Count; i++)
        {
            Entity e = watched[i];
            if (e == null || e.IsDead || dead.Contains(e)) deadCount++; // null == destroyed at runtime
        }

        bool met = requires == Need.All ? deadCount == watched.Count : deadCount > 0;
        return invert ? !met : met;
    }

    public override string Summary
    {
        get
        {
            int n = targets != null ? targets.Count : 0;
            string core = requires == Need.All ? $"all {n} dead" : $"any of {n} dead";
            return invert ? "not (" + core + ")" : core;
        }
    }
}