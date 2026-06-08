using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_All : NPC
{
    public NPCIdleState idleState { get; private set; }
    public NPCWalkState walkState { get; private set; }

    [Header("Movement")]
    [SerializeField] private float defaultMoveSpeed = 2f;

    // Kept public-get so NPCWalkState keeps working unchanged.
    public float moveSpeed { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new NPCIdleState(this, stateMachine, "Idle", this);
        walkState = new NPCWalkState(this, stateMachine, "Walk", this);

        moveSpeed = defaultMoveSpeed;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    // ---- Helpers used by the universal controller / actions ----
    // These only switch FSM state + set parameters. The states themselves
    // still own velocity and animation, so nothing fights over rb.velocity.

    public bool IsIdle => stateMachine.currentState == idleState;
    public bool IsWalking => stateMachine.currentState == walkState;

    public void EnterIdle()
    {
        if (stateMachine.currentState != idleState)
            stateMachine.ChangeState(idleState);
    }

    public void EnterWalk()
    {
        if (stateMachine.currentState != walkState)
            stateMachine.ChangeState(walkState);
    }

    public void SetMoveSpeed(float speed) => moveSpeed = speed;

    /// <summary>Face the given horizontal direction (-1 left, +1 right).</summary>
    public void SetFacing(int dir)
    {
        if (dir != 0 && dir != facingDir)
            Flip(); // Flip() keeps facingDir + facingRight + transform in sync.
    }

    /// <summary>Face toward a world X position.</summary>
    public void FaceTowardsX(float worldX)
    {
        float dx = worldX - transform.position.x;
        if (Mathf.Abs(dx) > 0.001f)
            SetFacing(dx > 0f ? 1 : -1);
    }
}
