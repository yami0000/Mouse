using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSoilderWalkState : NPCstate
{
    protected NPC_SheepSoilder sheep;

    public SheepSoilderWalkState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_SheepSoilder sheep) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.sheep = sheep;
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

        if (!sheep.isFollowing || sheep.Player == null)
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        float dist = sheep.XDistanceToPlayer();

        // Reached the inner ring -> stop following.
        // The gap between stopDistanceX and followStartDistanceX prevents start/stop jitter.
        if (dist <= sheep.stopDistanceX)
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        int dir = sheep.DirectionToPlayerX();

        // Ledge / wall safety: do not walk off platforms or push into walls.
        // Since platform-jumping is intentionally ignored, the sheep just waits at the edge.
        if (sheep.useLedgeAndWallSafety && (sheep.IsWallDetected() || !sheep.IsGroundDetected()))
        {
            sheep.Setvelocity(0f, rb.velocity.y);
            sheep.FacePlayer();
            sheep.TryWarpToPlayer(); // optional rescue if it is stuck and far away
            return;
        }

        // Smoothly ramp toward the target speed instead of snapping to it.
        // MoveTowards from the current velocity gives the follow a slight weight/lag.
        float targetVelX = dir * sheep.CurrentFollowSpeed();
        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetVelX, sheep.acceleration * Time.deltaTime);

        // Setvelocity also auto-flips the sprite based on the sign of newVelX.
        sheep.Setvelocity(newVelX, rb.velocity.y);
    }
}
