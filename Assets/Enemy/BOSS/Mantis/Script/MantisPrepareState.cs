using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisPrepareState : EnemyState
{
    private Boss_Mantis boss;
    public MantisPrepareState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.stateTimer_Mantis = 10.2f;
    }

    public override void Exit()
    {   
        boss.transform.rotation = Quaternion.Euler(0, 180, 0);
        base.Exit();
        
    }

    public override void Update()
    {
        base.Update();
        if(boss.stateTimer_Mantis <= 0)
          stateMachine.ChangeState(boss.staticState);
        
    } 
}
