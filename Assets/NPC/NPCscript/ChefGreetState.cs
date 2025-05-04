using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefGreetState : NPCstate
{


    protected NPCchef Npc;
    public ChefGreetState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName,NPCchef npc) : base(_npcBase, _stateMachine, _animBoolName)
    {
    this.Npc = npc;
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

        if (triggerCalled)
        {   
            
            stateMachine.ChangeState(Npc.idleState);
           

        }
    }
}
