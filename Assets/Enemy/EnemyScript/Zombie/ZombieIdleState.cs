using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieIdleState : ZombieGroundedState
{
    public ZombieIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Zombie enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {

    }
    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Zombie = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.zerovelocity();
        if (enemy.stateTimer_Zombie < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);


        }
    }
}
