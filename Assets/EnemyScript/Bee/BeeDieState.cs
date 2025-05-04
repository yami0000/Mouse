using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeDieState : EnemyState
{
    private Enemy_Bee enemy;
    public BeeDieState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Bee enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

        stateTimer_Bee = .1f;
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
