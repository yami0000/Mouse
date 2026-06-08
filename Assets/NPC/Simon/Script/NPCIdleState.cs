using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCIdleState : NPCstate
{
    protected NPC_All NPC;

    public NPCIdleState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName,NPC_All npc) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.NPC = npc;
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
        NPC.zerovelocity();
    }
}
