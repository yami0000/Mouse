using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomAttackState : EnemyState
{
    protected Enemy_Mushroom enemy;

    private float attackTimeElapsed;
    private bool hasThrown;

    public MushroomAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Mushroom enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.lastTimeAttacked = Time.time;
        attackTimeElapsed = 0f;
        hasThrown = false;

        // Aim before the throw, then plant the feet for the whole animation.
        enemy.TurnToPlayer();
        enemy.zerovelocity();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.zerovelocity();
    }

    public override void Update()
    {
        base.Update();

        attackTimeElapsed += Time.deltaTime;

        // Throwing is a planted animation, the mushroom holds still all the way through.
        enemy.zerovelocity();

        // ReadyToAttack comes from an AttackPreparetion animation event, the timer is the
        // fallback so the throw still happens on clips with no event on them.
        if (!hasThrown && (ReadyToAttack || attackTimeElapsed >= enemy.throwDelay))
        {
            enemy.ThrowBomb();
            hasThrown = true;
        }

        // Likewise triggerCalled comes from an AnimationTrigger event at the end of the clip.
        if (hasThrown && (triggerCalled || attackTimeElapsed >= enemy.attackDuration))
            stateMachine.ChangeState(enemy.battleState);
    }
}
