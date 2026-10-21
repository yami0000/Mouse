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
        else
        {

            if (Vector2.Distance(player.transform.position, enemy.transform.position) > 16)
                stateMachine.ChangeState(enemy.idleState);
        }

 
      
        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);


        if (distanceToPlayer > enemy.battleStopDistance && enemy.IsGroundDetected())
            enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);
        else if (enemy.IsGroundDetected())
            enemy.zerovelocity();
    }
}