using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonCleaningState : NPCstate
{
    protected Raccoon raccoon;

    
    public RaccoonCleaningState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, Raccoon raccoon) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.raccoon = raccoon;
    }

    public override void Enter()
    {
        base.Enter();
         
        raccoon.stateTimer_Raccoon = 1f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (raccoon.stateTimer_Raccoon <= 0)
        {
             
                stateMachine.ChangeState(raccoon.idleState);
             



        }
    }
}
