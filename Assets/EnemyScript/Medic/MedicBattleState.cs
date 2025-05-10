using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MedicBattleState : EnemyState
{
    protected Enemy_Medic enemy;
    private int MoveDir;
    public MedicBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Medic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        Tracing();
        if(CanAttack())
            stateMachine.ChangeState(enemy.healState);
    }

    private void Tracing()
    {
        float offsetX = enemy._enemy.facingDir * enemy.followingDistance;
        float targetX = enemy._enemy.transform.position.x - offsetX;
        float currentX = enemy.transform.position.x;
        float enemyX = enemy._enemy.transform.position.x;

        int moveDir = (currentX < targetX) ? 1 : -1;
        int facingDir = (currentX < enemyX) ? 1 : -1;
        float distance = Mathf.Abs(currentX - targetX);


        if (distance > 1f)
        {

            enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
        }
        else
        {

            enemy.Setvelocity(0.00001f * facingDir, rb.velocity.y);
        }
    }
    private bool CanAttack()
    {
        if (enemy.stateTimer_Medic <= 0)
        {
            
            return true;
        }

        return false;

    }
}
