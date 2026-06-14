using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Universal, data-driven NPC controller. Attach this next to an NPC_All on the
/// same GameObject, then compose the NPC's behaviour entirely in the Inspector
/// from reusable actions (Idle / Walk / WalkToTarget / Dialogue / Flip / Group)
/// and optional requirements (conditions). No per-NPC scripting required.
///
/// On top of the linear 'routine', it supports an INTERRUPT layer: a list of
/// reactive overrides that can preempt the routine on any frame (mid-action
/// included), run a short sequence of their own, then hand control back so the
/// routine resumes exactly where it was frozen. Use it for "pace back and forth,
/// but stop and talk when the player presses E, then keep pacing."
///
/// It never writes to rb.velocity itself. It only decides which action is active,
/// and each action drives the existing idleState / walkState, so animation and the
/// Setvelocity() sprite-flipping keep working exactly as before.
/// </summary>
[RequireComponent(typeof(NPC_All))]
public class NPCController : MonoBehaviour
{
    [Tooltip("The ordered behaviour for this NPC. Set 'loop' on the routine to repeat the whole thing.")]
    public ActionRunner routine = new ActionRunner();

    [Tooltip("Reactive overrides. Any of these can preempt the routine mid-action, run, then hand " +
             "control back. Fire one via its trigger condition or via NPCController.Raise(\"id\").")]
    public List<NPCInterrupt> interrupts = new List<NPCInterrupt>();

    [Tooltip("Begin running automatically. Turn off to start it later from another script.")]
    public bool playOnStart = true;

    private NPC_All npc;
    private bool live;          // controller is active (between Play and Stop)
    private bool running;       // the routine still has actions left to run
    private bool parkedIdle;
    private NPCInterrupt active; // the interrupt currently being serviced, or null

    public bool IsInterrupted => active != null;
    public string ActiveInterruptId => active != null ? active.id : null;

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
        if (npc == null || !live) return;

        // 1) Servicing an interrupt: tick ONLY its body; the routine stays frozen.
        if (active != null)
        {
            if (active.body.Tick(npc))
            {
                active = null;
                if (running) routine.ReEnterCurrent(npc); // restore the frozen action's state
                else npc.EnterIdle();                     // routine already done: settle to idle
            }
            return;
        }

        // 2) Can an interrupt fire right now? First armed match wins (priority = list order).
        //    A current action flagged blockInterrupts shields the routine for now.
        NPCAction current = running ? DeepCurrent(routine) : null;
        bool shielded = current != null && current.blockInterrupts;
        if (!shielded)
        {
            for (int i = 0; i < interrupts.Count; i++)
            {
                NPCInterrupt it = interrupts[i];
                if (it == null) continue;
                if (it.ShouldFire(npc))
                {
                    active = it;
                    it.Consume();
                    if (it.parkInIdleOnEnter) npc.EnterIdle();
                    parkedIdle = false;
                    return;
                }
            }
        }

