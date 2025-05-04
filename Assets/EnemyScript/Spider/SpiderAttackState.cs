using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAttackState : EnemyState
{
    private Enemy_Spider enemy;
    public SpiderAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Spider enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();

        if (ReadyToAttack)
            enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir*10, rb.velocity.y);

        if(triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
}
