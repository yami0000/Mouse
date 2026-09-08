using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackState : EnemyState
{
    protected Enemy_Zombie enemy;
    public ZombieAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Zombie enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Zombie = 1f;
    }

    public override void Update()
    {
        base.Update();
        
        if (enemy.stateTimer_Zombie < 0)
        {

            stateMachine.ChangeState(enemy.battleState);


        }
    }

    public override void Exit()
    {
        base.Exit();
    }


}
