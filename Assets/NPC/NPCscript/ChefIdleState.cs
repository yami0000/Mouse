using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefIdleState : NPCstate
{
    protected NPCchef Npc;

    protected Transform player;

    public ChefIdleState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName,NPCchef npc) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.Npc = npc;
    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (visited)
        {
            if (Vector2.Distance(Npc.transform.position, player.position) > 5.5f)
                visited = false;


        }

        if (player != null) 
        if (Vector2.Distance(Npc.transform.position, player.position) < 2.5f && visited !=true  )
        {

            visited = true;
            ¡¡
            stateMachine.ChangeState(Npc.greetState); 
        
        }
    }
}
