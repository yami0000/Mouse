using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicIdleState : MechanicGroundedState
{
    public MechanicIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Mechanic enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();

        enemy.stateTimer_Mechanic = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.stateTimer_Mechanic < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        }
    }
}
