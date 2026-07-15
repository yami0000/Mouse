using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianBarrageState : EnemyState
{
    protected Boss_Magician boss;
    public MagicianBarrageState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }
    public override void Enter()
    {
        base.Enter();
        boss.TurnToPlayer();
        boss.zerovelocity();
        boss.Barrage();
        boss.StateTimer_Magician = 1.5833f;
    }


    public override void Exit()
    {
        base.Exit();
      
    }

    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Magician < 0)
        {
            stateMachine.ChangeState(boss.idleState);

        }
    }
     
}
