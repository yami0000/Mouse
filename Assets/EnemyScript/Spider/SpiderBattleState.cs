using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBattleState : EnemyState

{
    private Transform player;
    private Enemy_Spider enemy;
    private int moveDir;
    public SpiderBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Spider enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
            Debug.Log("player is detected");
            stateTimer = enemy.battleTime;
            if (Vector2.Distance(player.transform.position, enemy.transform.position) < enemy.attackDistance)
            {

                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);


            }
        }
        else {
            Debug.Log("player is not detected");
            if (stateTimer < 0 || Vector2.Distance(player.transform.position,enemy.transform.position)>6)
                stateMachine.ChangeState(enemy.idleState);
        }
        

        if(player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    private bool CanAttack()
    {
    if(Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown) 
        {
            enemy.lastTimeAttacked = Time.time;
            return true;
        }

        return false;
    
    }

}
