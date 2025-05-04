using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy_Laser : Enemy 
{ 
    public LaserIdleState idleState { get; private set; }

    public LaserMoveState walkState { get; private set; }

    public LaserAttackState attackState { get; private set; }

    public LaserDeathState deathState { get; private set; }

    [SerializeField] public LaserAttack laser;

    public bool isDetected;
protected override void Awake()
{
    base.Awake();

    idleState = new LaserIdleState(this, stateMachine, "Idle", this);
    walkState = new LaserMoveState(this, stateMachine, "Walk", this);
    attackState = new LaserAttackState(this, stateMachine, "Attack", this);
    deathState = new LaserDeathState(this, stateMachine, "Idle", this);
  

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


    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {
            lastTimeAttacked = Time.time;
            return true;
        }

        return false;

    }
}
