using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDesBattleState : EnemyState
{
    private Transform player;
    protected Enemy_SelfDes enemy;
    private int moveDir;
    public SelfDesBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_SelfDes enemy) : base(_enemyBase, _stateMachine, _animBoolName )
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

         
        if (enemy.IsPlayerDetected())
        {
            
            stateTimer_SelfDes = enemy.battleTime;

            float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);
 

            if (distanceToPlayer < enemy.attackDistance)
            {

                
                    stateMachine.ChangeState(enemy.explodeState);


            }

        }
        else
        {
            if (stateTimer_SelfDes < 0 || Vector2.Distance(player.position, enemy.transform.position) > 15)
                stateMachine.ChangeState(enemy.idleState);
        }


        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir* enemy.speedMultiplyer, rb.velocity.y);
    }

 
}
