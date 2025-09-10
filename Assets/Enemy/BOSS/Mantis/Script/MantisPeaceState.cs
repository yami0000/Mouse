using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisPeaceState : EnemyState
{
    private Boss_Mantis boss;
    public MantisPeaceState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
    }


    public override void Update()
    {
        base.Update();
        if(GM.Instance.GameManager.isMantisBossFightStarted)
            stateMachine.ChangeState(boss.prepareState);
    }
    public override void Exit()
    {
        base.Exit();
    }
}
