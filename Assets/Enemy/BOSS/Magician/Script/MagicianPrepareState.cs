using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianPrepareState : EnemyState
{
protected Boss_Magician boss;
    public MagicianPrepareState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Magician boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }
    public override void Enter()
    {
        base.Enter();
        boss.StateTimer_Magician = 3.4f;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
        
        if (boss.StateTimer_Magician <= 0)
        {
            stateMachine.ChangeState(boss.idleState);
        }
    }
}
