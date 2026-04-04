using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SwordHideState : EnemyState
{
    Boss_Sword boss;
    private int P;
    public Boss_SwordHideState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss= boss;

        
    }

    public override void Enter()
    {
        base.Enter();
        P = Random.Range(0, 2);

        if(P==1)
        boss.HideAndShow();
        else
        boss.HideAndShoot();
        boss.StateTimer_Sword = 2*boss.transitionDuration + boss.moveDuration;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (boss.StateTimer_Sword <= 0)
        {
            if (P == 1)
                stateMachine.ChangeState(boss.chopState);
            else
                stateMachine.ChangeState(boss.shootState);

        }
    }
}
