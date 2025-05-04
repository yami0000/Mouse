using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDesDeathState : EnemyState
{
    private Enemy_SelfDes enemy;
    public SelfDesDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_SelfDes enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.explode.ExecuteEffect_Explode(enemy.transform);

        enemy.anim.SetBool(enemy.lastAnimBoolName, true);
        enemy.anim.speed = 0;
        enemy.cd.enabled = false;

        stateTimer = .1f;
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
