using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Police : Enemy
{
    public PoliceIdleState idleState { get; private set; }
    public PoliceWalkState walkState { get; private set; }
    public PoliceBattleState battleState { get; private set; }
    public PoliceIShootState iattackState { get; private set; }
    public PoliceWShootState wattackState { get; private set; }
    public PoliceDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_Police;
    [Header("Back")]
    public float s = 14f;       // speed
    public float t = 0.35f;
    private bool isMoving = false;
    [Header("Shoot")]
    [SerializeField] private float ReadyTime;
    [SerializeField] private float xVelocity;
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private float Range;
    [SerializeField] private Transform _Transform;
    [Header("ShotGun")]
    [SerializeField] public float threshold;
    [SerializeField] private float AMin = 30f, AMax = 50f;
    [SerializeField] private float BMin = 40f, BMax = 65f;
    [SerializeField] private float prepareTimeforSummon;
    [SerializeField] private float _xVelocity;
    [SerializeField] private float _Range;
    [SerializeField] private Transform __Transform;
    [SerializeField] private GameObject motionPrefab;

    protected override void Awake()
    {
        base.Awake();

        idleState = new PoliceIdleState(this,stateMachine,"Idle",this);
        walkState = new PoliceWalkState(this, stateMachine, "Walk", this);
        battleState = new PoliceBattleState(this,stateMachine,"Walk",this);
        iattackState = new PoliceIShootState(this, stateMachine, "IShoot", this);
        wattackState = new PoliceWShootState(this, stateMachine, "WShoot", this);
        deathState = new PoliceDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Police -= Time.deltaTime;
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {

            return true;
        }

        return false;

    }
    public void BackDown() => StartCoroutine(MoveInDirection(-facingDir, s, t));
    private IEnumerator MoveInDirection(int direction, float speed, float duration)
    {
        isMoving = true;

        Vector2 startPos = transform.position;
 
        float distance = speed * duration;
        Vector2 targetPos = startPos + new Vector2(direction * distance, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

           
            float progress = elapsed / duration;

            transform.position = Vector2.Lerp(startPos, targetPos, progress);

            yield return null; // wait one frame
        }

        
        transform.position = targetPos;

        isMoving = false;
    }
    public void Shoot() => StartCoroutine(shoot());
    IEnumerator shoot()
    {
        yield return new WaitForSeconds(ReadyTime);
        TurnToPlayer();

        Vector2 dir = Vector2.right;

        GameObject Ammo = Instantiate(AmmoPrefab, _Transform.position, Quaternion.identity);

        Ammo.GetComponent<Rigidbody2D>().velocity = dir.normalized * Mathf.Abs(xVelocity)*facingDir;

        EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(this);

        Destroy(Ammo, Range);

    }
    public void ShotGun() => StartCoroutine(shotGun());

    IEnumerator shotGun()
    {
        float A = Random.Range(AMin, AMax);
        float B = Random.Range(BMin, BMax);


        yield return new WaitForSeconds(prepareTimeforSummon);

        float[] angles = { 0f, A * facingDir, B * facingDir };

        foreach (float angle in angles)
        {

            Vector2 dir = Quaternion.Euler(0, 0, angle) * new Vector2(facingDir, 0);


            GameObject Ammo = Instantiate(AmmoPrefab, __Transform.position, Quaternion.identity);


            Ammo.GetComponent<Rigidbody2D>().velocity = dir.normalized * Mathf.Abs(_xVelocity);


            EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
            if (_Ammo != null)
                _Ammo.Initialize(this);

            Destroy(Ammo, _Range);
        }
    }

    public void Motion() => StartCoroutine(DestroyAfterAnimation());
   

    private IEnumerator DestroyAfterAnimation()
    {
       
        GameObject M = Instantiate(motionPrefab, __Transform.position, Quaternion.identity);
        Animator anim = M.GetComponent<Animator>();
        float duration = anim.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log(duration);
        Destroy(M,duration);
        yield return null;
    }
}
