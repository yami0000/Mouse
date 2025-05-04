using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderGroundedState : EnemyState
{
    protected Enemy_Spider enemy;

    protected Transform player;
    public SpiderGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Spider enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        if(enemy.IsPlayerDetected()|| Vector2.Distance(enemy.transform.position,player.position)<2)
            stateMachine.ChangeState(enemy.battleState);
    }
}
