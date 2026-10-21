using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks which of the two patterns the Heavy commits to, then manoeuvres into position for it.
///
/// The choice is made once per attack, on entering this state, because the two patterns want
/// opposite spacing: the chop wants to be close, the charge wants a runway. Rolling every
/// frame would leave him dithering on the spot, so he commits and then walks in or backs off
/// to earn the range his chosen pattern needs.
/// </summary>
public class HeavyBattleState : EnemyState
{
    private enum HeavyPlan { Chop, Charge }

    protected Enemy_Heavy enemy;
    private Transform player;

    private HeavyPlan plan;
    private float giveGroundTimer;

    public HeavyBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.Instance.player.transform;

        // base.Enter has just raised the move bool, so start in step with it.
        isMoving = true;

        // Fresh budget per attack, so giving ground can never become his whole behaviour.
        giveGroundTimer = 0f;

        // One roll per attack. Coming back here after a swing is what re-rolls it.
        plan = enemy.WantsToCharge(enemy.DistanceToPlayerX()) ? HeavyPlan.Charge : HeavyPlan.Chop;
    }

    public override void Exit()
    {
        base.Exit();

        ClearMoveAnimBools();
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        float distanceX = enemy.DistanceToPlayerX();
        float distance = Vector2.Distance(player.position, enemy.transform.position);

        // Player got away, go back to patrolling.
        if (!enemy.IsPlayerNoticed() && distance > enemy.loseTargetDistance)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        // A crowded charge is a shove, not a run up. Asking it for a full runway is what made
        // him walk backwards for ever while the player stayed on top of him, so at this range
        // he needs no room at all and goes straight into the lift.
        bool crowded = distanceX < enemy.chargeCrowdedDistance;

        float roomNeeded;

        if (plan == HeavyPlan.Charge)
            roomNeeded = crowded ? 0f : enemy.chargeMinDistance;
        else
            roomNeeded = enemy.chopRangeMin;

        // Giving ground is on a budget. Once it runs out he attacks from where he stands, so a
        // player who keeps following can never walk him backwards indefinitely.
        bool settleForHere = giveGroundTimer >= enemy.maxGiveGroundTime;

        if (distanceX < roomNeeded && !settleForHere)
        {
            // Face away first, the ground and wall checks read whichever way he is facing.
            enemy.TurnAwayFromPlayer();

            if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
            {
                // Backed into a wall or a ledge, so there is no room to be had. He keeps the
                // pattern he rolled and simply throws it from here.
                settleForHere = true;
                StandStill();
            }
            else
            {
                giveGroundTimer += Time.deltaTime;
                MoveAt(enemy.moveSpeed);
                return;
            }
        }

        enemy.TurnToPlayer();

        if (plan == HeavyPlan.Charge)
        {
            // Enough runway, so lift the axe as soon as the cooldown allows. From here the
            // three charge states run in a fixed order with no way back to this one until
            // the upstroke finishes.
            if (enemy.CanAttack())
                stateMachine.ChangeState(enemy.liftState);
            else
                StandStill();   // hold the distance instead of walking the runway away

            return;
        }

        // Chop: close the gap, then swing once inside the band.
        if (distanceX > enemy.chopRangeMax)
        {
            if (enemy.IsGroundDetected() && !enemy.IsWallDetected())
                MoveAt(enemy.moveSpeed);
            else
                StandStill();

            return;
        }

        StandStill();

        // Out of room or out of budget means he is inside his own reach with nowhere to back
        // off to, so the lower bound is waived rather than letting him stand there doing nothing.
        bool inChopBand = distanceX >= enemy.chopRangeMin || settleForHere;

        if (inChopBand && enemy.CanAttack())
            stateMachine.ChangeState(enemy.chopState);
    }
}
