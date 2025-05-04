using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDesExplodeState : EnemyState
{
    private Enemy_SelfDes enemy;
    public SelfDesExplodeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_SelfDes enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer_SelfDes = enemy.explodeTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();

        if (stateTimer_SelfDes < 0)
        {
            
            stateMachine.ChangeState(enemy.deathState);
        }
    }
}
