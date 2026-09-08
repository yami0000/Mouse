using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianIdleState : EnemyState
{
 protected Boss_Magician boss;
    public MagicianIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Magician boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }
    public override void Enter()
    {
        base.Enter(); 
        boss.StateTimer_Magician = !boss.EnterStageTwo? Random.Range(0.5f,1.5f) : Random.Range(0.3f, 1f);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
        if (boss.StateTimer_Magician <= 0)
        boss.RandomPattern();

    }
}
