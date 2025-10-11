using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisDeathState : EnemyState
{
    private Boss_Mantis boss;
    public MantisDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.anim.SetBool(boss.lastAnimBoolName, true);
        boss.anim.speed = 0;
        boss.cd.enabled = false;

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
        {
            rb.velocity = new Vector2(0, 5);
            GM.Instance.GameManager.isMantisAlive = false;
        }
    }
}
