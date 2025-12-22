using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalvaryFlyState : EnemyState
{
    protected Enemy_Calvary enemy;
    protected Transform player;
    public CalvaryFlyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Calvary enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.Reset();
        enemy.stateTimer_Calvary  = 3f;
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


        if (enemy.stateTimer_Calvary < 0)
        {


            enemy.Flip();

            enemy.stateTimer_Calvary = 3f;

        }

        if (player != null)
            if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 5f)
                stateMachine.ChangeState(enemy.battleState);
    }
}
