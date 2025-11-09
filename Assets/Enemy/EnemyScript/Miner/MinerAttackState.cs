using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerAttackState : EnemyState
{
    protected Enemy_Miner enemy;

    protected Transform player;

    private bool Hasturned;

    private int moveDir;
    public MinerAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Miner enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.Instance.player.transform;

        Hasturned = false;
        enemy.stateTimer_Miner = 1.7f;




        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);

        enemy.laser.LaunchLaser();
    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();

        if(enemy.stateTimer_Miner <= 0 && !Hasturned)
        {

            if (player.position.x > enemy.transform.position.x)
                moveDir = 1;
            else if (player.position.x < enemy.transform.position.x)
                moveDir = -1;

            enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);

            Hasturned = true;
        }
        if (!enemy.laser.isAttack)

        {

            stateMachine.ChangeState(enemy.battleState);
        }


    }
}
