using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [HideInInspector] public float currentChargeTimer;

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

    [Header("jump info")]
    public float jumpTime;
    [HideInInspector] public float jumpTimeCounter;

    [Header("Shield")]
    [SerializeField] Collider2D shieldcollider;

    public float Timer;

    [HideInInspector] public float xInput;
    [HideInInspector] public float yInput;
    [HideInInspector] public bool isAutoControl = false;

    [Header("Reaper")]
    [HideInInspector] public bool UsingSkill;

    private HellEngine hellEngine;

    [Header("Double Jump")]
    [HideInInspector] public bool CanDoubleJump;

    [Header("Partical Effect")]
    public GameObject Charging;
    [HideInInspector] public GameObject _ChargingAni;
    [Header("Dust")]
    public GameObject dustPrefab;
    public float runDustInterval = 0.15f;
    public float dashDustInterval = 0.05f;
    public float wallSlideDustInterval = 0.1f;
    [Tooltip("X = player speed, Y = dust density multiplier (2 = twice as much dust, 0 = no dust)")]
    public AnimationCurve dustDensityBySpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(12, 1), new Keyframe(25, 2));
    private float dustTimer;
    [Header("Character UI")]
    public GameObject E;//E互动ui
    [Header("Throwable")]
    public GameObject Grenade;
    public GameObject impactDot;
    public LayerMask collisionLayer;
    public LineRenderer lineRenderer;
    public float throwPower = 18f;
    public int numPoints = 30;
    public float timeBetweenPoints = 0.1f;

    [Header("REVIVE UI")]
    [SerializeField] public GameObject ReviveUI;
    [HideInInspector] public bool inScriptedDefeat=false;   // true for the whole intro boss fight (player is meant to lose)
    public event System.Action OnScriptedDefeat;       // fires when the player dies while inScriptedDefeat is true

    public void NotifyScriptedDefeat() => OnScriptedDefeat?.Invoke();

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

        defaultJumpForce = jumpforce;
        defaultMoveSpeed = movespeed;
        defaultDashSpeed = dashspeed;
        shieldcollider.enabled = false;
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


    public void Revive()
    {
        stats.Revive();                                   
        GM.Instance.GameManager.isUIOpened = false;       
        StateMachine.ChangeState(idleState);              
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

        HandleActiveSkills();

        StateMachine.currentState.Update();

        Timer -= Time.deltaTime;

        CheckForDashInput();

        if (xInput == 0 && StateMachine.currentState != wallSlideState)
            GetMouseP();
    }

    private void GetMouseP()
    {

        if (PlayerManager.Instance.player != null)
        {

            Vector3 mouseScreenPos = Input.mousePosition;

            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            if (mouseWorldPos.x > transform.position.x && facingDir == -1)
                Flip();
            if (mouseWorldPos.x < transform.position.x && facingDir == 1)
                Flip();

        }
    }
    #region Skill System
    // Runs with the player, so like everything else here it stops while a menu is open:
    // cooldowns wait, and picking a new trigger key can't fire the skill.
    private void HandleActiveSkills()
    {
        Skill skill = Skill.Instance;
        if (skill == null) return;

        skill.TickCooldowns(Time.deltaTime);

        for (int i = 0; i < skill.ActiveSkills.Count; i++)
        {
            ActiveSkillEntry entry = skill.ActiveSkills[i];

            if (entry.key != KeyCode.None && Input.GetKeyDown(entry.key) && entry.IsReady)
                UseActiveSkill(entry);
        }
    }

    // What each active skill does. Its cooldown starts when it ends, not here.
    private void UseActiveSkill(ActiveSkillEntry _entry)
    {
        switch (_entry.id)
        {
            case ActiveSkillId.ProtectiveShield:
                if (StateMachine.currentState is PlayerGroundedState)
                    StateMachine.ChangeState(shieldState);
                break;

            case ActiveSkillId.Reaper:
                if (!UsingSkill)
                    StartCoroutine(Reaper((int)(stats.Damage.GetValue() * 0.25f)));
                break;

            case ActiveSkillId.HellEngine:
                hellEngine ??= new HellEngine((PlayerStats)stats);

                if (!hellEngine.IsRunning)
                    hellEngine.Trigger();
                break;
        }
    }

    IEnumerator Reaper(int damageplus)
    {
        UsingSkill = true;

        stats.Damage.AddModifier(damageplus);

        yield return new WaitForSeconds(10);

        UsingSkill = false;
        stats.Damage.RemoveModifier(damageplus);
        Skill.Instance.StartCooldown(ActiveSkillId.Reaper);
    }//Skill "Reaper"
    public void shield()
    {
        StartCoroutine(Shield());

    }
    IEnumerator Shield()
    {
        yield return new WaitForSeconds(0.15f);


        float shieldTime = Skill.Instance.OverCharging ? 0.8f : 0.5f;

        while (shieldTime >= 0)
        {
            if (Skill.Instance.OverCharging)
                anim.speed = 0.625f;

            shieldTime -= Time.deltaTime;
            shieldcollider.enabled = true;
            yield return null;
        }
        shieldcollider.enabled = false;
        anim.speed = 1f;

    }
    #endregion
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
        if (Exp >= ExpUntilUP)
        {
            int EX = Exp - ExpUntilUP;
            Exp = EX;
            Level += 1;
            ExpUntilUP = Level * 100;
            if (Skill.Instance != null) // OnValidate also runs outside play mode
                Skill.Instance.SP += 1;

        }
    }

    #endregion
    public void ChargingAni()
    {
        if (_ChargingAni != null) return;
        _ChargingAni = Instantiate(Charging, transform);
    }
    public void DestroyChargingAni() => Destroy(_ChargingAni);

    #region Recoil
    private Coroutine recoilRoutine;

    /// <summary>
    /// Pushes the player back against _shotDirection as the weapon's recoil describes: speeding up,
    /// then slowing to a stop. Movement input waits until it is over, and the player never turns around.
    /// </summary>
    public void ApplyRecoil(Vector2 _shotDirection, WeaponRecoil _recoil)
    {
        if (_recoil == null || !_recoil.Enabled)
            return;

        Vector2 pushDirection = -_shotDirection;
        if (_recoil.horizontalOnly)
            pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.0001f)
            return;

        // A shot fired mid-recoil carries on at full push instead of building up from a standstill again,
        // so automatic weapons shove steadily rather than stutter.
        float startTime = isRecoiling ? _recoil.accelerationTime : 0f;

        if (recoilRoutine != null)
            StopCoroutine(recoilRoutine);

        recoilRoutine = StartCoroutine(Recoil(pushDirection.normalized, _recoil, startTime));
    }

    private IEnumerator Recoil(Vector2 _direction, WeaponRecoil _recoil, float _time)
    {
        isRecoiling = true;
        float lastVerticalPush = 0f;

        while (_time < _recoil.Duration)
        {
            yield return new WaitForFixedUpdate();
            _time += Time.fixedDeltaTime;

            Vector2 push = _direction * _recoil.SpeedAt(_time);

            // Sideways the push is the whole velocity; vertically it rides on top of gravity and jumps.
            rb.velocity = new Vector2(push.x, rb.velocity.y - lastVerticalPush + push.y);
            lastVerticalPush = push.y;
        }

        isRecoiling = false;
        recoilRoutine = null;
    }

    // A disabled object stops its coroutines, which would otherwise leave movement locked.
    private void OnDisable()
    {
        isRecoiling = false;
        recoilRoutine = null;
    }
    #endregion

    public void CreateDust(float _interval)
    {
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        SpawnDust(groundHit, _interval);
    }

    public void CreateWallDust(float _interval)
    {
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        SpawnDust(wallHit, _interval);
    }

    private void SpawnDust(RaycastHit2D _hit, float _interval)
    {
        dustTimer -= Time.deltaTime;

        if (dustTimer > 0 || dustPrefab == null || _hit.collider == null)
            return;

        float density = dustDensityBySpeed.Evaluate(rb.velocity.magnitude);
        if (density <= 0)
            return;

        dustTimer = _interval / density;//faster = shorter wait = more dust

        //the prefab is made for flat ground (dust goes up), so turn its "up" to the surface normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);
        GameObject dust = Instantiate(dustPrefab, _hit.point, rotation);

        ParticleSystem ps = dust.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1f;
        Destroy(dust, lifetime);
    }

    public void DrawProjection(Vector2 direction)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = numPoints;

        Vector3 startPosition = transform.position;
        Vector3 startVelocity = (Vector3)direction * throwPower;

         
        float gravityScale = Grenade.GetComponent<Rigidbody2D>().gravityScale;
        Vector3 customGravity = (Vector3)(Physics2D.gravity * gravityScale);

        Vector3 previousPosition = startPosition;

        for (int i = 0; i < numPoints; i++)
        {
            float t = i * timeBetweenPoints;
            Vector3 currentPosition = startPosition + startVelocity * t + 0.5f * customGravity * t * t;

            RaycastHit2D hit = Physics2D.Linecast(previousPosition, currentPosition, collisionLayer);

            if (hit.collider != null)
            {
               
                lineRenderer.positionCount = i + 1;
                lineRenderer.SetPosition(i, hit.point);
 
                impactDot.SetActive(true);
                impactDot.transform.position = hit.point;
 
                return;
            }
 
            lineRenderer.SetPosition(i, currentPosition);
            previousPosition = currentPosition;
        }
 
        impactDot.SetActive(false);
    }

    public void FireInAHole(Vector2 direction,ItemData item) 
    {
        impactDot.SetActive(false);
        if (item == null) return;
     GameObject G = Instantiate(Grenade, transform.position,Quaternion.identity);
     EntityStats stats = G.GetComponent<EntityStats>();
     Rigidbody2D GrenadeRb = G.GetComponent<Rigidbody2D>();
      
     GrenadeRb.GetComponent<SpriteRenderer>().sprite = item.icon;
     GrenadeRb.velocity = new Vector3(direction.x, direction.y, 0) * throwPower;
     GrenadeRb.angularVelocity = -720f;

     ItemData_Equipment _item = item as ItemData_Equipment;
        stats.Damage.BaseValue = _item.Damage;
        stats.FireDamage.BaseValue = _item.FireDamage;
        stats.FrostDamage.BaseValue = _item.FrostDamage;
        stats.PoisonDamage.BaseValue = _item.FrostDamage;
        stats.LightningDamage.BaseValue = _item.LightningDamage;



    }

  
}
