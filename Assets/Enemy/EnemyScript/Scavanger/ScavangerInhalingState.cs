using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavangerInhalingState : EnemyState
{
    protected Enemy_Scavanger enemy;
    public ScavangerInhalingState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;

    }

    public override void Enter()
    {
        base.Enter();
        enemy.zerovelocity();
        enemy.cd.enabled = false;
        enemy.stateTimer_Scavanger = 2.5f;
       
    }

    public override void Exit()
    {
        base.Exit();
        enemy.CoolDown = enemy.CoolDownT;
        enemy.cd.enabled = true;
    }

    public override void Update()
    {
        base.Update();
        enemy.HeadToPlayer();
        enemy.SuckAmmo();
        if(enemy.energy>=5 || enemy.stateTimer_Scavanger<=0)
            stateMachine.ChangeState(enemy.releaseState);
    }
}

