using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionLaserState : EnemyState
{
    private Boss_Scorpion boss;

    protected Transform player;

    private int moveDir;
    public ScorpionLaserState(Enemy _bossBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_bossBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;

    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;




        if (player.position.x > boss.transform.position.x)
            moveDir = 1;
        else if (player.position.x < boss.transform.position.x)
            moveDir = -1;

        boss.Setvelocity(boss.moveSpeed * moveDir, rb.velocity.y);

        boss.laser.LaunchLaser();
    }

    public override void Exit()
    {
        base.Exit();
        boss.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        boss.zerovelocity();



        if (!boss.laser.isAttack)

        {

            stateMachine.ChangeState(boss.idleState);
        }


    }
}

