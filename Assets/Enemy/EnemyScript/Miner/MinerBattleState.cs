using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MinerBattleState : EnemyState
{
    private Transform player;
    private Enemy_Miner enemy;
    private int moveDir;
    public MinerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Miner enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
        Debug.Log("Battle");
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit Battle");
    }

    public override void Update()
    {
        base.Update();

        if (!enemy.IsGroundDetected())
            enemy.Setvelocity(0.0000001f * moveDir, rb.velocity.y);


         


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
            //Debug.Log("player is not detected");
            if (Vector2.Distance(player.transform.position, enemy.transform.position) > 15)
                stateMachine.ChangeState(enemy.idleState);
        }


        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        if (enemy.IsGroundDetected())
            enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
        else
            enemy.Setvelocity(0.0000000000001f * moveDir, rb.velocity.y);

    }
}
