using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic "special action" state. One instance is created per Animator bool
/// (see NPC_All.EnterSpecial), so any NPC-exclusive animation can be played
/// just by naming its bool - no per-NPC state class required.
/// </summary>
public class NPCSpecialState : NPCstate
{
    protected NPC_All NPC;

    public string AnimBoolName { get; private set; }

    // Walk forward at NPC.moveSpeed while playing; otherwise stand still like idle.
    public bool moveWhilePlaying;

    // True once the clip's Animation Event has called NPC.AnimationTrigger().
    public bool AnimationFinished => triggerCalled;

    public NPCSpecialState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_All npc) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.NPC = npc;
        AnimBoolName = _animBoolName;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (moveWhilePlaying)
            NPC.Setvelocity(NPC.moveSpeed * NPC.facingDir, rb.velocity.y);
        else
            NPC.zerovelocity();
    }

    /// <summary>
    /// Replay from the start while already in this state (e.g. the same special
    /// action twice in a row). The bool is already true, so the Animator would
    /// otherwise just keep looping the clip without resetting.
    /// </summary>
    public void Restart()
    {
        triggerCalled = false;
        rb = npc.rb;
        AnimatorStateInfo info = npc.anim.GetCurrentAnimatorStateInfo(0);
        npc.anim.Play(info.fullPathHash, 0, 0f);
    }
}
