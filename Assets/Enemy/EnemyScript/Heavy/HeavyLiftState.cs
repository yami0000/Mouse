using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pattern 2, step one of three: stand still and lift the axe into position.
///
/// Fixed order, no way out: this state only ever leads to HeavyChargeState, which only ever
/// leads to HeavyUpstrokeState. Once the Heavy commits to the charge he plays all three.
///
/// This is also where the charge direction is locked in, because the run itself never turns
/// to follow the player. Dodging a committed charge is the point of it.
/// </summary>
public class HeavyLiftState : EnemyState
{
    protected Enemy_Heavy enemy;

    private float elapsed;

    public HeavyLiftState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.lastTimeAttacked = Time.time;
        elapsed = 0f;

        // The last aim of the whole pattern.
        enemy.TurnToPlayer();
        enemy.zerovelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        elapsed += Time.deltaTime;

        enemy.zerovelocity();

        // triggerCalled comes from an AnimationTrigger event at the end of the lift clip,
        // the timer is the fallback for clips with no event on them.
        if (elapsed >= enemy.liftTime)
            stateMachine.ChangeState(enemy.chargeState);
    }
}
