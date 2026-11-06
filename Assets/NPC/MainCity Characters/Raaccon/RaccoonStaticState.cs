using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonStaticState : NPCstate
{
 protected Raccoon raccoon;

    private int P;

    public RaccoonStaticState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName,Raccoon raccoon) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.raccoon = raccoon;
    }

    public override void Enter()
    {
        base.Enter();
        P = Random.Range(0, 99);
        raccoon.stateTimer_Raccoon = 0.5f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if(raccoon.stateTimer_Raccoon<=0)
        {
            if (P <= 9)
                stateMachine.ChangeState(raccoon.cleaningState);
            else
            {
                raccoon.stateTimer_Raccoon = 0.5f;
                P = Random.Range(0, 99);
            }


        }
    }
}
