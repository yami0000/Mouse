using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCstate  
{
    protected NPCstatemachine stateMachine;
    protected NPC npc;
     

    protected Rigidbody2D rb;

 
    private string animBoolName;

    protected float stateTimer = 0;
    protected bool triggerCalled;
    protected bool visited = false;

    public NPCstate(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName)
    {
        this.npc = _npcBase;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;


    }
    public virtual void Enter()
    {
        triggerCalled = false;
        npc.anim.SetBool(animBoolName, true);
        rb = npc.rb;
 

    }
    public virtual void Update()
    {

        stateTimer -= Time.deltaTime;



        

 



    }
    public virtual void Exit()
    {


        npc.anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
