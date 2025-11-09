using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicDeathState : EnemyState
{
    private Enemy_Mechanic enemy;
    public MechanicDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Mechanic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

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
