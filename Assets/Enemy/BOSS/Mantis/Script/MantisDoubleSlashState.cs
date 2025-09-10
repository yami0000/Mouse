using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisDoubleSlashState : EnemyState
{
    private Boss_Mantis boss;
    public MantisDoubleSlashState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.stateTimer_Mantis = 1.25f;
        boss.TurnToPlayer();

        boss.StartDash();
    }
    public override void Update()
    {
        base.Update();    
        if (boss.stateTimer_Mantis <= 0)
            stateMachine.ChangeState(boss.staticState);


    }

    public override void Exit()
    {
        base.Exit();
    
    }

}
