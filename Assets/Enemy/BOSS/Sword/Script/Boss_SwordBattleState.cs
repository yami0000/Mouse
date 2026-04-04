using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boss_SwordBattleState : EnemyState
{
    protected Boss_Sword boss;

    public Boss_SwordBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Sword boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }

    public override void Enter()
    {
        boss.anim.SetFloat("IdleType", boss.idleType);
        base.Enter();

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (Vector2.Distance(boss.transform.position, GetPlayerPosition()) > 8)
        {
            boss.TurnToPlayer();
            boss.Setvelocity(boss.moveSpeed * boss.facingDir, 0);
        }
        else
            boss.Setvelocity(boss.moveSpeed * boss.facingDir, 0);

        if (boss.CanAttack())
        {
            if (boss.idleType == 0)
                stateMachine.ChangeState(boss.hideState);
            if (boss.idleType == 1)
                stateMachine.ChangeState(Random.value < 0.5f ? boss.doubleShootState : boss.retrieveState);
        }

        if (Vector2.Distance(boss.transform.position, GetPlayerPosition()) >= boss.playerCheckDistance)
            stateMachine.ChangeState(boss.idleState);
    }
    public static Vector3 GetPlayerPosition()
    {
        return PlayerManager.Instance.player.transform.position;
    }

    
}
