using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ScavangerBattleState : EnemyState
{
    protected Enemy_Scavanger enemy;
    public ScavangerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.zerovelocity();
        enemy.Move();
    }

    public override void Exit()
    {
        base.Exit();
        if(enemy.C != null)
        {
            enemy.StopCoroutine(enemy.C);
            enemy.C = null;  
            enemy.HeadToPlayer();
            
        }
    }

    public override void Update()
    {
        base.Update();
        if (enemy.CoolDown <= 0)
        {
            if (enemy.DetectAmmo())
                stateMachine.ChangeState(enemy.inhaleState);
        }
        if(enemy.AmmoCoolDown <= 0)
        {
            stateMachine.ChangeState(enemy.shootState);

        }
        if (enemy.Run)
        {
            enemy.Run = false;
            stateMachine.ChangeState(enemy.flyState);

        }
         
    }
}
