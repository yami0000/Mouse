using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
 




public class Player : Entity
{
    [Header("Level")]
    public int Level;
    public int Exp;
    public int ExpUntilUP;

    [Header("Attack Details")]
    public float[] attackMovement;

    public bool isBusy { get; private set; }


    public Transform attackCheck;
    public float attackCheckRadius;

    [HideInInspector]public float currentChargeTimer;

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


    [Header("Shield")]
    [SerializeField] Collider2D shieldcollider;

    [HideInInspector] public float Timer;
    [HideInInspector] public float SkillTimer;

    [HideInInspector] public float xInput;
    [HideInInspector] public float yInput;
    [HideInInspector] public bool isAutoControl = false;

    [Header("Reaper")]
    [HideInInspector]public bool UsingSkill;
    private bool HaveUsedSkill;

    [Header("Double Jump")]
    [HideInInspector] public bool CanDoubleJump;


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
    public PlayerShieldState shieldState { get; private set; }
    

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
        shieldState = new PlayerShieldState(this, StateMachine, "Shield");


    }



   
    protected override void Start()
    {
        base.Start();

        

        StateMachine.Initialize(idleState);

        defaultJumpForce =jumpforce;
        defaultMoveSpeed = movespeed;
        defaultDashSpeed = dashspeed;
        shieldcollider.enabled = false;
        HaveUsedSkill = false;
    }




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
    #endregion
    protected override void Update()
    {
        base.Update();

         

        if (!isAutoControl)
        {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");
        }

        if (Input.GetMouseButtonDown(1))
        {if (SK.Instance.Skill.EquippedSkill == "Reaper")
                if (CanUseSkill() && !HaveUsedSkill)
                {
                    Debug.Log("Reaper");
                    int FirePowerPlus = (int)(stats.FirePower.GetValue() * 0.25f);
                    HaveUsedSkill = true;
                    StartCoroutine(Reaper(FirePowerPlus)); 
                }
        }

            StateMachine.currentState.Update();

        SkillTimer -= Time.deltaTime;
        Timer -= Time.deltaTime;

        CheckForDashInput();
    }

    IEnumerator Reaper(int firepowerplus) 
    {
        UsingSkill = true;
       
        stats.FirePower.AddModifier(firepowerplus);

        yield return new WaitForSeconds(10);

        UsingSkill = false;
        HaveUsedSkill = false;
        stats.FirePower.RemoveModifier(firepowerplus);
        SkillTimer = SK.Instance.Skill.Reaper;
    }//Skill "Reaper"
    public bool CanUseSkill()
    {
        if (SkillTimer <= 0)
            return true;
        else
            return false;

    }
    public void shield() 
    {
        StartCoroutine(Shield());

     }
    IEnumerator Shield() 
    {
    yield return new WaitForSeconds(0.15f);

        
        float shieldTime = SK.Instance.Skill.OverCharging ? 0.8f : 0.5f;

        while (shieldTime >= 0)
        {
            if (SK.Instance.Skill.OverCharging)
            anim.speed = 0.625f;

            shieldTime -= Time.deltaTime;
            shieldcollider.enabled = true;
            yield return null;
        }
        shieldcollider.enabled = false;
        anim.speed = 1f;

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

    #region Level
    private void OnValidate()
    {
        if(Exp >= ExpUntilUP)
        {
            int EX = Exp - ExpUntilUP;
            Exp = EX;
            Level += 1;
            ExpUntilUP = Level * 100;
            SK.Instance.Skill.SP += 1;

        }
    }

    #endregion







}
