using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceIdleState : PoliceGroundedState
{
    

    public PoliceIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Police enemy) : base(_enemyBase, _stateMachine, _animBoolName,enemy)
    {
       
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Police = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.zerovelocity();
        if (enemy.stateTimer_Police < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);


        }
    }
}
