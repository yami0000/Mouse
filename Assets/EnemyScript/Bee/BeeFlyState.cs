using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeFlyState : EnemyState
{
    protected Enemy_Bee enemy;
    protected Transform player;
    public BeeFlyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Bee enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer_Bee = 3f;
        player = PlayerManager.Instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

         

        enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir, 0);


        if (stateTimer_Bee < 0)
        {
           
                 
            enemy.Flip();
                
             
            stateTimer_Bee = 4f;
        }

        if(player != null)
        if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 5f)
            stateMachine.ChangeState(enemy.battleState);
    }
}
