using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordChopState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordChopState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.Chop();
        boss.StateTimer_Sword = 0.67f;
    }

    public override void Exit()
    {
        base.Exit();
        boss.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
        if (boss.StateTimer_Sword <= 0)
          stateMachine.ChangeState(boss.battleState);
       
    }

    public static Vector3 GetPlayerPosition()
    {
        return PlayerManager.Instance.player.transform.position;
    }
}
