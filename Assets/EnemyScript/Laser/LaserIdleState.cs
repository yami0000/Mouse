using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserIdleState : LaserGrounededState
{
     
    public LaserIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Laser enemy) : base(_enemyBase, _stateMachine, _animBoolName,enemy)
    {
         
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer_Laser = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();
        if (stateTimer_Laser < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);

             
        }
    }
}
