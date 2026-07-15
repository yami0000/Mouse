using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianSpikeState : EnemyState
{
  protected Boss_Magician boss;
    public MagicianSpikeState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }
    public override void Enter()
    {
        base.Enter();
        boss.TurnToPlayer();
        boss.zerovelocity();
       
        
        boss.StateTimer_Magician = 0.75f;  
    }
    public override void Exit()
    {
        base.Exit(); 
        boss.Spike();
    }
    public override void Update()
    {
        base.Update();
        if(boss.StateTimer_Magician <= 0)
            stateMachine.ChangeState(boss.idleState);
    }
}
