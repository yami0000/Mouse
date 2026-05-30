using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceWShootState : EnemyState
{
    protected Enemy_Police enemy;

    public PoliceWShootState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Police enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Police = 1f;
        enemy.Shoot();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);
        if (enemy.stateTimer_Police <= 0)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
