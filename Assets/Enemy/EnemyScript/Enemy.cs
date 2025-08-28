using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy : Entity
{
    [SerializeField] protected LayerMask whatIsPlayer;
    [SerializeField] protected float playerCheckDistance;

    [Header("move info")]
    public float moveSpeed;
    public float idleTime;
   
    public float battleTime;
    private float defaultMoveSpeed;

    [Header("Attack info")]
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;
    [HideInInspector] public float lastTimeAttacked_Bee;
    public EnemyStateMachine stateMachine { get; private set; }
    public string lastAnimBoolName {  get; private set; }

    public RaycastHit2D hit;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();

        defaultMoveSpeed = moveSpeed;
    }

    protected override void Update()
    {
        base.Update();

         

        stateMachine.currentState.Update();

        
    }

    public virtual void AssignLastBoolName(string _animBoolName)=>lastAnimBoolName = _animBoolName;

    public override void SlowEntityBy(float _SlowPercentage, float _SlowDuration)
    {
        moveSpeed = moveSpeed * (1 - _SlowPercentage);
        anim.speed = anim.speed * (1 - _SlowPercentage);

        Invoke("ReturnDefaultSpeed", _SlowDuration);

    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        moveSpeed = defaultMoveSpeed;
    }

    public virtual void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public virtual void AttackPreparetion() => stateMachine.currentState.AttackPreparetionTrigger();

    public virtual void AnimationTrigger_Bee() => stateMachine.currentState.AnimationFinishTrigger_Bee();

    public virtual void AttackPreparetion_Bee() => stateMachine.currentState.AttackPreparetionTrigger_Bee();

    public virtual void AnimationTrigger_Medic() => stateMachine.currentState.AnimationFinishTrigger_Medic();

    public virtual void AttackPreparetion_Medic() => stateMachine.currentState.AttackPreparetionTrigger_Medic();

    //public virtual RaycastHit2D IsPlayerDetected() =>Physics2D.Raycast(wallCheck.position,Vector2.right*facingDir, playerCheckDistance, whatIsPlayer);


    public virtual bool IsPlayerDetectedAll() 
    {
        float distance = (PlayerManager.Instance.player.transform.position - transform.position).magnitude;
        float playerPosition = PlayerManager.Instance.player.transform.position.x;

        if(distance <= playerCheckDistance )
            return true;


        return false;
    
    }

    public virtual bool IsPlayerDetected()
    {
        float distance = (PlayerManager.Instance.player.transform.position - transform.position).magnitude;
        float playerPosition = PlayerManager.Instance.player.transform.position.x;


        if (facingDir == 1)
        {
            
            if (playerPosition > transform.position.x)
            {
                if (distance < playerCheckDistance && !HasLineOfSightOfWall())

                    return true;
            }
        }

        if (facingDir == -1)
        {
            if (playerPosition < transform.position.x)
            {
                if (distance < playerCheckDistance && !HasLineOfSightOfWall())

                    return true;
            }
        }
        

            return false;


    }

    private bool HasLineOfSightOfWall()

    {
        Transform playerPosition = PlayerManager.Instance.player.transform  ;
        hit = Physics2D.Raycast(transform.position, DirectionToPlayer(), playerCheckDistance, whatIsGround);
        if (hit)
        {    if (Vector2.Distance(playerPosition.position, transform.position) > (transform.position - ((Vector3)hit.point)).magnitude)
            return true;
            
        }
       
            return false;

    }

    private Vector2 DirectionToPlayer()
    {
        return (PlayerManager.Instance.player.transform.position - transform.position).normalized;

    }

 
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));


        if (IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)DirectionToPlayer() * playerCheckDistance);
        }
    }


}
