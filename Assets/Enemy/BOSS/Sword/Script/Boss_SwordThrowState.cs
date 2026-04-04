using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordThrowState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordThrowState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.Throw(); 
        boss.idleType = 1;
        boss.StateTimer_Sword = 0.66f;
    }

    public override void Exit()
    {
        base.Exit();
       
        boss.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Sword <= 0)
        {  
            stateMachine.ChangeState(boss.battleState);
           
        }
          
    }
}
