using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWalkState : NPCstate
{
    protected NPC_All NPC;

    public NPCWalkState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_All npc) : base(_npcBase, _stateMachine, _animBoolName)
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
        NPC.Setvelocity(NPC.moveSpeed * NPC.facingDir,rb.velocity.y);
    }
}
