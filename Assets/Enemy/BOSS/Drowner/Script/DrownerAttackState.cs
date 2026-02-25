using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrownerAttackState :EnemyState
{
    protected Boss_Drowner boss;
    public DrownerAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.Summon();
        boss.StateTimer_Drowner = 2f;
    }
    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Drowner <= 0)
            stateMachine.ChangeState(boss.downState);
    }
    public override void Exit()
    {
        base.Exit();
    }
}
