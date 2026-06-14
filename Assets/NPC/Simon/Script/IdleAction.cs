using UnityEngine;

// ---------------------------------------------------------------------------
// Concrete actions. Each is [System.Serializable] so it shows up in the
// Inspector's "Add" dropdown for the SerializeReference action lists.
// ---------------------------------------------------------------------------

/// <summary>Stand idle. duration <= 0 stays idle indefinitely (good as a final/parking step).</summary>
[System.Serializable]
public class IdleAction : NPCAction
{
    public float duration = 1f;

    private float timer;

    public override void OnEnter(NPC_All npc)
    {
        timer = duration;
        npc.EnterIdle();
    }
    public override void OnResume(NPC_All npc) => npc.EnterIdle();
    public override bool Tick(NPC_All npc)
    {
        if (duration <= 0f) return false; // park forever
        timer -= Time.deltaTime;
        return timer <= 0f;
    }

    public override string Summary => duration <= 0f ? "Idle (hold)" : $"Idle {duration:0.##}s";
}

/// <summary>Walk for a while (or until blocked). Direction can follow current facing or be forced.</summary>
[System.Serializable]
public class WalkAction : NPCAction
{
    [Tooltip("If true, walk in whatever direction the NPC currently faces.")]
    public bool useCurrentFacing = true;
    [Tooltip("-1 = left, +1 = right. Used only when 'useCurrentFacing' is false.")]
    public int direction = 1;

    public float speed = 2f;
    [Tooltip("Seconds to walk. <= 0 means walk until a stop condition is hit.")]
    public float duration = 2f;

    public bool stopAtWall = true;
    public bool stopAtLedge = true;

    private float timer;

    public override void OnEnter(NPC_All npc)
    {
        timer = duration;
        if (!useCurrentFacing) npc.SetFacing(direction);
        npc.SetMoveSpeed(speed);
        npc.EnterWalk();
    }
    public override void OnResume(NPC_All npc)
    {
        if (!useCurrentFacing) npc.SetFacing(direction);
        npc.SetMoveSpeed(speed);
        npc.EnterWalk();
    }
    public override bool Tick(NPC_All npc)
    {
        if (stopAtWall && npc.IsWallDetected()) return true;
        if (stopAtLedge && !npc.IsGroundDetected()) return true;

        if (duration > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) return true;
        }
        return false;
    }

    public override string Summary
    {
        get
        {
            string dir = useCurrentFacing ? "forward" : (direction < 0 ? "left" : "right");
            string dur = duration > 0f ? $"{duration:0.##}s" : "until blocked";
            return $"Walk {dir} ({dur}) @ {speed:0.##}";
        }
    }
}

/// <summary>Walk toward a target Transform until within a horizontal threshold (X-axis only).</summary>
[System.Serializable]
public class WalkToTargetAction : NPCAction
{
    public Transform target;
    public bool IsTargetPlayer;
    public float speed = 2f;
    [Min(0.01f)] public float arriveThreshold = 0.15f;
    [Tooltip("Give up if the path is blocked by a wall or ledge.")]
    public bool stopIfBlocked = true;



    public override void OnEnter(NPC_All npc)
    {
        npc.SetMoveSpeed(speed);
        npc.EnterWalk();
    }

    public override bool Tick(NPC_All npc)
    {
        if (target == null¡¡&& !IsTargetPlayer) return true;
        if(IsTargetPlayer) 
            target = PlayerManager.Instance.player.transform;

        float dx = target.position.x - npc.transform.position.x;
        if (Mathf.Abs(dx) <= arriveThreshold) return true;

        npc.SetFacing(dx > 0f ? 1 : -1);

        if (stopIfBlocked && (npc.IsWallDetected() || !npc.IsGroundDetected()))
            return true;

        return false;
    }

    public override string Summary => $"\u2192 {(target ? target.name : "<no target>")} @ {speed:0.##}";
}

/// <summary>Trigger a Yarn/Narrative dialogue node, optionally waiting for it to finish.</summary>
[System.Serializable]
public class DialogueAction : NPCAction
{
    public string nodeName = "";
    [Tooltip("Delay before the dialogue starts, passed to RequestDialogue's t.")]
    public float delay = 0f;
    [Tooltip("Stand idle while talking.")]
    public bool goIdleWhileTalking = true;
    [Tooltip("Block the routine until the dialogue finishes before moving on.")]
    public bool waitForFinish = true;

    private bool requested;
    private bool finished;

    public override void OnEnter(NPC_All npc)
    {
        requested = false;
        finished = false;
        if (goIdleWhileTalking) npc.EnterIdle();
    }

    public override bool Tick(NPC_All npc)
    {
        if (!requested)
        {
            requested = true;
            if (NarrativeManager.Instance != null)
                NarrativeManager.Instance.RequestDialogue(nodeName, delay, () => finished = true);
            else
                finished = true; // no manager in scene: don't stall the routine
        }

        if (!waitForFinish) return true;
        return finished;
    }

    public override string Summary =>
        $"Say \"{(string.IsNullOrEmpty(nodeName) ? "?" : nodeName)}\"{(waitForFinish ? "" : " (no wait)")}";
}

/// <summary>Flip the NPC to face the other direction.</summary>
[System.Serializable]
public class FlipAction : NPCAction
{
    public override bool Tick(NPC_All npc)
    {
        npc.Flip();
        return true;
    }

    public override string Summary => "Flip facing";
}

/// <summary>
/// A nested block of actions. Use this to REPEAT a group inside a larger chain.
/// e.g. patrol forever between A and B while the surrounding routine runs once.
///   loop = true,  repeatCount = 0  -> repeat the inner actions forever
///   loop = true,  repeatCount = 3  -> run the inner actions 3 times
///   loop = false                   -> run the inner actions once
/// </summary>
[System.Serializable]
public class GroupAction : NPCAction
{
    public ActionRunner sequence = new ActionRunner();

    public override void OnEnter(NPC_All npc) => sequence.Reset();
    public override bool Tick(NPC_All npc) => sequence.Tick(npc);

    public override string Summary
    {
        get
        {
            int count = sequence != null && sequence.actions != null ? sequence.actions.Count : 0;
            string mode = sequence != null && sequence.loop
                ? (sequence.repeatCount > 0 ? $"x{sequence.repeatCount}" : "loop")
                : "once";
            return $"Group [{count}] {mode}";
        }
    }
}
