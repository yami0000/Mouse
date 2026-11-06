using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Raccoon : NPC
{
    public RaccoonStaticState idleState { get; private set; }

    public RaccoonCleaningState cleaningState { get; private set; }

    [HideInInspector]public float stateTimer_Raccoon;
    protected override void Awake()
    {
        base.Awake();

        idleState = new RaccoonStaticState(this, stateMachine, "Idle", this);
        cleaningState = new RaccoonCleaningState(this, stateMachine, "Clean", this);
    }

    
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        stateTimer_Raccoon -= Time.deltaTime;

    }
}
