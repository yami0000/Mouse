using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandmaInteractState : NPCstate
{
    protected Grandma Grandma;
    public GrandmaInteractState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, Grandma grandma) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.Grandma = grandma;
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
        Grandma.zerovelocity();

        base.Update();
        if (!GM.Instance.GameManager.isInteractGrandma)
            stateMachine.ChangeState(Grandma.idleState);
    }
}
