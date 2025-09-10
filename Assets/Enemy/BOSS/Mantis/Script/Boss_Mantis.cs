using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Mantis : Enemy
{
    #region Initialize

    
    private Transform player;
    private int moveDir;
    public MantisPeaceState peaceState { get; private set; }
    public MantisPrepareState prepareState { get; private set; }
    public MantisStaticState staticState { get; private set; }
    public MantisMoveFState moveFState { get; private set; }
    public MantisMoveBState moveBState { get; private set; }
    public MantisDoubleSlashState doubleSlashState { get; private set; }
    public MantisDashSlashState dashSlashState { get; private set; }
    public MantisSummonStateState summonState { get; private set; }
    public MantisJumpAttackState jumpAttackState { get; private set; }
    public MantisDeathState deathState { get; private set; }

    public float stateTimer_Mantis;
    [SerializeField] DamageDetect damageDetect;

    [Header("Control Distance")]
    [SerializeField] private float Melee;
    [SerializeField] private float Long;

    [Header("Data for Double Slash")]
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dash1Time; // A seconds
    [SerializeField] private float dash2Time; // B seconds
    [SerializeField] private AnimationCurve dashCurve; // speed curve

    [Header("Data for Jump Slash")]
    [SerializeField] private float prepareTime;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float hangTime;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float slamDuration;
    [SerializeField] private AnimationCurve slamCurve;

    [Header("Data for Dash Slash")]
    [SerializeField] private float jumpAheadDistance;
    [SerializeField] private float prepareTimeforDash;
    [SerializeField] private float jumpDurationforDash;
    [HideInInspector] private Vector2 target;

    [Header("Data for Summon")]
    [SerializeField] private float prepareTimeforSummon;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float Range;
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private float xVelocity;

    #endregion


    protected override void Awake()
    {
        base.Awake();

        peaceState = new MantisPeaceState(this, stateMachine, "Peace", this);
        prepareState = new MantisPrepareState(this, stateMachine, "Prepare", this);
        staticState = new MantisStaticState(this, stateMachine, "Static", this);
        moveFState = new MantisMoveFState(this, stateMachine, "MoveF", this);
        moveBState = new MantisMoveBState(this, stateMachine, "MoveB", this);
        doubleSlashState = new MantisDoubleSlashState(this, stateMachine, "DoubleSlash", this);
        dashSlashState = new MantisDashSlashState(this, stateMachine, "DashSlash", this);
        summonState = new MantisSummonStateState(this, stateMachine, "Summon", this);
        jumpAttackState = new MantisJumpAttackState(this, stateMachine, "JumpAttack", this);
        deathState = new MantisDeathState(this, stateMachine, "Death", this);


    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(peaceState);
        player = PlayerManager.Instance.player.transform;
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Mantis -= Time.deltaTime;

        if (GM.Instance.GameManager.isMantisBossFightStarted)
        {
            GM.Instance.GameManager.MantisHealth = this.stats.CurrentHP;

            GM.Instance.GameManager.MantisMaxHealth = stats.GetMaxHealth();
        }
    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }

    public void DecideMove()
    {
        Transform player = PlayerManager.Instance.player.transform;
        float D = Vector2.Distance(transform.position, player.position);
        float M = Random.Range(0, 100);

        if (D < Melee)
        {
            if (M < 80)
                stateMachine.ChangeState(doubleSlashState);
            else if (M >= 80 && M <= 90)
                stateMachine.ChangeState(jumpAttackState);
            else
                stateMachine.ChangeState(summonState);
        }
        if ( D >= Melee)
        {
             
            if ( M < 35)
                stateMachine.ChangeState(jumpAttackState);
            else if (M >= 35 && M <= 65)
                stateMachine.ChangeState(summonState);
            else
                stateMachine.ChangeState(dashSlashState);

        }
 



    }//根据位置决定行动
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Melee);

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, Long);
    }//距离可视化

    public void TurnToPlayer()
    {
        if (player.position.x > transform.position.x)
            moveDir = 1;
        else if (player.position.x < transform.position.x)
            moveDir = -1;

        if (IsGroundDetected())
            Setvelocity(0.0000001f * moveDir, rb.velocity.y);
        else
            Setvelocity(0.0000001f * moveDir, rb.velocity.y);
    }

    #region DoubleSlash
    public void StartDash()
    {
        StartCoroutine(DashSequence());
    }


    private IEnumerator DashSequence()
    {
        if (moveDir == 1)
        {
            yield return new WaitForSeconds(dash1Time);
            damageDetect.EnableDashHitbox();
            yield return StartCoroutine(Dash(Vector2.right));
            damageDetect.DisableDashHitbox();
        }
        else
        {
            yield return new WaitForSeconds(dash1Time);
            damageDetect.EnableDashHitbox();
            yield return StartCoroutine(Dash(Vector2.left));
            damageDetect.DisableDashHitbox();
        }

        // Recalculate direction after first dash
        TurnToPlayer();

        // dash again in the new direction
        yield return new WaitForSeconds(dash2Time - dash1Time);
        damageDetect.EnableDashHitbox();
        yield return StartCoroutine(Dash(moveDir == 1 ? Vector2.right : Vector2.left));
        damageDetect.DisableDashHitbox();


    }

    private IEnumerator Dash(Vector2 direction)
    {


        Vector2 start = rb.position;
        Vector2 end = start + direction.normalized * dashDistance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);

            // natural speed profile
            float curvedT = dashCurve.Evaluate(t);

            // move smoothly
            Vector2 newPos = Vector2.Lerp(start, end, curvedT);
            rb.MovePosition(newPos);

            yield return null;
        }

        rb.MovePosition(end);
    }
    #endregion

    #region JumpAttack
    public void JumpAttack()
    {
        StartCoroutine(JumpAndSlam());
    }
    private IEnumerator JumpAndSlam()
    {

        float originalGravity = rb.gravityScale;

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(prepareTime);

        rb.gravityScale = 0f; // ignore physics while animating jump arc

        Vector2 startPos = rb.position;
        float highestY = startPos.y + jumpHeight; // set in Inspector
        float elapsed = 0f;

        // Step 1: Go up to peak using curve
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float curvedT = jumpCurve.Evaluate(t); // curve 0→1

            float newY = Mathf.Lerp(startPos.y, highestY, curvedT);
            rb.MovePosition(new Vector2(startPos.x, newY));

            yield return null;
        }

        yield return new WaitForSeconds(hangTime);

        Vector2 start = rb.position;
        Vector2 target = PlayerManager.Instance.player.transform.position;
        elapsed = 0f;

        TurnToPlayer();
        damageDetect.EnableSlamHitbox();
        while (elapsed < slamDuration)
        {
            
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slamDuration);
            float curvedT = slamCurve.Evaluate(t);

            Vector2 newPos = Vector2.Lerp(start, target, curvedT);
            rb.MovePosition(newPos);

            yield return null;

            
           
        }
        damageDetect.DisableSlamHitbox();
        rb.gravityScale = originalGravity;
    }

    #endregion

    #region DashAttack
    public void DashAttack() 
    {
        StartCoroutine(ParabolaJump());
    }
    private IEnumerator ParabolaJump()
    {

        Vector2 start = rb.position;
        Vector2 playerPos = PlayerManager.Instance.player.transform.position;

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(prepareTimeforDash);

        TurnToPlayer();

         
        target = playerPos - new Vector2(moveDir * jumpAheadDistance, 0f);
        

        float elapsed = 0f;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        damageDetect.EnableDashHitbox();
        while (elapsed < jumpDurationforDash)
        {
            
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDurationforDash);

            // linear ground movement
            float x = Mathf.Lerp(start.x, target.x, t);

            // parabola y = (4h * t * (1-t)) + lerp(start.y, target.y, t)
            float baseY = Mathf.Lerp(start.y, target.y, t);
            float arc = 4f * jumpHeight * t * (1 - t);
            float y = baseY + arc;

            rb.MovePosition(new Vector2(x, y));

            yield return null;
        }
        rb.gravityScale = originalGravity;
        damageDetect.DisableDashHitbox();    
    }
    #endregion


    public void Summon() 
    {
        StartCoroutine(SummonProjectiles());
    
    }
    IEnumerator SummonProjectiles()
    {


        yield return new WaitForSeconds(prepareTimeforSummon);


        GameObject Ammo = Instantiate(AmmoPrefab, transform.position, Quaternion.identity);

        Ammo.GetComponent<Rigidbody2D>().velocity = new Vector2(xVelocity * moveDir, 0);

        Collide_SelfAimAmmoEnemy _Ammo = Ammo.GetComponent<Collide_SelfAimAmmoEnemy>();
        if (_Ammo != null)
            _Ammo.Initialize(enemy);

        Destroy(Ammo, Range);


    }


}

