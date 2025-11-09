using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicAttackState : EnemyState
{
    private Enemy_Mechanic enemy;
    private Transform player;
    private bool hasTriggered;
    private int moveDir;
    public MechanicAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Mechanic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
        enemy.stateTimer_Mechanic = 2.2f;
        hasTriggered = false;
    }
    public override void Update()
    {
        base.Update();


        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;
        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);

        enemy.zerovelocity();



        if (enemy.stateTimer_Mechanic <= 0.6f && !hasTriggered)
        {

            enemy.projectile.ExecuteEffect(enemy.transform);
            hasTriggered = true;

        }

        if (enemy.stateTimer_Mechanic <= 0f)
        {

            stateMachine.ChangeState(enemy.battleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        //enemy.stateTimer_Mechanic = enemy.attackCooldown;
        enemy.lastTimeAttacked = Time.time;
    }
}
