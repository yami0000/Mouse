using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeAttackState : EnemyState
{
    private Enemy_Bee enemy;
    public BeeAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Bee enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

        enemy.lastTimeAttacked_Bee = Time.time;
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();

        if (ReadyToAttack_Bee)
        {
            enemy.ammo.ExecuteEffect(enemy.transform);
            
            ReadyToAttack_Bee = false;
        }

        if (triggerCalled_Bee)
            stateMachine.ChangeState(enemy.battleState);
    }
}
