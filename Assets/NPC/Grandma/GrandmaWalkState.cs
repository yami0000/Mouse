using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandmaWalkState : NPCstate
{
    int P;
    protected Grandma Grandma;
    public GrandmaWalkState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, Grandma grandma) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.Grandma = grandma;
    }

    public override void Enter()
    {
        base.Enter();
        Grandma.stateTimer_Grandma = 1f;
        P = Random.Range(0, 100);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        Grandma.Setvelocity(Grandma.speed*Grandma.facingDir, rb.velocity.y);

        if (Grandma.facingDir == -1 && Grandma.transform.position.x <= Grandma.A.position.x || Grandma.facingDir == 1 &&  Grandma.transform.position.x >= Grandma.B.position.x)
            Grandma.Flip();

        if (GM.Instance.GameManager.isInteractGrandma)
            stateMachine.ChangeState(Grandma.interactState);

        if (Grandma.stateTimer_Grandma <= 0)
        {
            if (P <= 50)
                stateMachine.ChangeState(Grandma.idleState);
            else
                stateMachine.ChangeState(Grandma.walkState);

        }

    }
}
