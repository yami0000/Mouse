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

        // New random stopping gap for this follow run, so it doesn't always park at the same distance.
        sheep.PickNewStopDistance();
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

        // Reached the (randomised) stopping gap -> hand off to idle, which eases the rest of the speed off.
        if (dist <= sheep.currentStopDistance)
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        // Ledge / wall safety: wait at the edge instead of falling off or grinding into a wall.
        if (sheep.IsBlockedAhead())
        {
            sheep.DecelerateToStop();
            sheep.FacePlayer();
            sheep.TryWarpToPlayer(); // optional rescue if stuck and far away
            return;
        }

        // Aim for a point currentStopDistance short of the player; MoveTowardsX eases in as it arrives.
        int dir = sheep.DirectionToPlayerX();
        float targetX = sheep.Player.position.x - dir * sheep.currentStopDistance;
        sheep.MoveTowardsX(targetX, sheep.CurrentFollowSpeed());
    }
}