        // 3) Normal routine.
        if (running && routine.Tick(npc))
        {
            running = false;
            if (!parkedIdle)
            {
                npc.EnterIdle();
                parkedIdle = true;
            }
        }
    }

    /// <summary>
    /// Fire the interrupt with this id (edge trigger). It activates on the next
    /// frame. Ignored while another interrupt is already being serviced, so a
    /// second E-press during dialogue won't queue a second one.
    /// </summary>
    public void Raise(string id)
    {
        if (active != null) return;
        for (int i = 0; i < interrupts.Count; i++)
        {
            if (interrupts[i] != null && interrupts[i].id == id)
            {
                interrupts[i].raised = true;
                return;
            }
        }
    }

    /// <summary>Start (or restart) the routine from the beginning and re-arm interrupts.</summary>
    [ContextMenu("Play / Restart")]
    public void Play()
    {
        routine.Reset();
        for (int i = 0; i < interrupts.Count; i++)
            interrupts[i]?.Rearm();

        active = null;
        parkedIdle = false;
        running = true;
        live = true;
    }

    public void Stop()
    {
        live = false;
        running = false;
        active = null;
        if (npc != null) npc.EnterIdle();
    }

    // ---- Introspection used by the editor + Scene gizmos ----

    /// <summary>Human-readable description of what the NPC is doing right now (Play mode).</summary>
    public string CurrentActionDescription()
    {
        if (!Application.isPlaying) return "(edit mode)";
        if (!live) return "Stopped";

        if (active != null)
        {
            NPCAction ia = DeepCurrent(active.body);
            return $"[interrupt \u201c{active.id}\u201d] " + (ia != null ? ia.Summary : "(starting)");
        }

        if (!running) return routine.IsFinished ? "Finished \u2014 idle" : "Idle (waiting)";
        NPCAction a = DeepCurrent(routine);
        return a != null ? a.Summary : "(waiting for requirement)";
    }

    // Drills into a running GroupAction to find the deepest active action.
    private static NPCAction DeepCurrent(ActionRunner r)
    {
        if (r == null) return null;
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

/// <summary>
/// A reactive override that can preempt an NPCController's routine at ANY point
/// (mid-action included), run its own short sequence, then hand control back so
/// the routine resumes exactly where it was frozen.
///
/// Think of it like a hardware interrupt: the routine is the main program, this
/// is the service handler. The routine is never ticked while an interrupt is
/// active, so its index/timers stay frozen and resuming is automatic.
///
/// Fire it two ways (either or both):
///   * Polled -> set a 'trigger' condition (e.g. PlayerWithinXCondition).
///   * Edge   -> call NPCController.Raise("id") from anything (e.g. an E-press
///               relay sitting on the NPC's interact collider).
///
/// It fires once per RISING edge: a held-true trigger won't re-fire until it
/// drops false again, and a Raise() fires once per call.
/// </summary>
[System.Serializable]
public class NPCInterrupt
{
    [Tooltip("Name used by NPCController.Raise(\"id\") to fire this interrupt from outside.")]
    public string id = "talk";

    [Tooltip("Optional polled requirement. Leave empty to fire only via Raise(\"id\").")]
    [SerializeReference] public NPCCondition trigger;

    [Tooltip("What the NPC does while interrupted. e.g. a single DialogueAction with 'wait for finish'.")]
    public ActionRunner body = new ActionRunner();

    [Tooltip("Force the NPC to stand still the instant the interrupt begins (avoids one frame of drift).")]
    public bool parkInIdleOnEnter = true;

    // ---- runtime (not serialized) ----
    [System.NonSerialized] public bool raised;    // set by Raise(); consumed when it fires
    [System.NonSerialized] private bool latched;   // true after firing, until the signal falls again

    private bool Signal(NPC_All npc) =>
        raised || (trigger != null && trigger.IsMet(npc));

    /// <summary>True on the rising edge of the signal (and only once per edge).</summary>
    public bool ShouldFire(NPC_All npc)
    {
        bool signal = Signal(npc);
        if (!signal) latched = false; // re-arm when the signal drops
        return signal && !latched;
    }

    /// <summary>Called by the controller the moment this interrupt becomes active.</summary>
    public void Consume()
    {
        latched = true; // don't re-fire until the signal falls again
        raised = false; // an edge raise is now spent
        body.Reset();
    }

    /// <summary>Reset to a clean, re-armed state (used on Play / Stop).</summary>
    public void Rearm()
    {
        raised = false;
        latched = false;
        body.Reset();
    }

    public string Summary
    {
        get
        {
            string t = trigger != null ? trigger.Summary : $"Raise(\"{id}\")";
            int n = (body != null && body.actions != null) ? body.actions.Count : 0;
            return $"\"{id}\"  [{t}]  \u2192 {n} action(s)";
        }
    }
}
