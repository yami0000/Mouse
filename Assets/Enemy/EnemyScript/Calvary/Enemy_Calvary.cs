
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Enemy_Calvary : Enemy
{
    public CalvaryFlyState flyState { get; private set; }
    public CalvaryBattleState battleState { get; private set; }
    public CalvaryChargeState chargeState { get; private set; }
    public CalvaryRecoverState recoverState { get; private set; }
    public CalvaryDeathState deathState { get; private set; }

    [HideInInspector] public float stateTimer_Calvary;
    [HideInInspector] public float InitializeY;
    [HideInInspector]public Transform Ro;

    [SerializeField] Hitbox hitbox;
    [Header("Dodging")]
    public float radius;
    public float moveSpeedD;
    public float waitTimeBefore;
    public float waitTime;
    [HideInInspector]public bool ReadyToCharge;
    [Header("Rotation")]
    public float timeBeforeRotation;
    public float rotationDuration ;
    public float rotationSpeed;
    private Transform player;
    private int moveDir;
    [Header("charging")]
    [SerializeField]private float dashDuration;
    [SerializeField] private float accelDuration;
    [SerializeField] private float maxSpeed;
    [Header("stop")]
    [SerializeField] private float stopDuration;
    [HideInInspector] public bool Stopped;
    [Header("RePos")]
    public float ReposDuration;
    public bool Over;
    protected override void Awake()
    {
        base.Awake();

        flyState = new CalvaryFlyState(this, stateMachine, "Fly", this);
        battleState = new CalvaryBattleState(this, stateMachine, "Fly", this);
        chargeState = new CalvaryChargeState(this, stateMachine, "Charge", this);
        recoverState = new CalvaryRecoverState(this, stateMachine, "Recover", this);
        deathState = new CalvaryDeathState(this, stateMachine, "Death", this);
    }
    protected override void Start()
    {
        base.Start();
        player = PlayerManager.Instance.player.transform;
        stateMachine.Initialize(flyState);
        InitializeY = transform.position.y;

        hitbox.DisableMeleeHitbox();
        ReadyToCharge = false;
        Stopped = false;
        Over = false;
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Calvary -= Time.deltaTime;
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    public void Dodge()
    {
        StartCoroutine(PatrolSequence());

    }
    #region Dodging
    IEnumerator PatrolSequence()
    {
        Vector2 center = transform.position;


        Vector2 point1 = center + (Random.insideUnitCircle * radius);
        Vector2 point2 = center + (Random.insideUnitCircle * radius);

        yield return new WaitForSeconds(waitTimeBefore);


        yield return StartCoroutine(MoveToPoint(point1));

        zerovelocity();
        yield return new WaitForSeconds(waitTime);


        yield return StartCoroutine(MoveToPoint(point2));

        ReadyToCharge = true;


    }

    IEnumerator MoveToPoint(Vector2 target)
    {

        while (Vector2.Distance(transform.position, target) > 0.1f)
        {

            transform.position = Vector2.MoveTowards(
                transform.position,
                target,
                moveSpeedD * Time.deltaTime
            );


            yield return null;
        }


        transform.position = target;
    }
    #endregion

    public void ChangeRotation()
    {
        StartCoroutine(RotateTowardsPlayer());
    }

    IEnumerator RotateTowardsPlayer()
    {
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation;

        yield return new WaitForSeconds(timeBeforeRotation);

        while (timeElapsed < rotationDuration)
        {
            Vector2 direction = (Vector2)player.position - (Vector2)transform.position;


            float worldAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;


            if (player.position.x > transform.position.x)
                moveDir = 1;
            else
                moveDir = -1;


            float targetZ = (moveDir == 1) ? worldAngle : 180f - worldAngle;


            float currentZ = transform.eulerAngles.z;
            float newZ = Mathf.MoveTowardsAngle(currentZ, targetZ, rotationSpeed * Time.deltaTime);

            float yRotation = (moveDir == 1) ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0f, yRotation, newZ);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
       
        yield return StartCoroutine(DashAtPlayer());
        
    }
    IEnumerator DashAtPlayer()
    {
        hitbox.EnableMeleeHitbox();
        Vector2 dashDirection = (player.position - transform.position).normalized;
        float startX = transform.position.x;
        float playerX = player.position.x;
        bool playerWasToRight = playerX > startX;

        float elapsed = 0f;
        
        float currentSpeed = 0f;

        while (elapsed < dashDuration)
        {
         
            if (elapsed < accelDuration)
            {
              
                currentSpeed = Mathf.Lerp(0, maxSpeed, elapsed / accelDuration);
            }
            else
            {
               
                currentSpeed = maxSpeed;
            }

             
            transform.position += (Vector3)dashDirection * currentSpeed * Time.deltaTime;

           
            float currentX = transform.position.x;
            bool hasCrossed = playerWasToRight ? (currentX >= playerX) : (currentX <= playerX);

            if (hasCrossed)
            {
                break;  
            }

            elapsed += Time.deltaTime;
            
            yield return null;
        }

      
        yield return StartCoroutine(SlowToStop(dashDirection, currentSpeed));
    }

    IEnumerator SlowToStop(Vector2 direction, float speedAtImpact)
    {
        float elapsed = 0f;
        

        while (elapsed < stopDuration)
        {
            
            float speed = Mathf.Lerp(speedAtImpact, 0, elapsed / stopDuration);

            transform.position += (Vector3)direction * speed * Time.deltaTime;

            elapsed += Time.deltaTime;
            yield return null;
        }
        hitbox.DisableMeleeHitbox();
        Stopped = true;
    }
    public void ReturnToNormal() => StartCoroutine(FinalReposition());

    IEnumerator FinalReposition()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        
        float randomX = Random.Range(-2f, 2f);
        Vector3 targetPos = new Vector3(startPos.x + randomX, InitializeY, startPos.z);

        while (elapsed < ReposDuration)
        {
            float t = elapsed / ReposDuration;

             
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            elapsed += Time.deltaTime;
           
            yield return null;
        }

        transform.position = targetPos;
        Over = true;
    }

    public void Reset()
    {
        transform.rotation = (facingDir == 1)? Quaternion.identity : Quaternion.Euler(0, 180, 0);
        Flip();

    }
}

