using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicHealState : EnemyState
{
    private Enemy_Medic enemy;
    private int moveDir;
    public MedicHealState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Medic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        if ( enemy.transform.position.x < enemy._enemy.transform.position.x)
            moveDir = 1;
        else if ( enemy.transform.position.x >= enemy._enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);

    }

    public override void Exit()
    {
        base.Exit();
        enemy.stateTimer_Medic = enemy.attackCooldown;
    }

    public override void Update()
    {
        base.Update();
        enemy.zerovelocity();

        if (ReadyToAttack_Medic)
        {
            enemy.healAmmo.ExecuteEffect(enemy.transform);

            ReadyToAttack_Medic = false;
        }

        if (triggerCalled_Medic)
            stateMachine.ChangeState(enemy.battleState);
    }
}
