using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyWalkState : EnemyState
{
    protected Enemy_Heavy enemy;

    public HeavyWalkState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.Setvelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);

        // Noticed, not detected: the Heavy is alerted from behind as well.
        if (enemy.IsPlayerNoticed())
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        // Wall ahead or no floor left, stop in idle and turn around from there.
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
            stateMachine.ChangeState(enemy.idleState);
    }
}
