using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrownerDownState : EnemyState
{
    protected Boss_Drowner boss;

 
    public DrownerDownState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.cd.enabled = false;
        boss.RD();
        boss.StateTimer_Drowner = boss.riseDuration+boss.diveDuration;
    }
    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Drowner <= 0)
            stateMachine.ChangeState(boss.submergeState);


    }
    public override void Exit()
    {
        base.Exit();
    }
}
