using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionMeleeState : EnemyState
{
    private Boss_Scorpion boss;
     
    public ScorpionMeleeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.melee();
        boss.stateTimer_Scorpion = 0.83f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (boss.stateTimer_Scorpion <= 0)
            stateMachine.ChangeState(boss.idleState);
    }
}
