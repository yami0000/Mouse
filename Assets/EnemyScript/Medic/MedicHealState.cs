using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicHealState : EnemyState
{
    private Enemy_Medic enemy;
    private int moveDir;
    private int randomF;
    public MedicHealState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Medic enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        if (enemy._enemy != null && enemy.IsPlayerDetectedAll())//都有
        {
            randomF = Random.Range(0, 10);
            if (randomF <= 5)
                headingToEnemy();
            else
                headingToPlayer();
            
            

        }
        else if (enemy._enemy != null && !enemy.IsPlayerDetectedAll())//只有敌人
            randomF = 0;
        else if (enemy._enemy == null && enemy.IsPlayerDetectedAll())//只有玩家
            randomF = 10;
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

            if (randomF <= 5)
            {
                headingToEnemy();
                enemy.healAmmo.ExecuteEffect(enemy.transform);
            }
            else
            {
                headingToPlayer();
                enemy.ammo.ExecuteEffect(enemy.transform); }

            ReadyToAttack_Medic = false;
        }

        if (triggerCalled_Medic)
            stateMachine.ChangeState(enemy.battleState);
    }
    private void headingToPlayer()
    {
        if (enemy.transform.position.x < PlayerManager.Instance.player.transform.position.x)
            moveDir = 1;
        else if (enemy.transform.position.x >= PlayerManager.Instance.player.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    private void headingToEnemy()
    {
        if (enemy.transform.position.x < enemy._enemy.transform.position.x)
            moveDir = 1;
        else if (enemy.transform.position.x >= enemy._enemy.transform.position.x)
            moveDir = -1;

        enemy.Setvelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }
}
