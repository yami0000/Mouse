using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Entity : MonoBehaviour 
{
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Effects fx { get; private set; }
    public EntityStats stats{ get; private set; }

    public CapsuleCollider2D cd{ get; private set; }

    [Header("Knockback info")]
    [SerializeField] protected Vector2 knockbackDirection;
    [SerializeField] protected float knockbackDuration;
    protected bool isKnocked;

    #endregion
    [Header("Collision info")]
    public Transform attackCheck;
    public float attackCheckRadius;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;

    public int facingDir { get; private set; } = 1;
    private bool facingRight = true;

    public bool isBackingup;

    protected virtual void Awake()
   {
    
    }

    protected virtual void Start() 
    {   
        fx = GetComponent<Effects>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EntityStats>();
        cd = GetComponent<CapsuleCollider2D>();
    }

    protected virtual void Update() 
    {

    }

    public virtual void SlowEntityBy(float _SlowPercentage,float _SlowDuration)
    {


    }

    protected virtual void ReturnDefaultSpeed() 
    {
        anim.speed = 1;
    }
    public virtual void damageEffect()
    {
        fx.StartCoroutine("FlashFX");
        StartCoroutine("HitKnockback");
         
    }

    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;

        rb.velocity = new Vector2(knockbackDirection.x * -facingDir,knockbackDirection.y);

        yield return new  WaitForSeconds(knockbackDuration);
        isKnocked = false;
    }

    #region Detect
    public bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
        Gizmos.DrawWireSphere(attackCheck.position,attackCheckRadius);
    }

    #endregion
    #region Flip and Move


    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public void zerovelocity() 
    {
        if(isKnocked) 
        return;
        
        rb.velocity = new Vector2(0, 0);
    }
    




    public void Setvelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }
    public virtual void FlipController(float _x)

    {
        if (isBackingup)
            return;

        if (_x > 0 && !facingRight)
            Flip();
        else if (_x < 0 && facingRight)
            Flip();
    }
    #endregion

    public virtual void Die() 
    {
    
    }
}
