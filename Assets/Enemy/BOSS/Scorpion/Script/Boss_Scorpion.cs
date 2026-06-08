 
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
 

public class Boss_Scorpion : Enemy

{
    
    private Transform player;
    [SerializeField] private float ammoCheckDistance;

    [SerializeField] private LayerMask whatIsAmmo;

    [HideInInspector] public float stateTimer_Scorpion;

    [HideInInspector] public bool isWaiting;

    [SerializeField] DetectDamage_Scor damagedetect;

    [HideInInspector] public int canBlock;
    public ScorpionIdleState idleState { get; private set; }
    public ScorpionWalkState walkState { get; private set; }
    public ScorpionDashState dashState { get; private set; }
    public ScorpionBlockState blockState { get; private set; }
    public ScorpionMeleeState meleeState { get; private set; }
    public ScorpionSwingState swingState { get; private set; }
    public ScorpionLaserState laserState { get; private set; }
    public ScorpionShotState shotState { get; private set; }
    public ScorpionCombo1State combo1State { get; private set; }    
    public ScorpionDeathState deathState { get; private set; }

    [Header("Data for Swing")]
    [SerializeField] private float preparetime;
    [SerializeField] private float firstswing;
    [SerializeField] private float secondswing;
    public float swingdistance;
    [Header("Damage calculation")]
    public float Energy;
    [SerializeField] private float FirePowerMultiplyer;
    [SerializeField] private float FallOffSpeed;
    private int OriFirePower;
    [Header("Laser")]
    public LaserAttack laser;
    [Header("ShotGun")]
    [SerializeField] private float prepareTimeforSummon;
    [SerializeField] private float xVelocity;
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float Range;
    [SerializeField] private Transform _Transform;
    [Header("Combo1")]
    [SerializeField] private float prepareTimeBeforeDash;
    [SerializeField] private float DashSpeed;
    [SerializeField] private float dashFrontOffset;
    [SerializeField] private float dashTime;
    [Header("Sprint")]
    public float distanceToDash;
    public float sprintTime;
    [Header("Melee")]
    public float prepareTimeBeforeMelee;
    public float attackTime;
    [Header("Walk Or Idle?")]
    [HideInInspector] public int StateNum = 0;
    [HideInInspector] public float StateTime1;
    [HideInInspector] public float StateTime2;
    [HideInInspector] public float StateTime3;
    [HideInInspector] public bool isMoving;
    [Header("Motion")]
    [SerializeField] public GameObject motionPrefab;
    [SerializeField] private Transform __Transform;
    [Header("Dialougue After Rigel is Dead")]
    [SerializeField] private string DialogueName;
    [SerializeField] private float t;
    [SerializeField] private string ID;

