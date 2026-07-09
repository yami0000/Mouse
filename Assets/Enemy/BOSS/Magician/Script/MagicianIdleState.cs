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
        boss.StateTimer_Magician = 1f;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
  
    }
}
