using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pattern 2, step two of three: run in a straight line with the axe levelled.
/// The static hitbox lives exactly as long as this state, and the run never re-aims, so the
/// direction chosen during the lift is the direction he commits to.
///
/// Fixed order: this state is only ever entered from HeavyLiftState and only ever leaves to
/// HeavyUpstrokeState, however the run ends.
///
/// The run has two phases. First he sprints. Then something ends the sprint and he skids: the
/// weight carries him Brake Distance further while he slows to a stop, and only then does he
/// swing. Braking inside this state rather than in a fourth one keeps the pattern three states
/// long, and keeps the levelled axe alive for the whole skid.
/// </summary>
public class HeavyChargeState : EnemyState
{
    protected Enemy_Heavy enemy;

    private float elapsed;

    private bool isBraking;
    private float brakeStartX;
    private float brakeSpeed;
    private float brakeDeceleration;

    public HeavyChargeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        elapsed = 0f;
        isBraking = false;

        // The levelled axe. Carried for the whole run and the skid, put away in Exit.
        enemy.SpawnChargeHitbox();
    }

    public override void Exit()
    {
        base.Exit();

        enemy.DespawnChargeHitbox();
        enemy.zerovelocity();
    }

    public override void Update()
    {
        base.Update();

        elapsed += Time.deltaTime;

        // No TurnToPlayer here on purpose: the charge holds the line it started on.
        bool nowhereLeftToRun = !enemy.IsGroundDetected() || enemy.IsWallDetected();

        if (isBraking)
        {
            UpdateBrake(nowhereLeftToRun);
            return;
        }

        enemy.Setvelocity(enemy.chargeSpeed * enemy.facingDir, rb.velocity.y);

        float distanceX = enemy.DistanceToPlayerX();

        bool inUpstrokeBand = distanceX <= enemy.upstrokeRangeMax && distanceX >= enemy.upstrokeRangeMin;

        // Inside the inner edge, or the player is behind him now, means the run went too far.
        // Swinging late still beats running past without swinging at all.
        bool overshot = distanceX < enemy.upstrokeRangeMin || enemy.IsPlayerBehind();

        // Every way out of the charge leads to the upstroke, never back to battle. That is what
        // makes the three states a fixed order: connecting with the player, reaching the band,
        // running out of floor or timing out all end the sprint, and he still finishes the swing.
        //
        // None of them stop him dead. They hand over to the skid, and the skid hands over to
        // the upstroke, so all that weight carries through instead of snapping to a halt.
        //
        // The band and the overshoot wait out Min Charge Time first. A charge started from
        // close range begins already inside the band, and without that wait it would end on
        // its first frame and look like a standing swing rather than a charge.
        bool sprintedLongEnough = elapsed >= enemy.minChargeTime;

        if (enemy.ChargeHitboxHasHit
            || nowhereLeftToRun
            || elapsed >= enemy.maxChargeTime
            || (sprintedLongEnough && (inUpstrokeBand || overshot)))
        {
            BeginBrake(nowhereLeftToRun);
        }
    }

    /// <summary>
    /// Works out the deceleration that brings him from his current speed to a standstill in
    /// exactly Brake Distance, so the skid always covers the distance set in the inspector
    /// however fast he happened to be going.
    /// </summary>
    private void BeginBrake(bool _nowhereLeftToRun)
    {
        // A wall or a ledge is a hard stop, and so is a brake distance of zero.
        if (_nowhereLeftToRun || enemy.brakeDistance <= 0f)
        {
            stateMachine.ChangeState(enemy.upstrokeState);
            return;
        }

        isBraking = true;
        brakeStartX = enemy.transform.position.x;
        brakeSpeed = Mathf.Abs(rb.velocity.x);

        if (brakeSpeed <= 0.01f)
        {
            stateMachine.ChangeState(enemy.upstrokeState);
            return;
        }

        // v squared over twice the distance: constant deceleration that lands on zero at x.
        brakeDeceleration = (brakeSpeed * brakeSpeed) / (2f * enemy.brakeDistance);
    }

    private void UpdateBrake(bool _nowhereLeftToRun)
    {
        brakeSpeed = Mathf.Max(0f, brakeSpeed - (brakeDeceleration * Time.deltaTime));

        enemy.Setvelocity(brakeSpeed * enemy.facingDir, rb.velocity.y);

        float skidded = Mathf.Abs(enemy.transform.position.x - brakeStartX);

        // Out of speed, out of road, or the skid has covered its distance.
        if (brakeSpeed <= 0f || _nowhereLeftToRun || skidded >= enemy.brakeDistance)
            stateMachine.ChangeState(enemy.upstrokeState);
    }
}
