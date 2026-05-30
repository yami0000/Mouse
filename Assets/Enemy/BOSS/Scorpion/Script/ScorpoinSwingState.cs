using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionSwingState : EnemyState 
{

    private Boss_Scorpion boss;


    public ScorpionSwingState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.stateTimer_Scorpion = 1f;
        boss.Swing();
        boss.Motion();
    }

    public override void Exit()
    {
        base.Exit();
        boss.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (boss.stateTimer_Scorpion <= 0)
        {
            stateMachine.ChangeState(boss.idleState);
        }
    }
}
  

