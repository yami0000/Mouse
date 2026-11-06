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

    // ---- Special actions (NPC-exclusive animations driven by an Animator bool) ----

    private readonly Dictionary<string, NPCSpecialState> specialStates = new Dictionary<string, NPCSpecialState>();

    public bool IsInSpecial => stateMachine.currentState is NPCSpecialState;

    /// <summary>True if this NPC's Animator has a bool parameter with this exact name.</summary>
    public bool HasAnimBool(string boolName)
    {
        if (anim == null || string.IsNullOrEmpty(boolName)) return false;
        foreach (AnimatorControllerParameter p in anim.parameters)
            if (p.type == AnimatorControllerParameterType.Bool && p.name == boolName)
                return true;
        return false;
    }

    /// <summary>
    /// Switch to the special state for this Animator bool (created on first use).
    /// If already in that exact state, the animation restarts from the beginning.
    /// </summary>
    public NPCSpecialState EnterSpecial(string animBoolName, bool moveWhilePlaying = false)
    {
        if (!specialStates.TryGetValue(animBoolName, out NPCSpecialState state))
        {
            state = new NPCSpecialState(this, stateMachine, animBoolName, this);
            specialStates[animBoolName] = state;
        }

        state.moveWhilePlaying = moveWhilePlaying;

        if (stateMachine.currentState != state)
            stateMachine.ChangeState(state);
        else
            state.Restart();

        return state;
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
