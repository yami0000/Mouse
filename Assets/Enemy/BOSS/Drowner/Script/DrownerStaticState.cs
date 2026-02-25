using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrownerStaticState : EnemyState
{
    protected Boss_Drowner boss;

    private int a;
    public DrownerStaticState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.StateTimer_Drowner = 0.333f*a;
    }


    public override void Update()
    {
        base.Update();
        

    }
    public override void Exit()
    {
        base.Exit();
    }
}
