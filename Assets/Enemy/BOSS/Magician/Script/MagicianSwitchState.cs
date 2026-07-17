using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianSwitchState : EnemyState
{
    protected Boss_Magician boss;

    public MagicianSwitchState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.StateTimer_Magician = 0.9166f;
       
    }

    public override void Update()
    {
        base.Update(); 
        if (boss.StateTimer_Magician <= 0f)
           stateMachine.ChangeState(boss.idleState);
    }
    public override void Exit()
    {
        base.Exit();
        boss.anim.SetFloat("_Axe", 1); 
        boss.EnterStageTwo = true;

    }

}
