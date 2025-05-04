using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spider : Enemy
{
    public SpiderWalkingState walkState{  get; private set; }
    public SpiderIdleState idleState{ get; private set; }

    public SpiderBattleState battleState{ get; private set; }   

    public SpiderAttackState attackState{ get; private set; }

    public SpiderDeathState deathState{ get; private set; } 
      
    protected override void Awake()
    {
        base.Awake();

        walkState = new SpiderWalkingState(this, stateMachine,"Walk", this);
        idleState = new SpiderIdleState(this,stateMachine,"Idle",this);
        battleState = new SpiderBattleState(this, stateMachine, "Walk", this);
        attackState = new SpiderAttackState(this,stateMachine,"Attack",this);
        deathState = new SpiderDeathState(this,stateMachine,"Idle",this);
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
