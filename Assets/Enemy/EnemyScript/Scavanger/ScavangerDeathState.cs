using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavangerDeathState : EnemyState
{
    protected Enemy_Scavanger enemy;
    public ScavangerDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Scavanger enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.anim.SetBool(enemy.lastAnimBoolName, true);
        enemy.anim.speed = 0;
        enemy.cd.enabled = false;
        rb.gravityScale = 2.5f;

        enemy.stateTimer_Scavanger = .1f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer > 0)
            rb.velocity = new Vector2(0, 5);
    }
}

