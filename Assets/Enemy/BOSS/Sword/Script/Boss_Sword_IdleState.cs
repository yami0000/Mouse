using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Sword_IdleState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_Sword_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
