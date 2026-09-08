using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBattleState : EnemyState
{
    protected Enemy_Zombie enemy;
    private Transform player;

    public ZombieBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Zombie enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.TurnToPlayer();
        if (enemy.IsPlayerDetected())

        {


            if (Vector2.Distance(player.transform.position, enemy.transform.position) < enemy.attackDistance)
            {

                if (enemy.CanAttack())
                    stateMachine.ChangeState(enemy.attackState);


            }
        }
 

    }
}