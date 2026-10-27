using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour 
{
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Effects fx { get; private set; }

    public SpecialEffect fxs { get; private set; }
    public EntityStats stats{ get; private set; }

    public CapsuleCollider2D cd{ get; private set; }

   // [Header("Knockback info")]
    [HideInInspector] protected Vector2 knockbackDirection;
    [HideInInspector] protected float knockbackDuration;
    [HideInInspector]protected bool isKnocked;
    // Set while something else is driving the velocity (e.g. weapon recoil), so states can't overwrite it.
    protected bool isRecoiling;

    [Header("Death consequences")]
    [Tooltip("What happens when this entity dies. Tick as many Consequences as needed.")]
    [SerializeField] private EventAction deathConsequences = new EventAction();

    [Header("State FX")]
    [Tooltip("Effects spawned when this entity enters a state. Several can share a state.")]
    [SerializeField] private List<StateFX> stateFX = new List<StateFX>();

    [Header("Facing")]
    [SerializeField] private bool spriteFacesLeft;
    [SerializeField] private bool startFacingLeft;
    #endregion
    [Header("Collision info")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;

    [HideInInspector]public int facingDir  = 1;
    private bool facingRight = true;

    [HideInInspector]public bool isBackingup;

    public event System.Action<Entity> OnDied;
    public bool IsDead { get; private set; }


    protected virtual void Awake()
    {
       
    }

    protected virtual void Start() 
    {   
        fx = GetComponent<Effects>();
        fxs = GetComponent<SpecialEffect>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EntityStats>();
        cd = GetComponent<CapsuleCollider2D>();

      /*  if (Mathf.Approximately(transform.eulerAngles.y, 180f))
        {
            facingDir = -1;
            facingRight = false;
            
        }*/
        if (spriteFacesLeft && anim != null && anim.transform != transform)
            anim.transform.Rotate(0f, 180f, 0f);

        if (startFacingLeft && facingRight)
            Flip();
    }

    protected virtual void Update()
    {
        UpdateStateFX();
    }

    #region State FX

    private object lastState;
    // Bumped on every state change, so a delayed effect knows whether its state has ended meanwhile.
    private int stateVisit;
    private readonly List<GameObject> untilStateEnds = new List<GameObject>();

    // Watches the state machine rather than hooking each state, so it works for any entity unchanged.
    private void UpdateStateFX()
    {
        if (stateFX.Count == 0) return;

        object current = StateLookup.CurrentState(this);
        if (current == lastState) return;

        lastState = current;
        stateVisit++;
        EndStateFX();

        if (current == null) return;

        foreach (StateFX entry in stateFX)
        {
            if (entry == null || entry.disabled || StateLookup.GetState(this, entry.state) != current) continue;

            if (entry.delay > 0f)
                StartCoroutine(SpawnStateFXLater(entry, stateVisit));
            else
                SpawnStateFX(entry);
        }
    }

    private IEnumerator SpawnStateFXLater(StateFX _entry, int _visit)
    {
        yield return new WaitForSeconds(_entry.delay);

        if (_visit == stateVisit)
            SpawnStateFX(_entry);
    }

    private void SpawnStateFX(StateFX _entry)
    {
        GameObject spawned = _entry.Spawn(transform);

        if (spawned != null && _entry.lifetime == StateFX.Lifetime.UntilStateEnds)
            untilStateEnds.Add(spawned);
    }

    private void EndStateFX()
    {
        foreach (GameObject spawned in untilStateEnds)
            if (spawned != null)
                Destroy(spawned);

        untilStateEnds.Clear();
    }

    // Until State Ends effects that don't follow the entity would otherwise outlive it.
    protected virtual void OnDestroy() => EndStateFX();

    #endregion

    public virtual void SlowEntityBy(float _SlowPercentage,float _SlowDuration)
    {


    }

    protected virtual void ReturnDefaultSpeed() 
    {
        anim.speed = 1;
    }
    public virtual void damageEffect()
    {
        if(fxs != null)
        fxs.StartCoroutine("FlashFX");
        if (fx != null)
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
        //Gizmos.DrawWireSphere(attackCheck.position,attackCheckRadius);
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
        if(isKnocked || isRecoiling)
        return;
        
        rb.velocity = new Vector2(0, 0);
    }
    




    public void Setvelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked || isRecoiling)
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
        if (IsDead) return;   // never fire twice
        IsDead = true;
        OnDied?.Invoke(this);

        deathConsequences?.Execute(this, transform);
    }
}
