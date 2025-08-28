using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtiliaryIdleState : ArtiliaryGroundState
{
    public ArtiliaryIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Artiliary enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();

        enemy.stateTimer_Artiliary = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.stateTimer_Artiliary < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        }
    }
}
