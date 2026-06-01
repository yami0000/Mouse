using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSoilderIdleState : NPCstate
{
    protected NPC_SheepSoilder sheep;

    // Counts how long the player has been outside the follow range.
    // This produces the natural "lag" before the sheep reacts.
    private float reactionTimer;

    public SheepSoilderIdleState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_SheepSoilder sheep) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.sheep = sheep;
    }

    public override void Enter()
    {
        base.Enter();
        reactionTimer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Stand still horizontally but keep gravity, then turn to look at the player.
        // This is the "relax inside the circle and Flip to look natural" behaviour.
        sheep.Setvelocity(0f, rb.velocity.y);
        sheep.FacePlayer();

        if (!sheep.isFollowing || sheep.Player == null)
            return;

        // Player left the outer ring -> start counting the reaction delay.
        if (sheep.XDistanceToPlayer() > sheep.followStartDistanceX)
        {
            reactionTimer += Time.deltaTime;

            if (reactionTimer >= sheep.reactionDelay)
                stateMachine.ChangeState(sheep.walkState);
        }
        else
        {
            // Player came back inside the ring before the sheep reacted -> reset the lag.
            reactionTimer = 0f;
        }
    }
}
