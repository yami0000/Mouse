using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerIdleState : MinerGroundedState

{
    public MinerIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Miner enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {

    }

    public override void Enter()
    {
        base.Enter();

        enemy.stateTimer_Miner = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.zerovelocity();
        if (enemy.stateTimer_Miner < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);


        }
    }
}
