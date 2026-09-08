using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Zombie : Enemy
{
    public ZombieIdleState idleState { get; private set; }
    public ZombieWalkState walkState { get; private set; }
    public ZombieBattleState battleState { get; private set; }
    public ZombieAttackState attackState { get; private set; }
    public ZombieDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_Zombie;

    protected override void Awake()
    {
        base.Awake();
        idleState = new ZombieIdleState(this, stateMachine, "Idle", this);
        walkState = new ZombieWalkState(this, stateMachine, "Walk", this);
        battleState = new ZombieBattleState(this, stateMachine, "Walk", this);
        attackState = new ZombieAttackState(this, stateMachine, "Attack", this);
        deathState = new ZombieDeathState(this, stateMachine, "Idle", this);
    }
    protected override void Start()
    {
        base.Start();
    }


    protected override void Update()
    {
        base.Update();
    }
}
