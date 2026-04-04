using System.Collections;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Boss_Sword : Enemy
{

    private Transform player;
    public Boss_SwordIdleState idleState{ get; private set; }
    public Boss_SwordBattleState battleState{ get; private set; }
    public Boss_SwordChopState chopState { get; private set; }  
    public Boss_SwordShootState shootState { get; private set; }
    public Boss_SwordDeathState deathState { get; private set; }
    public Boss_SwordHideState hideState { get; private set; }
    public Boss_SwordThrowState throwState { get; private set; }
    public Boss_SwordRetrieveState retrieveState{ get; private set; }
    public Boss_SwordDoubleShootState doubleShootState { get; private set; }


    [Header("Invisible")]
    public float transitionDuration;
    public float moveDuration;
    [SerializeField] float xOffset;
    [Header("Chop")]
    [SerializeField] float t1;
    [SerializeField] float t2;
    [Header("Shoot")]
    [SerializeField] float xOffsetS;
    [SerializeField] private float prepareTimeforSummon;
    [SerializeField] private float xVelocity;
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float Range;
    [SerializeField] private Transform _Transform;
    [Header("Throw")]
    [SerializeField] float t;
    [SerializeField] public GameObject _Blade;
    [HideInInspector] public Blade bladeS;
    [SerializeField] GameObject bladePrefab;
    [SerializeField] float XOffset;
    [SerializeField] float YOffset;
    [SerializeField] float S;
    [Header("Retrieve")]
    public float beforeRetrieve;
    public float r = 0.5f;
    public float _r = 0.3f; 
    public float shakeAmount = 0.1f;
    public float xOffsetR = 0.5f;
    public float yOffsetR = 0.2f;
    [Header("DoubleShoot")]
    [SerializeField] private Transform _Transform1;
    [SerializeField] private float interval;

    [HideInInspector] public float StateTimer_Sword;
    [HideInInspector] public int idleType;

    [SerializeField] private float X;
    protected override void Awake()
    {
        base.Awake();

        idleState = new Boss_SwordIdleState(this, stateMachine, "Idle", this);
        battleState = new Boss_SwordBattleState(this, stateMachine, "Idle", this);
        chopState = new Boss_SwordChopState(this, stateMachine, "Chop", this);
        shootState = new Boss_SwordShootState(this, stateMachine, "Shoot", this);
        deathState = new Boss_SwordDeathState(this, stateMachine, "Idle", this);
        hideState = new Boss_SwordHideState(this, stateMachine, "Idle", this);
        throwState = new Boss_SwordThrowState(this, stateMachine, "Throw", this);
        retrieveState = new Boss_SwordRetrieveState(this, stateMachine, "Retrieve", this);
        doubleShootState = new Boss_SwordDoubleShootState(this, stateMachine, "DoubleShoot", this);



    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

        idleType = 0;
        player = PlayerManager.Instance.player.transform;

    }

    protected override void Update()
    {
        base.Update();
        SetAniSpeed();
        StateTimer_Sword -= Time.deltaTime*X;
    }
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    private void SetAniSpeed() 
    {
        anim.speed = 1 * X;
    }
    public void HideAndShow() 
    {
        StartCoroutine(hideAndShow());
        
    }
    IEnumerator hideAndShow() 
    {
        float elapsed = 0f;

         
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            
            fx.sr.material.SetFloat("_Dissolve", Mathf.Lerp(0, 1, t));
            yield return null;  
        }
        cd.enabled = false;
        elapsed = 0f;
        
        int i = Random.Range(0, 2);

        Vector3 startPos = transform.position;
        
        if (i == 1)
           xOffset = -xOffset;

        Vector3 targetPos = new Vector3(player.position.x + xOffset, transform.position.y, transform.position.z);
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
             
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        elapsed = 0f;
        TurnToPlayer();
        zerovelocity();
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            fx.sr.material.SetFloat("_Dissolve", Mathf.Lerp(1, 0, t));
            yield return null;
        }
        cd.enabled = true;
        fx.sr.material.SetFloat("_Dissolve", 0);
    }
    public void HideAndShoot() 
    {
        StartCoroutine(hideAndShoot());
    }
    IEnumerator hideAndShoot() 
    {
        
        float elapsed = 0f;


        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            fx.sr.material.SetFloat("_Dissolve", Mathf.Lerp(0, 1, t));
            yield return null;
        }
        cd.enabled = false;
        elapsed = 0f;

        int i = Random.Range(0, 2);

        Vector3 startPos = transform.position;

        if (i == 1)
            xOffsetS = -xOffsetS;

        Vector3 targetPos = new Vector3(player.position.x + xOffsetS, transform.position.y, transform.position.z);
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        elapsed = 0f;
        TurnToPlayer();
        zerovelocity();
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            fx.sr.material.SetFloat("_Dissolve", Mathf.Lerp(1, 0, t));
            yield return null;
        }
        cd.enabled = true;
        fx.sr.material.SetFloat("_Dissolve", 0);

    }
    public void Chop() => StartCoroutine(chop());
    IEnumerator chop() 
    {
        Hitboxs hitbox = this.GetComponentInChildren<Hitboxs>();
        Debug.Log(hitbox);
        yield return new WaitForSeconds(t1);
        hitbox.EnableHitbox(0);
        yield return new WaitForSeconds(t2);
        hitbox.DisableHitbox(0);
    
    }
    public void Shoot() => StartCoroutine(shoot());

    IEnumerator shoot() 
    {
        yield return new WaitForSeconds(prepareTimeforSummon);
        TurnToPlayer();

        Vector2 dir = (player.position - transform.position).normalized;


        GameObject Ammo = Instantiate(AmmoPrefab, _Transform.position, Quaternion.identity);


        Ammo.GetComponent<Rigidbody2D>().velocity = dir.normalized * Mathf.Abs(xVelocity);


        EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(enemy);

        Destroy(Ammo, Range);

    }

    public void Throw() => StartCoroutine(_Throw());
    IEnumerator _Throw()
    {
        yield return new WaitForSeconds(0.416f);


        Vector3 spawnPos = new Vector3(
        transform.position.x + (XOffset * facingDir),
        transform.position.y + YOffset,
        0
    );

        Quaternion spawnRotation = (facingDir == -1)
    ? Quaternion.Euler(0, 180f, 0) * bladePrefab.transform.rotation
    : bladePrefab.transform.rotation;

        
        GameObject blade = Instantiate(bladePrefab, spawnPos, spawnRotation);

        Vector3 dashDirection = (player.position - blade.transform.position).normalized;

        Vector2 dir2 = (player.position - blade.transform.position).normalized;
        float angle = Mathf.Atan2(dir2.y, dir2.x) * Mathf.Rad2Deg;
       


        
        float yRot = (facingDir == 1) ? 0f : 180f;
        Quaternion targetRot = Quaternion.Euler(0, yRot, facingDir == 1? angle:180-angle);
        Quaternion startRot = blade.transform.rotation;

        bladeS = blade.GetComponent<Blade>();
        bladeS.direction = (player.position - spawnPos).normalized;
        bladeS.speed = S;
        bladeS.Initialize(this);

        _Blade = blade;
        

        float elapsed = 0;
        while (blade != null)
        {
            elapsed += Time.deltaTime;

            
            

           
            if (elapsed <= t)
            {
                blade.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / t);
            }
            else
            {
               
                blade.transform.rotation = targetRot;
            }

            yield return null;
        }
    }
    public void Retrieve() => StartCoroutine(retrieve());
    IEnumerator retrieve() 
    {
        if (_Blade == null) yield break;

        yield return new WaitForSeconds(beforeRetrieve);
         
        Vector3 originalPos = _Blade.transform.position;
        float elapsed = 0;

        while (elapsed < r)
        {
            elapsed += Time.deltaTime;
            
            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0;  
            _Blade.transform.position = originalPos + randomOffset;
            yield return null;
        }

        
        elapsed = 0;
        Vector3 startDashPos = _Blade.transform.position;

        while (elapsed < _r)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _r;

           
            Vector3 targetPos = new Vector3(
                transform.position.x + (xOffsetR * facingDir),
                transform.position.y + yOffsetR,
                transform.position.z
            );

            
            _Blade.transform.position = Vector3.Lerp(startDashPos, targetPos, t);

            
            yield return null;
        }
        Destroy( _Blade );

    }
    public void DoubleShoot()
    {
        StartCoroutine(doubleShoot(0.833f/X,_Transform));
        StartCoroutine(doubleShoot(1.0833f/X,_Transform1));
    }
    IEnumerator doubleShoot(float f,Transform _transform) 
    {
     
        yield return new WaitForSeconds(f);
        TurnToPlayer();
        GenerateAmmo(_transform);
        yield return new WaitForSeconds(interval);
        TurnToPlayer();
        GenerateAmmo(_transform);
    }
    

    private void GenerateAmmo(Transform _transform)
    {
        Vector2 dir = (player.position - transform.position).normalized;


        GameObject Ammo = Instantiate(AmmoPrefab, _transform.position, Quaternion.identity);


        Ammo.GetComponent<Rigidbody2D>().velocity = dir.normalized * Mathf.Abs(xVelocity);


        EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(enemy);

        Destroy(Ammo, Range);
    }

    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown )
        {
            return true;
        }

        return false;

    }

    
}
