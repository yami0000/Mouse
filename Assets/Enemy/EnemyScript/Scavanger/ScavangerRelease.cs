using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavangerReleaseState : EnemyState
{
    protected Enemy_Scavanger enemy;
    public ScavangerReleaseState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.zerovelocity();
        enemy.stateTimer_Scavanger = 0.5f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.HeadToPlayer();
        if(enemy.stateTimer_Scavanger<=0)
            stateMachine.ChangeState(enemy.flyState);
    }
}

