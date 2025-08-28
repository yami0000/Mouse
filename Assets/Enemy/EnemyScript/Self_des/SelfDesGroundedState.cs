using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDesGroundedState : EnemyState
{
    protected Enemy_SelfDes enemy;

    protected Transform player;
    public SelfDesGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_SelfDes enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
            if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 4)
               
                stateMachine.ChangeState(enemy.battleState);
    }


}
