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
}

