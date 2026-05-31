using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_SheepSoilder : NPC
{
    public SheepSoilderIdleState idleState { get; private set; }
    public SheepSoilderWalkState walkState { get; private set; }
    public SheepSoilderDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_SheepSoilder;

    protected override void Awake()
    {
        base.Awake();

        idleState = new SheepSoilderIdleState(this, stateMachine, "Idle", this);
        walkState = new SheepSoilderWalkState(this, stateMachine, "Walk", this);
        deathState = new SheepSoilderDeathState(this, stateMachine, "Idle", this);

    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_SheepSoilder -= Time.deltaTime;
    }
    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }
}
