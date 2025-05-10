using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicGroundState : EnemyState
{

    protected Enemy_Medic enemy;

    protected Transform player;
    public MedicGroundState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Medic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
       this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();


        enemy.pinpointClosestEnemy();
        if (enemy._enemy != null)
            stateMachine.ChangeState(enemy.battleState);


    }
}
