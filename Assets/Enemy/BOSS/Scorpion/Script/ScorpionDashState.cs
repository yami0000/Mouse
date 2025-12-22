using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionDashState : EnemyState
{
    private Boss_Scorpion boss;
    private float P;
    public ScorpionDashState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.sprint();
        boss.stateTimer_Scorpion =boss.sprintTime;
        P = Random.Range(0, 100);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(boss.stateTimer_Scorpion<=0)
        {
            if (P <= 50)
            { 
                stateMachine.ChangeState(boss.swingState); 
            }
            else
            {
                
                stateMachine.ChangeState(boss.meleeState);
            }
        }

    }
}
