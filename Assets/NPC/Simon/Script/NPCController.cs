using UnityEngine;

/// <summary>
/// Universal, data-driven NPC controller. Attach this next to an NPC_All on the
/// same GameObject, then compose the NPC's behaviour entirely in the Inspector
/// from reusable actions (Idle / Walk / WalkToTarget / Dialogue / Flip / Group)
/// and optional requirements (conditions). No per-NPC scripting required.
///
/// It never writes to rb.velocity itself. It only decides which action is active,
/// and each action drives the existing idleState / walkState, so animation and the
/// Setvelocity() sprite-flipping keep working exactly as before.
///
/// Covers the four cases:
///   1. Walk/Idle gated by a requirement (or none)  -> Walk/Idle actions + startCondition
///   2. Specific actions gated by a requirement      -> e.g. DialogueAction + startCondition
///   3. Repeat a group of actions                    -> loop the routine, or a looping GroupAction
///   4. Chain of actions (run once)                  -> routine with loop = false
/// </summary>
[RequireComponent(typeof(NPC_All))]
public class NPCController : MonoBehaviour
{
    [Tooltip("The ordered behaviour for this NPC. Set 'loop' on the routine to repeat the whole thing.")]
    public ActionRunner routine = new ActionRunner();

    [Tooltip("Begin running automatically. Turn off to start it later from another script.")]
    public bool playOnStart = true;

    private NPC_All npc;
    private bool running;
    private bool parkedIdle;

    private void Awake()
    {
        npc = GetComponent<NPC_All>();
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    private void Update()
    {
        if (!running || npc == null) return;

        if (routine.Tick(npc))
        {
            // Chain finished (or all loops done): park in idle once.
            running = false;
            if (!parkedIdle)
            {
                npc.EnterIdle();
                parkedIdle = true;
            }
        }
    }

    /// <summary>Start (or restart) the routine from the beginning.</summary>
    [ContextMenu("Play / Restart")]
    public void Play()
    {
        routine.Reset();
        parkedIdle = false;
        running = true;
    }

    public void Stop()
    {
        running = false;
        if (npc != null) npc.EnterIdle();
    }

    // ---- Introspection used by the editor + Scene gizmos ----

    /// <summary>Human-readable description of what the NPC is doing right now (Play mode).</summary>
    public string CurrentActionDescription()
    {
        if (!Application.isPlaying) return "(edit mode)";
        if (!running) return routine.IsFinished ? "Finished \u2014 idle" : "Stopped";
        NPCAction a = DeepCurrent(routine);
        return a != null ? a.Summary : "(waiting for requirement)";
    }

    // Drills into a running GroupAction to find the deepest active action.
    private static NPCAction DeepCurrent(ActionRunner r)
    {
        NPCAction a = r.CurrentAction;
        if (a is GroupAction g)
        {
            NPCAction inner = DeepCurrent(g.sequence);
            return inner ?? a;
        }
        return a;
    }

    // Gathers every WalkToTarget destination (recursing into groups) in order.
    private static void CollectTargets(ActionRunner r, System.Collections.Generic.List<Transform> outList)
    {
        if (r == null || r.actions == null) return;
        foreach (NPCAction a in r.actions)
        {
            if (a is WalkToTargetAction w && w.target != null) outList.Add(w.target);
            else if (a is GroupAction g) CollectTargets(g.sequence, outList);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (routine == null) return;

        var targets = new System.Collections.Generic.List<Transform>();
        CollectTargets(routine, targets);

        // Patrol path: spheres at each destination, joined in order.
        Gizmos.color = new Color(0.25f, 0.8f, 1f, 0.9f);
        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 p = targets[i].position;
            Gizmos.DrawWireSphere(p, 0.2f);
            UnityEditor.Handles.Label(p + Vector3.up * 0.4f, $"{i + 1}. {targets[i].name}");
            if (i > 0) Gizmos.DrawLine(targets[i - 1].position, p);
        }

        // Live state while playing.
        if (Application.isPlaying)
        {
            if (DeepCurrent(routine) is WalkToTargetAction cur && cur.target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, cur.target.position);
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, CurrentActionDescription());
        }
    }
#endif
}
