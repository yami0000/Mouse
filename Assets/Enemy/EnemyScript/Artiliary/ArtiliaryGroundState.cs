using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtiliaryGroundState :EnemyState
{
    protected Enemy_Artiliary enemy;

    protected Transform player;
    public ArtiliaryGroundState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Artiliary enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
            if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < 12)
            {
                if (enemy.CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }



    }
}
