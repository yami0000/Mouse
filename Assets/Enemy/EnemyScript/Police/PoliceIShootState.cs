using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceIShootState : EnemyState
{
    protected Enemy_Police enemy;
    private bool isBacked = false;  

    public PoliceIShootState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Police enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.stateTimer_Police = 1.5f;
        isBacked = false;
        enemy.ShotGun();
        enemy.Motion();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        if(enemy.stateTimer_Police <= 0.85f) 
        {
            if (!isBacked)
            {
                enemy.BackDown();
                isBacked = true;
            }
        }

        if (enemy.stateTimer_Police <= 0) 
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
