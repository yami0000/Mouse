using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCchef : NPC
{
    public ChefIdleState idleState { get; private set; } 

    public ChefGreetState greetState { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        idleState = new ChefIdleState(this, stateMachine, "Idle",this);
        greetState = new ChefGreetState(this, stateMachine, "Greet",this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();



    }
}
