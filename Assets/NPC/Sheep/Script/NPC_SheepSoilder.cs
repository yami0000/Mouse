using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_SheepSoilder : NPC
{
    public SheepSoilderIdleState idleState { get; private set; }
    public SheepSoilderWalkState walkState { get; private set; }
    public SheepSoilderDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_SheepSoilder;

    [Header("Follow / Trace")]
    [Tooltip("Master switch for the trace system. Set false to make the sheep stop following (e.g. before it is recruited).")]
    public bool isFollowing = true;
    [Tooltip("Outer X distance. If the player gets farther than this on the X axis, the sheep starts following.")]
    public float followStartDistanceX = 3f;
    [Tooltip("Inner X distance. The sheep stops once it gets this close. MUST be smaller than followStartDistanceX (this gap is the 'circle' the sheep can relax in).")]
    public float stopDistanceX = 1.5f;
    [Tooltip("Seconds the sheep waits after the player leaves the follow range before reacting. This is the natural 'lag'.")]
    public float reactionDelay = 0.25f;

    [Header("Follow Movement")]
    public float followSpeed = 6f;
    [Tooltip("How fast the sheep ramps its speed up/down. High = snappy, low = floaty/heavy. This smoothing is the second source of natural lag.")]
    public float acceleration = 30f;
    [Tooltip("If the player gets this far in X, the sheep speeds up so it does not get left behind forever.")]
    public float catchUpDistanceX = 8f;
    public float catchUpSpeedMultiplier = 1.6f;

    [Header("Follow Safety")]
    [Tooltip("If true, the sheep stops at ledges and walls instead of falling off / pushing into them. Requires groundCheck and wallCheck to be assigned on the prefab.")]
    public bool useLedgeAndWallSafety = true;
    [Tooltip("If true, the sheep teleports near the player when hopelessly far (e.g. stuck across platforms it cannot jump to).")]
    public bool allowWarp = false;
    public float warpDistanceX = 20f;

    private Transform player;

    protected override void Awake()
    {
        base.Awake();

        idleState = new SheepSoilderIdleState(this, stateMachine, "Idle", this);
        walkState = new SheepSoilderWalkState(this, stateMachine, "Walk", this);
        deathState = new SheepSoilderDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
            player = PlayerManager.Instance.player.transform;

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

    // Last-resort rescue when the sheep is stuck and the player is very far (different platform, etc.).
    public void TryWarpToPlayer()
    {
        if (!allowWarp || player == null) return;
        if (XDistanceToPlayer() <= warpDistanceX) return;

        Vector3 p = player.position;
        p.x -= DirectionToPlayerX() * stopDistanceX; // land just behind the player
        transform.position = p;
    }

    #endregion
}
