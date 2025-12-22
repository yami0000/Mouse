using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalvaryChargeState : EnemyState
{
    protected Enemy_Calvary enemy;
    protected Transform player;
    private int moveDir;
    public CalvaryChargeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Calvary enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
        enemy.ChangeRotation();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
         if(enemy.Stopped)
        {
            enemy.Stopped = false;
            stateMachine.ChangeState(enemy.recoverState);

        }
      
    }
}
