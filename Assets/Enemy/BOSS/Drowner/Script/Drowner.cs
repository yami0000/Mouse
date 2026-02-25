using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Drowner : Enemy
{
    public DrownerBeforeState beforeState { get; private set; }
    public DrownerStaticState idleState { get; private set; }
    public DrownerUpState UpState { get; private set; }
    public DrownerAttackState attackState { get; private set; }
    public DrownerDownState downState { get; private set; }
    public DrownerSubmergeState submergeState { get; private set; }
    public DrownerDeathState deathState { get; private set; }

    [HideInInspector] public Transform player;
    [HideInInspector] public int P;
    [HideInInspector] public float StateTimer_Drowner;

    public int moveDir;
    [Header("Ping Pong")]
    public float height = 0.3f;   
    public float speed = 1f;
    [Header("Dive")]
    public float riseHeight = 2f;         
    public float riseDuration = 0.4f;       
    public float diveTargetY = -10f;       
    public float diveDuration = 0.8f;
    private Vector3 startPos;
    [Header("Positions")]
    public float rePosDuration = 1.5f;
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;
    private int Pos;
    private int Atk;
    [Header("UnDive")]
    public float emergeHeight = 10f;        
    public float emergeDuration = 0.8f;    
    public float fallDistance = 1f;     
    public float fallDuration = 0.4f;
    [Header("Fire!")]
    [SerializeField] private float prepareTimeforSummon;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float Range;
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private float xVelocity;
    [SerializeField] private int shotCount;
    [SerializeField] private float interval;
    [SerializeField] private float Wave;
    [Header("Fall!")]
    [HideInInspector] public int Num;
    [HideInInspector] public bool isFalling;
    public FallingPlatform F;
    public FallingPlatform S;
    public FallingPlatform T;

    protected override void Awake()
    {
        base.Awake();


        beforeState =new DrownerBeforeState(this, stateMachine, "Idle", this);
        idleState = new DrownerStaticState(this, stateMachine, "Idle", this);
        UpState = new DrownerUpState(this, stateMachine, "Idle", this);
        attackState = new DrownerAttackState(this, stateMachine, "Attack", this);
        downState = new DrownerDownState(this, stateMachine, "Idle", this);
        submergeState = new DrownerSubmergeState(this, stateMachine, "Idle", this);
        deathState = new DrownerDeathState(this, stateMachine, "Idle", this);

    }

    protected override void Start()
    {
        base.Start();
        Flip();
        player = PlayerManager.Instance.player.transform;
        stateMachine.Initialize(beforeState);
    }

    protected override void Update()
    {
        base.Update();
       
        StateTimer_Drowner -= Time.deltaTime;
    }
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    public void RD()
    {
        startPos = transform.position;
        StartCoroutine(RiseAndDive());
    }

    IEnumerator RiseAndDive()
    {
        // ---------- RISE ----------
        Vector3 riseStart = startPos;
        Vector3 riseEnd = startPos + new Vector3(0, riseHeight, 0);

        float time = 0f;
        while (time < riseDuration)
        {
            float t = time / riseDuration;
            t = Mathf.SmoothStep(0, 1, t); // smooth easing
            transform.position = Vector3.Lerp(riseStart, riseEnd, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = riseEnd;

        // ---------- DIVE ----------
        Vector3 diveStart = riseEnd;
        Vector3 diveEnd = new Vector3(transform.position.x, diveTargetY, transform.position.z);

        time = 0f;
        while (time < diveDuration)
        {
            float t = time / diveDuration;
            t = Mathf.SmoothStep(0, 1, t); // smooth easing
            transform.position = Vector3.Lerp(diveStart, diveEnd, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = diveEnd;
    }

    public void AdjustPosition() 
    {
        Pos = Random.Range(0, 100);
        Atk = Random.Range(0, 20);

        

        Transform newtransform =
      Pos < 25 ? A:
      Pos < 50 ? B:
      Pos < 75 ? C:
                 D;

        startPos = transform.position;
        StartCoroutine(rePos(newtransform));


    }

      IEnumerator rePos(Transform newtransform) 
    {
        Vector3 emergeStart = startPos;
        Vector3 emergeEnd = new Vector3(newtransform.position.x, transform.position.y, transform.position.z);
        float time = 0f;
        while (time < rePosDuration) 
        {
            float t = time / emergeDuration;

            // Strong upward push, slows near top
            t = 1 - Mathf.Pow(1 - t, 3); // ease-out cubic

            transform.position = Vector3.Lerp(emergeStart, emergeEnd, t);

            time += Time.deltaTime;
            yield return null;
        }
        transform.position = emergeEnd;
    }
    public void UF() 
    {
        startPos = transform.position;
        StartCoroutine(UnDiveAndFall());
    }
    IEnumerator UnDiveAndFall()
    {
        // -------- EMERGE --------
        Vector3 emergeStart = startPos;
        Vector3 emergeEnd = new Vector3(transform.position.x, emergeHeight, transform.position.z);

        float time = 0f;
        while (time < emergeDuration)
        {
            float t = time / emergeDuration;

            // Strong upward push, slows near top
            t = 1 - Mathf.Pow(1 - t, 3); // ease-out cubic

            transform.position = Vector3.Lerp(emergeStart, emergeEnd, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = emergeEnd;

        // -------- SMALL FALL --------
        Vector3 fallStart = emergeEnd;
        Vector3 fallEnd = emergeEnd - new Vector3(0, fallDistance, 0);

        time = 0f;
        while (time < fallDuration)
        {
            float t = time / fallDuration;

            // Gravity-like acceleration
            t = t * t; // ease-in

            transform.position = Vector3.Lerp(fallStart, fallEnd, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = fallEnd;
    }

    public void TurnToPlayer()
    {


        if (player.position.x > transform.position.x)
            moveDir = 1;
        else if (player.position.x < transform.position.x)
            moveDir = -1;

      
            Setvelocity(0.0000001f * moveDir, rb.velocity.y);
    }

    public void Summon()
    {
        StartCoroutine(SummonProjectiles());

    }
    IEnumerator SummonProjectiles()
    {


        yield return new WaitForSeconds(prepareTimeforSummon);
        TurnToPlayer();

        for (int i = 0; i < shotCount; i++)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y += 0.8f;

            GameObject ammo = Instantiate(AmmoPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = ammo.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = new Vector2(xVelocity * moveDir, 0);

            EnemyAmmo_Effect effect = ammo.GetComponent<EnemyAmmo_Effect>();
            if (effect != null)
                effect.Initialize(enemy);

            Destroy(ammo, Range);

            // 最后一发不需要再等
            if (i < shotCount - 1)
            {   float inter = Random.Range(interval-Wave,interval+Wave);
                yield return new WaitForSeconds(inter);
                TurnToPlayer();
            }
        }


    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerCheckDistance);
         
    }
}
