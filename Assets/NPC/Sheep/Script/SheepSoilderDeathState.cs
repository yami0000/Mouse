using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSoilderDeathState : NPCstate
{
    protected NPC_SheepSoilder sheep;

    public SheepSoilderDeathState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_SheepSoilder sheep) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.sheep = sheep;
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
    }
}
