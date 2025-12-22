using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Miner : Enemy
{
    public MinerIdleState idleState { get; private set; }

    public MinerWalkState walkState { get; private set; }

    public MinerAttackState attackState { get; private set; }

    public MinerDeathState deathState { get; private set; }

    public MinerBattleState battleState { get; private set; }

    public float stateTimer_Miner;

    [SerializeField]public Laser2Attack laser;

    public bool isDetected;
    protected override void Awake()
    {
        base.Awake();

        idleState = new MinerIdleState(this, stateMachine, "Idle", this);
        walkState = new MinerWalkState(this, stateMachine, "Walk", this);
        attackState = new MinerAttackState(this, stateMachine, "Attack", this);
        deathState = new MinerDeathState(this, stateMachine, "Idle", this);
        battleState = new MinerBattleState(this, stateMachine, "Walk", this);


    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Miner -= Time.deltaTime;


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
}
