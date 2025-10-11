using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grandma : NPC
{
    public GrandmaIdleState idleState { get; private set; }

    public GrandmaWalkState walkState { get; private set; }

    public GrandmaInteractState interactState { get; private set; } 

    
    [HideInInspector]public float stateTimer_Grandma;
    public float speed;

    public Transform A;
    public Transform B;

    protected override void Awake()
    {
        base.Awake();

        idleState = new GrandmaIdleState(this, stateMachine, "Idle", this);
        walkState = new GrandmaWalkState(this, stateMachine, "Walk", this);
        interactState = new GrandmaInteractState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Grandma -= Time.deltaTime;

    }
}
