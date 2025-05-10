using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicIdleState : MedicGroundState
{
    public MedicIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Medic enemy) : base(_enemyBase, _stateMachine, _animBoolName,enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();

        enemy.stateTimer_Medic = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.stateTimer_Medic < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        }
    }
}
