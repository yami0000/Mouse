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
}
