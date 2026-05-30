using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceBattleState : EnemyState
{
    protected Enemy_Police enemy;
    private Transform player;

    public PoliceBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Police enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
                    stateMachine.ChangeState(Vector2.Distance(player.transform.position, enemy.transform.position) <enemy.threshold ? enemy.iattackState : enemy.wattackState);


            }
        }
        else
        {
             
            if (Vector2.Distance(player.transform.position, enemy.transform.position) > 15)
                stateMachine.ChangeState(enemy.idleState);
        }

        if (enemy.IsGroundDetected())
            enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);
        else
            enemy.zerovelocity();
             
     
    }
}