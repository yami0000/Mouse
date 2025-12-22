using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavangerFlyState :EnemyState
{
    protected Enemy_Scavanger enemy;
    protected Transform player;
    public ScavangerFlyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Scavanger = 3f;
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


        if (enemy.stateTimer_Scavanger < 0)
        {


            enemy.Flip();

            enemy.stateTimer_Scavanger = 3f;

        }

        if (player != null)
            if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 10f)
                stateMachine.ChangeState(enemy.battleState);
    }
}
