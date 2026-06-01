using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepSoilderWanderState : NPCstate
{
    protected NPC_SheepSoilder sheep;

    private float wanderTargetX;

    // How close (in X) counts as "arrived" at the wander target.
    private const float arriveThreshold = 0.2f;

    public SheepSoilderWanderState(NPC _npcBase, NPCstatemachine _stateMachine, string _animBoolName, NPC_SheepSoilder sheep) : base(_npcBase, _stateMachine, _animBoolName)
    {
        this.sheep = sheep;
    }

    public override void Enter()
    {
        base.Enter();
        PickWanderTarget();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Following disabled -> just settle.
        if (!sheep.isFollowing || sheep.Player == null)
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        // Player walked off -> drop the stroll and chase immediately (no extra lag, it's already moving).
        if (sheep.XDistanceToPlayer() > sheep.followStartDistanceX)
        {
            stateMachine.ChangeState(sheep.walkState);
            return;
        }

        // Hit a ledge or wall while strolling -> give up this target and settle.
        if (sheep.IsBlockedAhead())
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        // Arrived at the chosen spot -> pause (idle will wait, then pick a new wander later).
        if (Mathf.Abs(wanderTargetX - sheep.transform.position.x) <= arriveThreshold)
        {
            stateMachine.ChangeState(sheep.idleState);
            return;
        }

        // Slow stroll toward the target, easing in near it.
        sheep.MoveTowardsX(wanderTargetX, sheep.wanderSpeed);
    }

    private void PickWanderTarget()
    {
        // Stay inside the follow ring so wandering never re-triggers the chase by accident.
        float maxOffset = Mathf.Min(sheep.wanderRange, sheep.followStartDistanceX - 0.5f);
        if (maxOffset < 0.1f) maxOffset = 0.1f;

        float px = sheep.Player.position.x;
        wanderTargetX = px + Random.Range(-maxOffset, maxOffset);
    }
}
