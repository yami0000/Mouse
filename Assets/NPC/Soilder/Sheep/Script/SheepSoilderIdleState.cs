using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSoilderIdleState : NPCstate
{
    protected NPC_SheepSoilder sheep;

    // Counts how long the player has been outside the follow range (reaction lag).
    private float reactionTimer;

    // Counts idle time before the sheep starts wandering. Randomised each entry.
    private float idleTimer;
    private float wanderDelay;

    public SheepSoilderIdleState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_SheepSoilder sheep) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.sheep = sheep;
    }

    public override void Enter()
    {
        base.Enter();

        reactionTimer = 0f;
        idleTimer = 0f;
        wanderDelay = Random.Range(sheep.idleBeforeWanderMin, sheep.idleBeforeWanderMax);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Smoothly glide to a stop rather than snapping to zero.
        sheep.DecelerateToStop();

        // Only turn to face the player once basically stopped, to avoid flipping mid-glide.
        if (Mathf.Abs(rb.velocity.x) < 0.1f)
            sheep.FacePlayer();

        if (!sheep.isFollowing || sheep.Player == null)
            return;

        // Player is outside the follow ring.
        if (sheep.XDistanceToPlayer() > sheep.followStartDistanceX)
        {
            reactionTimer += Time.deltaTime;

            // If something is in the way (ledge/wall), keep WAITING in idle instead of
            // bouncing into the walk state. Optionally warp if hopelessly far.
            if (sheep.IsBlockedAhead())
            {
                sheep.TryWarpToPlayer();
                return;
            }

            // Path is clear and the reaction lag has elapsed -> start following.
            if (reactionTimer >= sheep.reactionDelay)
                stateMachine.ChangeState(sheep.walkState);

            return;
        }

        // Player is comfortably inside the ring.
        reactionTimer = 0f;

        // After a short, randomised pause, start wandering freely to look alive
        // (only if the path isn't blocked).
        idleTimer += Time.deltaTime;
        if (idleTimer >= wanderDelay && !sheep.IsBlockedAhead())
            stateMachine.ChangeState(sheep.wanderState);
    }
}
