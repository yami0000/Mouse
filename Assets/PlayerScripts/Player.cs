using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;




public class Player : Entity
{

    [Header("Attack Details")]
    public float[] attackMovement;

    public bool isBusy { get; private set; }


    public Transform attackCheck;
    public float attackCheckRadius;

    [Header("move info")]
    public float movespeed = 12f;
    public float jumpforce;
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    [Header("Dash info")]
    [SerializeField] private float dashcooldown;
    private float dashUsageTimer;
    public float dashspeed;
    public float dashduration;
    private float defaultDashSpeed;    
    public float dashDir { get; private set; }

    public float Timer;




    #region States
    public PlayerStateMachine StateMachine { get; private set; }



    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }

    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }

    public PlayerWallSlideState wallSlideState { get; private set; }

    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDashState dashState { get; private set; }

    public PlayerAttackState attackState { get; private set; }

    public PlayerDeathState deathState { get; private set; }

    #endregion

    protected override void Awake()
    {   
        base.Awake();
        
        StateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, StateMachine, "Idle");
        moveState = new PlayerMoveState(this, StateMachine, "Move");
        jumpState = new PlayerJumpState(this, StateMachine, "Jump");
        airState = new PlayerAirState(this, StateMachine, "Jump");
        dashState = new PlayerDashState(this, StateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, StateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, StateMachine, "WallJump");
        attackState = new PlayerAttackState(this, StateMachine, "Attack");
        deathState = new PlayerDeathState(this, StateMachine, "Death");

       

    }



   
    protected override void Start()
    {
        base.Start();

        

        StateMachine.Initialize(idleState);

        defaultJumpForce =jumpforce;
        defaultMoveSpeed = movespeed;
        defaultDashSpeed = dashspeed;
        
    }



    protected override void Update()
    {
        base.Update();
        StateMachine.currentState.Update();

        Timer -= Time.deltaTime;

        CheckForDashInput();
    }

    public override void SlowEntityBy(float _SlowPercentage, float _SlowDuration)
    {
        movespeed = movespeed * (1 - _SlowPercentage);  
        jumpforce = jumpforce * (1 - _SlowPercentage);
        dashspeed = dashspeed * (1 - _SlowPercentage);  
        anim.speed = anim.speed * (1 - _SlowPercentage);

        Invoke("ReturnDefaultSpeed", _SlowDuration);
    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        movespeed = defaultMoveSpeed;
        jumpforce = defaultJumpForce;
        dashspeed = defaultDashSpeed;
    }
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }

    public void AnimationTrigger() => StateMachine.currentState.AnimationFinishTrigger();

    public void Keyframe() => StateMachine.currentState.KeyframeTrigger();







    private void CheckForDashInput()
    {
        if (IsWallDetected())
            return;


        dashUsageTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashUsageTimer < 0)
        {
            dashUsageTimer = dashcooldown;
            dashDir = Input.GetAxisRaw("Horizontal");

            if (dashDir == 0)
                dashDir = facingDir;

            StateMachine.ChangeState(dashState);
        }
    }

    public override void Die()
    {
        base.Die();

        StateMachine.ChangeState(deathState);
    }


    protected new virtual void OnDrawGizmos()
    {

        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }


}
