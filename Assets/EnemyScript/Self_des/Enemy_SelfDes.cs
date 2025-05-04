using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy_SelfDes : Enemy
{
    public SelfDesIdleState idleState {  get; private set; }

    public SelfDesMoveState walkState { get; private set; } 

    public SelfDesBattleState battleState { get; private set; }

    public SelfDesExplodeState explodeState { get; private set; }

    public SelfDesDeathState deathState { get; private set; }

    [SerializeField] public SelfDes explode;

    public float explodeTime;
    public float speedMultiplyer;
    protected override void Awake()
    {
        base.Awake(); 

        idleState = new SelfDesIdleState(this, stateMachine, "Idle", this);
        walkState = new SelfDesMoveState(this, stateMachine, "Walk", this);
        battleState = new SelfDesBattleState(this, stateMachine, "Walk", this);
        explodeState = new SelfDesExplodeState(this, stateMachine, "Explode", this);
        deathState = new SelfDesDeathState(this, stateMachine, "Explode", this);

    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();



    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }
}

