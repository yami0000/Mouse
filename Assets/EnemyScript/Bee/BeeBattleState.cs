using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeBattleState : EnemyState
{
    private Transform player;
    private Enemy_Bee enemy;
    private int moveDir;
    public BeeBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Bee enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

         

        if (Vector2.Distance(player.transform.position, enemy.transform.position) <6.5)
        {
            stateTimer_Bee = enemy.battleTime;
            if (Vector2.Distance(player.transform.position, enemy.transform.position) < enemy.attackDistance)
            {

                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);


            }
        }
        else
        {
            if (stateTimer_Bee < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 8)
                stateMachine.ChangeState(enemy.flyState);
        }


        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    private bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttacked_Bee + enemy.attackCooldown)
        {
            enemy.lastTimeAttacked_Bee = Time.time;
            return true;
        }

        return false;

    }
}