    protected override void Awake()
    {
        base.Awake();
        idleState = new ScorpionIdleState(this, stateMachine, "Idle", this);
        walkState = new ScorpionWalkState(this, stateMachine, "Walk", this);
        dashState = new ScorpionDashState(this, stateMachine, "Idle", this);
        blockState = new ScorpionBlockState(this, stateMachine, "Block", this);
        meleeState = new ScorpionMeleeState(this, stateMachine, "Melee", this);
        swingState = new ScorpionSwingState(this, stateMachine, "Swing", this);
        laserState = new ScorpionLaserState(this, stateMachine, "Laser", this);
        shotState = new ScorpionShotState(this, stateMachine, "Shot", this);
        combo1State = new ScorpionCombo1State(this, stateMachine, "Combo1", this);
        deathState = new ScorpionDeathState(this, stateMachine, "Idle", this);
    }
    protected override void Start()
    {
        base.Start();
        isWaiting = true;
        isMoving = true;
        canBlock = 0;
        GM.Instance.GameManager.ScorpionMaxHealth =  stats.MaxHP.GetValue();  
        stateMachine.Initialize(idleState);


        PlayerManager.Instance.player.OnScriptedDefeat += HandlePlayerDefeated;

        OriFirePower = stats.FirePower.BaseValue;

        player = PlayerManager.Instance.player.transform;

        Energy = 0;

    }
    protected override void Update()
    {
        base.Update();

        GM.Instance.GameManager.ScorpionHealth = stats.CurrentHP;

        stateTimer_Scorpion -= Time.deltaTime;

        Energy -= Time.deltaTime * FallOffSpeed;
        Energy = Mathf.Clamp(Energy, 0f, 100f);

        fx.sr.material.SetFloat("_BlendAmount", 1 - Energy / 100);

         
        stats.FirePower.BaseValue= OriFirePower + (int)(Energy * FirePowerMultiplyer);

        
        
    }
    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }

    private void HandlePlayerDefeated()
    {
        
        stateMachine.ChangeState(idleState);
        BattleState = false;
        NarrativeManager.Instance.RequestDialogue(DialogueName, t ,() => ExecuteActions());
    }

    private void ExecuteActions()
    {
        PlayerManager.Instance.player.Revive();
        GameManager.Instance.LoadScene(5, ID);
    }
    
    public bool DetectAmmo()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ammoCheckDistance, whatIsAmmo);

        if (hits.Length == 0)
        {

            return false;
        }
        else
        {
            return true;

        }
    }
    public void wait()
    {
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {

        

        yield return new WaitForSeconds(0.3f);

        

         

    }

    public void Swing()
    {
        StartCoroutine(swing());
    }
    IEnumerator swing() 
    {
        //isWaiting=false;
        yield return new WaitForSeconds(preparetime);
        damagedetect.EnableSwingHitbox1();
        yield return new WaitForSeconds(firstswing);
        damagedetect.DisableSwingHitbox1();
        damagedetect.EnableSwingHitbox2();
        yield return new WaitForSeconds(secondswing);
        damagedetect.DisableSwingHitbox2();
        //isWaiting = true;
    
    }

    public void Summon()
    {
        StartCoroutine(SummonProjectiles());
        

    }

     
    IEnumerator SummonProjectiles()
    {
        float A = Random.Range(30f, 50f);
        float B = Random.Range(40f, 65f);


        yield return new WaitForSeconds(prepareTimeforSummon);

        float[] angles = { 0f, A*facingDir, B*facingDir};  

        foreach (float angle in angles)
        {
            
            Vector2 dir = Quaternion.Euler(0, 0, angle) * new Vector2(facingDir, 0);

             
            GameObject Ammo = Instantiate(AmmoPrefab, _Transform.position, Quaternion.identity);

           
            Ammo.GetComponent<Rigidbody2D>().velocity = dir.normalized * Mathf.Abs(xVelocity);

            
            Collide_SelfAimAmmoEnemy _Ammo = Ammo.GetComponent<Collide_SelfAimAmmoEnemy>();
            if (_Ammo != null)
                _Ammo.Initialize(enemy);

            Destroy(Ammo,Range);
        }
    }

    public void Combo1() => StartCoroutine(combo1());

    IEnumerator combo1()
    {
        yield return new WaitForSeconds(prepareTimeBeforeDash);

        TurnToPlayer();

        Vector2 startPos = transform.position;
        Vector2 playerPos = player.transform.position;

        
        Vector2 targetPos = new Vector2 (playerPos.x,startPos.y) + new Vector2(-facingDir * dashFrontOffset, 0f);

        float dashDuration = dashTime;  
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;

            
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, easedT);
            transform.position = newPos;


            damagedetect.EnableMeleeHitbox();

            yield return null;
        }

     
        zerovelocity();
        damagedetect.DisableMeleeHitbox();
    }
    public void sprint() => StartCoroutine(Sprint());
    IEnumerator Sprint() 
    {
        //yield return new WaitForSeconds(prepareTimeBeforeSprint);

        TurnToPlayer();

        Vector2 startPos = transform.position;
        Vector2 playerPos = player.transform.position;


        Vector2 targetPos = new Vector2(playerPos.x, startPos.y) + new Vector2(-facingDir * dashFrontOffset, 0f);

        float dashDuration = sprintTime;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;


            float easedT = Mathf.SmoothStep(0f, 1f, t);


            Vector2 newPos = Vector2.Lerp(startPos, targetPos, easedT);
            transform.position = newPos;


           

            yield return null;
        }


        zerovelocity();
       



    }

    public void melee() => StartCoroutine(Melee());
    IEnumerator Melee()
    {
        TurnToPlayer();
        yield return new WaitForSeconds(prepareTimeBeforeMelee);

        float attackDuration = attackTime;
        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            damagedetect.EnableMeleeHitbox();
            yield return null;
        }
        damagedetect.DisableMeleeHitbox();
    }
    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown && !isMoving)
        {
            return true;
        }

        return false;

    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ammoCheckDistance);
        Gizmos.DrawWireSphere(transform.position, swingdistance);
    }

    public void Motion() => StartCoroutine(DestroyAfterAnimation());


    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(prepareTimeforSummon - 0.4f);
        GameObject M = Instantiate(motionPrefab, __Transform.position, Quaternion.identity);
        Animator anim = M.GetComponent<Animator>();
        float duration = anim.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log(duration);
        Destroy(M, duration);
    }


}
