using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordShootState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordShootState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.Shoot();
        boss.StateTimer_Sword = 1.665f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Sword <= 0)
            stateMachine.ChangeState(boss.throwState);
    }
}
