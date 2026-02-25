using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrownerSubmergeState : EnemyState
{
    protected Boss_Drowner boss;

    private int P;
 
    public DrownerSubmergeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Drowner boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        
        boss.AdjustPosition();
        boss.cd.enabled = false;
        boss.StateTimer_Drowner = 2f;

        P = Random.Range(0,100);
    }


    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Drowner <= 0f)
        {
            if (P < 25 && !boss.isFalling) 
            {
                if (boss.Num == 1)
                    boss.F.Falling();
                else if (boss.Num == 2) 
                    boss.S.Falling();
                else
                    boss.T.Falling();

                boss.StateTimer_Drowner = 2f;
            }
        else
            stateMachine.ChangeState(boss.UpState);
        }

    }
    public override void Exit()
    {
        base.Exit();
    }
}
