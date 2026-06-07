using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_SheepSoilder : NPC
{
    public SheepSoilderIdleState idleState { get; private set; }
    public SheepSoilderWalkState walkState { get; private set; }
    public SheepSoilderWanderState wanderState { get; private set; }
    public SheepSoilderDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_SheepSoilder;

    [Header("Follow / Trace")]
    [Tooltip("Master switch for the trace system. Set false to make the sheep stop following (e.g. before it is recruited).")]
    public bool isFollowing = true;
    [Tooltip("Outer X distance. If the player gets farther than this on the X axis, the sheep starts following.")]
    public float followStartDistanceX = 3f;
    [Tooltip("Minimum gap the sheep stops at. Each follow run a random gap between min and max is chosen so it never parks at the exact same spot.")]
    public float stopDistanceXMin = 1.2f;
    [Tooltip("Maximum gap the sheep stops at. Keep this smaller than followStartDistanceX.")]
    public float stopDistanceXMax = 2.2f;
    [Tooltip("Seconds the sheep waits after the player leaves the follow range before reacting. This is the natural reaction 'lag'.")]
    public float reactionDelay = 0.25f;

    [Header("Follow Movement")]
    public float followSpeed = 6f;
    [Tooltip("How fast the sheep ramps speed up / eases speed down while moving. High = snappy, low = floaty.")]
    public float acceleration = 30f;
    [Tooltip("How fast the sheep bleeds off speed when stopping. Lower = longer, smoother glide to a halt.")]
    public float deceleration = 18f;
    [Tooltip("Distance from the target at which the sheep starts slowing down (arrival easing). Larger = earlier, gentler braking.")]
    public float arrivalDistance = 1.5f;
    [Tooltip("Lowest speed kept while easing in. Stops the sheep from stalling just short of its target. Keep it small and below followSpeed/wanderSpeed.")]
    public float minArriveSpeed = 1f;
    [Tooltip("If the player gets this far in X, the sheep speeds up so it does not get left behind forever.")]
    public float catchUpDistanceX = 8f;
    public float catchUpSpeedMultiplier = 1.6f;

    [Header("Idle Wander")]
    [Tooltip("After staying near the player this long (a random value between min and max), the sheep starts wandering freely.")]
    public float idleBeforeWanderMin = 1.5f;
    public float idleBeforeWanderMax = 3.5f;
    [Tooltip("Slow stroll speed used while wandering.")]
    public float wanderSpeed = 2f;
    [Tooltip("How far from the player (in X) the sheep is allowed to wander. Keep smaller than followStartDistanceX so it stays inside the ring.")]
    public float wanderRange = 2.5f;

    [Header("Follow Safety")]
    [Tooltip("If true, the sheep stops at ledges and walls instead of falling off / pushing into them. Requires groundCheck and wallCheck on the prefab.")]
    public bool useLedgeAndWallSafety = true;
    [Tooltip("If true, the sheep teleports near the player when hopelessly far (e.g. stuck across platforms it cannot jump to).")]
    public bool allowWarp = false;
    public float warpDistanceX = 20f;

    // Chosen per follow run so the stopping gap varies.
    [HideInInspector] public float currentStopDistance;

    private Transform player;

    protected override void Awake()
    {
        base.Awake();

        idleState = new SheepSoilderIdleState(this, stateMachine, "Idle", this);
        walkState = new SheepSoilderWalkState(this, stateMachine, "Walk", this);
        wanderState = new SheepSoilderWanderState(this, stateMachine, "Walk", this);
        deathState = new SheepSoilderDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
            player = PlayerManager.Instance.player.transform;

        currentStopDistance = stopDistanceXMin;
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_SheepSoilder -= Time.deltaTime;
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    #region Follow helpers

    public Transform Player => player;

    // Only the X axis is considered, as requested (platform jumping is ignored).
    public float XDistanceToPlayer()
        => player == null ? 0f : Mathf.Abs(player.position.x - transform.position.x);

    public int DirectionToPlayerX()
    {
        if (player == null) return 0;
        return player.position.x >= transform.position.x ? 1 : -1;
    }

    // Turn to look at the player without moving (used while idle, gives the natural Flip).
    public void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x && facingDir == -1)
            Flip();
        else if (player.position.x < transform.position.x && facingDir == 1)
            Flip();
    }

    public float CurrentFollowSpeed()
        => XDistanceToPlayer() > catchUpDistanceX ? followSpeed * catchUpSpeedMultiplier : followSpeed;

    // Pick a fresh random stopping gap so the sheep does not always halt at the same distance.
    public void PickNewStopDistance()
        => currentStopDistance = Random.Range(stopDistanceXMin, stopDistanceXMax);

    public bool IsBlockedAhead()
        => useLedgeAndWallSafety && (IsWallDetected() || !IsGroundDetected());

    // Drive horizontal velocity toward a world-space X, easing down near the target (arrival behaviour).
    public void MoveTowardsX(float targetX, float maxSpeed)
    {
        if (IsBlockedAhead())
        {
            DecelerateToStop();
            return;
        }

        float toTarget = targetX - transform.position.x;
        float dist = Mathf.Abs(toTarget);
        int dir = toTarget >= 0f ? 1 : -1;

        // Ease the speed down as we approach, but lerp toward a small MINIMUM speed
        // (not zero) so the sheep actually reaches the target instead of stalling
        // just short of it. The final glide to a full stop happens in the Idle state.
        float minS = Mathf.Min(minArriveSpeed, maxSpeed);
        float eased = arrivalDistance > 0f ? Mathf.Clamp01(dist / arrivalDistance) : 1f;
        float speed = Mathf.Lerp(minS, maxSpeed, eased);
        float targetVelX = dir * speed;

        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetVelX, acceleration * Time.deltaTime);
        Setvelocity(newVelX, rb.velocity.y); // Setvelocity auto-flips by the sign of newVelX
    }

    // Smoothly bleed horizontal speed to zero (the deceleration / glide-to-stop).
    public void DecelerateToStop()
    {
        float newVelX = Mathf.MoveTowards(rb.velocity.x, 0f, deceleration * Time.deltaTime);
        Setvelocity(newVelX, rb.velocity.y);
    }

    // Last-resort rescue when the sheep is stuck and the player is very far.
    public void TryWarpToPlayer()
    {
        if (!allowWarp || player == null) return;
        if (XDistanceToPlayer() <= warpDistanceX) return;

        Vector3 p = player.position;
        p.x -= DirectionToPlayerX() * currentStopDistance; // land just behind the player
        transform.position = p;
    }

    #endregion
}
