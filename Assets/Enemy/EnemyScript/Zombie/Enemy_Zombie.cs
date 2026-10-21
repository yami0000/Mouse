using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Zombie : Enemy
{
    public ZombieIdleState idleState { get; private set; }
    public ZombieWalkState walkState { get; private set; }
    public ZombieBattleState battleState { get; private set; }
    public ZombieAttackState attackState { get; private set; }
    public ZombieDeathState deathState { get; private set; }

    [Header("Battle Movement")]
    [Tooltip("Inside this distance the zombie stops moving and stops turning to face the player.")]
    public float battleStopDistance = 5f;

    [Header("Attack Movement")]
    [Tooltip("Time base of the speed curve, and the minimum length of the attack state. The state also waits until the zombie has landed.")]
    public float attackDuration = 1f;
    [Tooltip("Horizontal speed the curve is multiplied by. When Aim Through Player is on, this is the speed cap.")]
    public float attackMoveSpeed = 5f;
    [Tooltip("Speed multiplier over the attack. X = normalized time (0..1), Y = multiplier. " +
             "Negative Y moves backwards, 0 stands still.")]
    public AnimationCurve attackMoveCurve = AnimationCurve.Constant(0f, 1f, 0f);

    [Header("Attack Jump")]
    [Tooltip("Upward velocity of the leap. Gravity (the Rigidbody's Gravity Scale) does the rest, " +
             "so the zombie flies past the player and lands further behind. 0 = no jump.")]
    public float attackJumpForce = 8f;
    [Tooltip("Scale the speed curve so the arc passes through the player's position on the way down. " +
             "The landing spot behind the player then follows from the jump force and the speed.")]
    public bool aimThroughPlayer = true;

    [HideInInspector] public float stateTimer_Zombie;

    protected override void Awake()
    {
        base.Awake();
        idleState = new ZombieIdleState(this, stateMachine, "Idle", this);
        walkState = new ZombieWalkState(this, stateMachine, "Walk", this);
        battleState = new ZombieBattleState(this, stateMachine, "Walk", this);
        attackState = new ZombieAttackState(this, stateMachine, "Attack", this);
        deathState = new ZombieDeathState(this, stateMachine, "Idle", this);
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }


    protected override void Update()
    {
        base.Update();
        stateTimer_Zombie -= Time.deltaTime;
    }
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }
}
