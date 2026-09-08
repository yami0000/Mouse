using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianWalkState : EnemyState
{
 protected Boss_Magician boss;
    public MagicianWalkState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.TurnToPlayer();

        boss.StateTimer_Magician = boss.EnterStageTwo ? Random.Range(0.3f, 1f):Random.Range(0.5f, 1.6f);
    }

    public override void Exit()
    {
        base.Exit();
        boss.TurnToPlayer();
    }

    public override void Update()
    {
        base.Update();
        boss.Setvelocity(boss.moveSpeed * boss.facingDir, 0);
        if (boss.StateTimer_Magician <= 0)
            boss.RandomPattern();


    }

    
}
