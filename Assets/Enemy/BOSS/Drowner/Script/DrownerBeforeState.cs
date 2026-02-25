using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DrownerBeforeState : EnemyState
{
    protected Boss_Drowner boss;

    private Vector3 startPos;
    public DrownerBeforeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        startPos = boss.transform.position;
    }


    public override void Update()
    {
        base.Update();
        float yOffset = Mathf.PingPong(Time.time * boss.speed, boss.height);
        boss.transform.position = startPos + new Vector3(0, yOffset, 0);
        if (boss.IsPlayerDetected())
        {
            stateMachine.ChangeState(boss.downState);

        }

    }
    public override void Exit()
    {
        base.Exit();
    }
}
