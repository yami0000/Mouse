using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalvaryBattleState : EnemyState
{
    protected Enemy_Calvary enemy;
    public CalvaryBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Calvary enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.Dodge();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if(enemy.ReadyToCharge)
        {
            enemy.ReadyToCharge = false;
            stateMachine.ChangeState(enemy.chargeState);
        }
    }
}
