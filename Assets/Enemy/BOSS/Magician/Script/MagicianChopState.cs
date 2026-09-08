using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianChopState : EnemyState
{
    protected Boss_Magician boss;

    public MagicianChopState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.zerovelocity();
        boss.SideDash();
        boss.StateTimer_Magician = 2.66f;
    }

    public override void Update()
    {
        base.Update();
        if(boss.StateTimer_Magician<=0)
            stateMachine.ChangeState(boss.idleState);
    }
    public override void Exit()
    {
        base.Exit();
    }

}
