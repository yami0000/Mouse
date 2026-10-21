using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyIdleState : EnemyState
{
    protected Enemy_Heavy enemy;

    public HeavyIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();

        // Noticed, not detected: the Heavy is alerted from behind as well.
        if (enemy.IsPlayerNoticed())
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        // Waited long enough, turn around and patrol the other way.
        if (stateTimer < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        }
    }
}
