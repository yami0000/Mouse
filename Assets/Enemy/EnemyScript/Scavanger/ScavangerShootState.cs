using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ScavangerShootState : EnemyState
{
    protected Enemy_Scavanger enemy;
    public ScavangerShootState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.Shoot();
        enemy.stateTimer_Scavanger = 1.75f;

    }

    public override void Exit()
    {
        base.Exit();
       
        enemy.AmmoCoolDown = enemy.CoolDownA - (enemy.energy / 2)*1.5f;
    }

    public override void Update()
    {
        base.Update();
        enemy.HeadToPlayer();
        if(enemy.stateTimer_Scavanger <=0)
        {
            enemy.energy--;
            enemy.energy = Mathf.Clamp(enemy.energy, 0, enemy.MaxEnergy);
            stateMachine.ChangeState(enemy.flyState);
        }

    }
}

