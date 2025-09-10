using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisStaticState : EnemyState
{
    private int moveDir;
    private Boss_Mantis boss;
    protected Transform player;
    private float P;
     
    public MantisStaticState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Mantis boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
        boss.stateTimer_Mantis = 1f;
        P = Random.Range(0, 100);
        
    }


    public override void Update()
    {
        base.Update();
        boss.TurnToPlayer();

        if (boss.stateTimer_Mantis <= 0)
        {
            if (P < 60)
                stateMachine.ChangeState(boss.moveFState);
            else if (60 <= P && P <= 75)
                stateMachine.ChangeState(boss.moveBState);
            else
                boss.DecideMove(); 

        }

    }
    public override void Exit()
    {
        base.Exit();
    }

 
}
