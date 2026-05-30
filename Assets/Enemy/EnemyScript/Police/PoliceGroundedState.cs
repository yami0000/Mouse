using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceGroundedState : EnemyState
{
    protected Enemy_Police enemy;
    protected Transform player;

    public PoliceGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Police enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        if (player != null)
            if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 10)
            {
                if (enemy.CanAttack())
                    stateMachine.ChangeState(enemy.battleState);
            }
    }
}
