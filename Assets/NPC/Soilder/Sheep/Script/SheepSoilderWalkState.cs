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
        else 
      

        // Ledge / wall ahead -> hand off to Idle so it WAITS in the idle pose (not the walk animation).
        // Idle will resume the chase automatically once the path is clear.
        if (sheep.IsBlockedAhead())
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        // Aim for a point currentStopDistance short of the player; MoveTowardsX eases in as it arrives.
        int dir = sheep.DirectionToPlayerX();
        float targetX = sheep.Player.position.x - dir * sheep.currentStopDistance;
        sheep.MoveTowardsX(targetX, sheep.CurrentFollowSpeed());
    }
}
