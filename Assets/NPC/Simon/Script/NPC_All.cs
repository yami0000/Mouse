using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_All : NPC
{
   public NPCIdleState idleState {  get; private set; }
   public NPCWalkState walkState { get; private set; }


    public float moveSpeed { get; private set; }
    protected override void Awake()
    {
        base.Awake();

        idleState = new NPCIdleState(this,stateMachine,"Idle",this);
        walkState = new NPCWalkState(this,stateMachine,"Walk",this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }
}
