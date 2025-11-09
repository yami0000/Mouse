using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Mechanic : Enemy
{
    public MechanicWalkState walkState { get; private set; }
    public MechanicIdleState idleState { get; private set; }
    public MechanicBattleState battleState { get; private set; }
    public MechanicAttackState attackState { get; private set; }
    public MechanicDeathState deathState { get; private set; }

    public float stateTimer_Mechanic;

 
    public Projectile projectile;
    protected override void Awake()
    {
        base.Awake();

        walkState = new MechanicWalkState(this, stateMachine, "Walk", this);
        idleState = new MechanicIdleState(this, stateMachine, "Idle", this);
        battleState = new MechanicBattleState(this, stateMachine, "Walk", this);
        attackState = new MechanicAttackState(this, stateMachine, "Attack", this);
        deathState = new MechanicDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Mechanic -= Time.deltaTime;


    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }


    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {

            return true;
        }

        return false;

    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerCheckDistance);
    }
}
