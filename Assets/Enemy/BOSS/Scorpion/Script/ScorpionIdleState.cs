using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScorpionIdleState : ScorpionGroundedState
{
    
    public ScorpionIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName,boss)
    {
         

    }

    public override void Enter()
    {
        base.Enter();
        R = true;
        boss.isMoving = true;
       
        if (boss.StateNum == 0)
        {
            boss.StateNum = Random.Range(1, 4);
            boss.StateTime1 = Random.Range(0, 1.5f);
            boss.StateTime2 = Random.Range(0f, 2f);
            boss.StateTime3 = Random.Range(0f, 1f);
            boss.stateTimer_Scorpion = boss.StateTime1;
        }
        if (boss.StateNum == 1)
            boss.stateTimer_Scorpion = boss.StateTime3;
        
    }

        public override void Update()
    {
        base.Update();
        if (boss.stateTimer_Scorpion <= 0 && R)
        { 
            boss.StateNum -= 1;
            if(boss.StateNum == 2) 
            {
                stateMachine.ChangeState(boss.walkState);
            }
            if (boss.StateNum == 1)
            {
                stateMachine.ChangeState(boss.walkState);
            }
            if (boss.StateNum == 0) 
            {
                boss.isMoving = false;
            }
            R = false;
        }
    }
         

       

    public override void Exit()
    {
        base.Exit();
    }
}
