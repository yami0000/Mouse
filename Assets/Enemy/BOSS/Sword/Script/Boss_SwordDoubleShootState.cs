using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordDoubleShootState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordDoubleShootState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.DoubleShoot();
        boss.StateTimer_Sword = 2.3f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Sword <= 0)
            stateMachine.ChangeState(boss.battleState);
    }
}
