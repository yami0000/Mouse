using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy_Scavanger : Enemy
{
  public ScavangerFlyState flyState { get; private set; }
  public ScavangerBattleState battleState { get; private set; }
  public ScavangerInhaleState inhaleState { get; private set; }
  public ScavangerInhalingState inhalingState { get; private set; }
  public ScavangerReleaseState releaseState { get; private set; }
  public ScavangerShootState shootState { get; private set; }
  public ScavangerDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_Scavanger;
    [HideInInspector]public Coroutine C;
    [HideInInspector] public Coroutine S;
    
    [Header("Ring Bounds (Around Player)")]
    [HideInInspector] public Transform player;
    [HideInInspector] public bool Run;
    public float innerRadius;
    public float outerRadius;

    [Header("Step Constraints (Relative to Enemy)")]
    public float minStep;
    public float maxStep;

    [Header("Timing")]
    public float minWaitTime;
    public float maxWaitTime;
    public float moveDuration ;

    [Header("Ammo Detect")]
    [SerializeField] private float ammoCheckDistance;
    [SerializeField] private LayerMask whatIsAmmo;
    public float pullRadius = 5f;      
    public float pullStrength = 10f;
    [HideInInspector] public int energy = 0;
    [HideInInspector]public float CoolDown;
    [HideInInspector]public float AmmoCoolDown;
    public float CoolDownA;
    public float CoolDownT;
    public int MaxEnergy;
    [Header("Shoot")]
    public float prepareTimeforSummon;
    public float Range;
    public float xVelocity;
    public GameObject AmmoPrefab;
    private int FP;

    protected override void Awake()
    {
        base.Awake();

        flyState = new ScavangerFlyState(this, stateMachine, "Fly", this);
        battleState = new ScavangerBattleState(this, stateMachine, "Fly", this);
        inhaleState = new ScavangerInhaleState(this, stateMachine, "Inhale", this);
        inhalingState = new ScavangerInhalingState(this, stateMachine, "Inhaling", this);
        releaseState = new ScavangerReleaseState(this, stateMachine, "Release", this);
        shootState = new ScavangerShootState(this, stateMachine, "Shoot", this);
        deathState = new ScavangerDeathState(this, stateMachine, "Fly", this);

    }

    protected override void Start()
    {
        base.Start();
        player = PlayerManager.Instance.player.transform; 
        stateMachine.Initialize(flyState);
        FP= stats.FirePower.GetValue();
        Run = false;
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Scavanger -= Time.deltaTime;
        CoolDown -= Time.deltaTime;
        AmmoCoolDown -= Time.deltaTime;
    }
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    #region Move
    public void Move()
    {
        C = StartCoroutine(MovementRoutine());
    }
    public void StopMove()
    {
        if (C != null)
        {
            StopCoroutine(C);
        }

    }
    IEnumerator MovementRoutine()
    {
        while (true)
        {
            
            float waitTimer = Random.Range(minWaitTime, maxWaitTime);

            
            while (waitTimer > 0)
            {
                
                if (Vector2.Distance(transform.position, player.position) > outerRadius)
                {
                    break;  
                }

                waitTimer -= Time.deltaTime;
                yield return null; 
            }

             
            Vector2 nextPos;
            float currentDistance = Vector2.Distance(transform.position, player.position);

            if (currentDistance > outerRadius && playerCheckDistance >currentDistance)
            {
                // Logic provided by you: Return to the ring immediately
                float midRingDistance = (innerRadius + outerRadius) / 2f;
                Vector2 directionFromPlayer = ((Vector2)transform.position - (Vector2)player.position).normalized;
                nextPos = (Vector2)player.position + (directionFromPlayer * midRingDistance);

                if (nextPos.y <= player.position.y+2f)
                    nextPos.y = player.position.y + 3f;
            }
            else if(playerCheckDistance <= currentDistance) 
            {
                Run=true;
                break;
            }
            else
            {
                // Normal random movement within the ring
                nextPos = GetValidRandomPointAbove();
            }

            // --- EXECUTE MOVEMENT ---
            yield return StartCoroutine(MoveToPosition(nextPos));
        }
    }

    IEnumerator MoveToPosition(Vector2 target)
    {
        // 1. Flip to face movement
        FlipController(target.x - transform.position.x);

        float elapsedTime = 0;
        Vector2 startPos = transform.position;
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(startPos, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        HeadToPlayer();
    }

    public void HeadToPlayer()
    {
        FlipController(player.position.x - transform.position.x);
    }

    Vector2 GetValidRandomPointAbove()
    {
        Vector2 candidate = transform.position;
        for (int i = 0; i < 20; i++)
        {
            float randomAngle = Random.Range(0, Mathf.PI * 2);
            float randomDist = Random.Range(minStep, maxStep);
            Vector2 offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDist;
            Vector2 potentialPoint = (Vector2)transform.position + offset;

            if (Vector2.Distance(potentialPoint, player.position) >= innerRadius &&
                Vector2.Distance(potentialPoint, player.position) <= outerRadius &&
                potentialPoint.y > player.position.y+2f)
            {
                return potentialPoint;
            }
        }
        return candidate;
    }

    #endregion
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
    public void SuckAmmo()
    {
       
        Collider2D[] caughtObjects = Physics2D.OverlapCircleAll(transform.position, pullRadius, whatIsAmmo);

        foreach (Collider2D rbCollider in caughtObjects)
        {
          
            Rigidbody2D rb = rbCollider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                
                Vector2 direction = (Vector2)transform.position - rb.position;

                 
                rb.velocity = direction.normalized * pullStrength;

                 
                if (direction.magnitude < 0.5f)
                {
                    Destroy(rbCollider.gameObject);
                    energy++;
                    energy = Mathf.Clamp(energy, 0, MaxEnergy);
                }
            }
        }
    }

    #region Shoot
    public void Shoot() 
    {
        stats.FirePower.BaseValue = FP * (energy / 2 + 1);
        S = StartCoroutine(ShootAmmo());
    
    }
 
    IEnumerator ShootAmmo()
    {
        

            yield return new WaitForSeconds(prepareTimeforSummon);


            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {

                Vector2 targetPosition = player.transform.position;
                Vector2 currentPosition = transform.position;
                Vector2 shootDirection = (targetPosition - currentPosition).normalized;


                GameObject Ammo = Instantiate(AmmoPrefab, transform.position, Quaternion.identity);



                float Velocity = xVelocity * (energy/2+1);

                Rigidbody2D rb = Ammo.GetComponent<Rigidbody2D>();
                if (rb != null)
                {

                    rb.velocity = shootDirection * Mathf.Abs(Velocity);
                }


                EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
            if (_Ammo != null)
            {
                if(energy>=3 && energy<5)
                _Ammo.sp.color = Color.blue;
                if(energy>=5)
                _Ammo.sp.color = Color.white;

                _Ammo.Initialize(this);
            }
                Destroy(Ammo, Range);



           


        }
    }
    #endregion
}
