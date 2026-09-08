using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianDeathState : EnemyState
{
  protected Boss_Magician boss;
    public MagicianDeathState(Boss_Magician magician, EnemyStateMachine stateMachine, string animBoolName, Boss_Magician boss) : base(magician, stateMachine, animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        boss.anim.speed = 0;
        boss.cd.enabled = false;

         

        boss.StateTimer_Magician = .1f;
    }

    public override void Update()
    {
        base.Update();
        if (boss.StateTimer_Magician > 0)
            rb.velocity = new Vector2(0, 5);
    }
    public override void Exit()
    {
        base.Exit();
    }

}
