using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for one thing an NPC can do. Subclasses live in NPCActions.cs.
/// An action has an optional requirement (startCondition). With no requirement
/// it simply runs. Tick() is called every frame and returns true when finished.
/// </summary>
[System.Serializable]
public abstract class NPCAction
{
    // null => no requirement. Picked in the Inspector via the SerializeReference type dropdown.
    [SerializeReference] public NPCCondition startCondition;

    [Tooltip("If true, interrupts can NOT preempt this action while it runs " +
             "(e.g. a scripted/cutscene step that must finish). Default false = interruptible.")]
    public bool blockInterrupts = false;

    public bool CanStart(NPC_All npc) => startCondition == null || startCondition.IsMet(npc);

    /// <summary>Called once when the action becomes active. Reset internal state here.</summary>
    public virtual void OnEnter(NPC_All npc) { }

    /// <summary>Called every frame while active. Return true when the action is done.</summary>
    public abstract bool Tick(NPC_All npc);

    /// <summary>Called once when the action finishes.</summary>
    public virtual void OnExit(NPC_All npc) { }

    /// <summary>
    /// Called when the action is RESUMED after an interrupt, instead of OnEnter,
    /// so its progress is not lost. The default re-establishes FSM state by calling
    /// OnEnter (which for a WalkAction restarts that leg's timer). Override if your
    /// action must resume without resetting its timer.
    /// </summary>
    public virtual void OnResume(NPC_All npc) => OnEnter(npc);

    /// <summary>One-line description shown in the Inspector list / Scene gizmo.
    /// Runtime-safe (no editor APIs). Override per action.</summary>
    public virtual string Summary => GetType().Name;
}

/// <summary>
/// Runs a list of actions in order. Used by both NPCController (the whole routine)
/// and by GroupAction (a nested, optionally-repeating block).
///
///   loop == false               -> run through the list once, then finish  (a CHAIN)
///   loop == true, repeatCount 0  -> repeat the list forever                 (a REPEATING group)
///   loop == true, repeatCount N  -> repeat the list N times, then finish
/// </summary>
[System.Serializable]
public class ActionRunner
{
    [SerializeReference] public List<NPCAction> actions = new List<NPCAction>();
    public bool loop = false;
    [Min(0)] public int repeatCount = 0; // 0 = infinite (only matters when loop == true)

    private int index = -1;
    private int loopsDone = 0;
    private bool actionStarted = false;
    private bool finished = false;

    public bool IsFinished => finished;

    // Read-only live state, used by the editor / gizmos to show what's running.
    public int CurrentIndex => index;
    public NPCAction CurrentAction =>
        (actions != null && index >= 0 && index < actions.Count) ? actions[index] : null;

    public void Reset()
    {
        index = -1;
        loopsDone = 0;
        actionStarted = false;
        finished = false;
    }

    /// <summary>
    /// Re-establish the currently-running action's state after an interrupt handed
    /// control back. Does nothing if nothing was mid-flight (the next Tick will
    /// OnEnter the next action cleanly). Recurses into nested groups via OnResume.
    /// </summary>
    public void ReEnterCurrent(NPC_All npc)
    {
        if (finished || actions == null) return;
        if (index < 0 || index >= actions.Count) return;
        if (!actionStarted) return; // nothing was running yet
        actions[index]?.OnResume(npc);
    }

    /// <summary>Advance the routine by one frame. Returns true once the whole runner is finished.</summary>
    public bool Tick(NPC_All npc)
    {
        if (finished) return true;
        if (actions == null || actions.Count == 0) { finished = true; return true; }

        if (index < 0) { index = 0; actionStarted = false; }

        NPCAction current = actions[index];
        if (current == null) { AdvanceIndex(); return finished; }

        // Gate on the requirement. We sit here (without advancing) until it is met.
        if (!actionStarted)
        {
            if (!current.CanStart(npc)) return false;
            current.OnEnter(npc);
            actionStarted = true;
        }

        if (current.Tick(npc))
        {
            current.OnExit(npc);
            actionStarted = false;
            AdvanceIndex();
        }

        return finished;
    }

    private void AdvanceIndex()
    {
        index++;
        if (index < actions.Count) return;

        // Reached the end of the list.
        index = 0;
        loopsDone++;

        bool keepLooping = loop && (repeatCount == 0 || loopsDone < repeatCount);
        if (!keepLooping)
            finished = true;
    }
}
