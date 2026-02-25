using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrownerUpState : EnemyState
{
    protected Boss_Drowner boss;
    public DrownerUpState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.TurnToPlayer();
        boss.UF();

        boss.cd.enabled = true;
        boss.StateTimer_Drowner = boss.emergeDuration + boss.fallDuration;
    }


    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Drowner <= 0)
            stateMachine.ChangeState(boss.attackState);

    }
    public override void Exit()
    {
        base.Exit();
    }
}
