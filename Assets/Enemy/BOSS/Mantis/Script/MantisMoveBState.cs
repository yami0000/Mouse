using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisMoveBState : EnemyState
{
    private Boss_Mantis boss;
    public MantisMoveBState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.stateTimer_Mantis = 1.25f;
        boss.isBackingup = true;
    }

    public override void Exit()
    {
        base.Exit();
        boss.isBackingup = false;
    }

    public override void Update()
    {
        base.Update();
        boss.Setvelocity(boss.moveSpeed * boss.facingDir, rb.velocity.y);
        if (boss.stateTimer_Mantis <= 0)
            boss.DecideMove();
    }
}
