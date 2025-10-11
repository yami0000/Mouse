using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandmaIdleState : NPCstate
{
    public int P;
    protected Grandma Grandma;
    public GrandmaIdleState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, Grandma grandma) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.Grandma = grandma;
    }

    public override void Enter()
    {
        base.Enter();
        P = Random.Range(1,100);
        Grandma.stateTimer_Grandma = 0.3f;
        
        
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        Grandma.Setvelocity(0, rb.velocity.y);


        if (GM.Instance.GameManager.isInteractGrandma)
            stateMachine.ChangeState(Grandma.interactState);

        if (Grandma.stateTimer_Grandma <= 0)
        {
            if (P <= 10)
                stateMachine.ChangeState(Grandma.idleState);
            else
                stateMachine.ChangeState(Grandma.walkState);

        }
    }
}
