using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionWalkState : ScorpionGroundedState
{

    public ScorpionWalkState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName, boss)
    {


    }

    public override void Enter()
    {
        base.Enter();
        boss.stateTimer_Scorpion = boss.StateTime2;
        R = true;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Vector2.Distance(boss.transform.position, GetPlayerPosition()) > 2)
        {
            boss.TurnToPlayer();
            boss.Setvelocity(boss.moveSpeed * boss.moveDir, 0);
        }
        else
            boss.Setvelocity(boss.moveSpeed * boss.moveDir, 0);

        if (boss.stateTimer_Scorpion <= 0 && R)
        {
            boss.StateNum -= 1;
            if (boss.StateNum == 1)
            {
                stateMachine.ChangeState(boss.idleState);

            }
            if (boss.StateNum == 0)
            {
                boss.isMoving = false;
            }
            R = false;
        }
    }
}
