using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDesIdleState : SelfDesGroundedState
{
     
    public SelfDesIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_SelfDes enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
         
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer_SelfDes = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer_SelfDes < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        }
    }
}
