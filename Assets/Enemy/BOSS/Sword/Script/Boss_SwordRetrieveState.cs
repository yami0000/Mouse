using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordRetrieveState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordRetrieveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.Retrieve();
        boss.StateTimer_Sword = 1.583f;
        boss.bladeS.AttackedOnce = false;
    }

    public override void Exit()
    {
        base.Exit();
        boss.idleType = 0;
    }

    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
        if (boss.StateTimer_Sword <= 0)
            stateMachine.ChangeState(boss.battleState);
    }
}
