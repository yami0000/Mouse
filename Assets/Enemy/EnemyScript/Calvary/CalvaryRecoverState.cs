using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalvaryRecoverState : EnemyState
{
    protected Enemy_Calvary enemy;
    private bool Once;
    public CalvaryRecoverState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Calvary enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Calvary = 0.5f;
        Once = true;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.stateTimer_Calvary <= 0)
        {
            if (Once)
            {
                Once = false;
                enemy.ReturnToNormal();
            }
        }
        if (enemy.Over)
        {
            enemy.zerovelocity();
            enemy.Over= false;
            
            stateMachine.ChangeState(enemy.flyState);

        }
    }

    
}
