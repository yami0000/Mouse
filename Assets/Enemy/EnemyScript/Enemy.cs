using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public enum EnemyDeathFate
{
    DestroyTemporary,   // Destroyed this session only ¡ª respawns next scene load
    DestroyPermanently, // Destroyed forever via PersistentWorldObject
    LeaveBody           // Never destroyed ¡ª body stays in scene
}

public class Enemy : Entity
{
    [SerializeField] protected LayerMask whatIsPlayer;
    public float playerCheckDistance;

    [Header("EXP")]
    public int EXP;

    [Header("Move Info")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;
    private float defaultMoveSpeed;

    [Header("Attack Info")]
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector] public float lastTimeAttacked;
    [HideInInspector] public float lastTimeAttacked_Bee;

    [Header("Death")]
    [Tooltip("DestroyTemporary = gone this session only.\nDestroyPermanently = gone forever (requires PersistentWorldObject).\nLeaveBody = body stays in scene.")]
    [SerializeField] private EnemyDeathFate deathFate = EnemyDeathFate.DestroyTemporary;
    [Tooltip("Seconds to wait after death before destroying the body.")]
    [SerializeField] private float destroyDelay = 0.5f;

    public EnemyStateMachine stateMachine { get; private set; }
    public string lastAnimBoolName { get; private set; }

    [HideInInspector] public bool BattleState = false;

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

    public virtual void AssignLastBoolName(string _animBoolName) => lastAnimBoolName = _animBoolName;

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

    public virtual bool IsPlayerDetectedAll()
    {
        float distance = (PlayerManager.Instance.player.transform.position - transform.position).magnitude;
        return distance <= playerCheckDistance;
    }

    public virtual bool IsPlayerDetected()
    {
        float distance = (PlayerManager.Instance.player.transform.position - transform.position).magnitude;
        float playerPosition = PlayerManager.Instance.player.transform.position.x;

        if (facingDir == 1)
        {
            if (playerPosition > transform.position.x)
                if (distance < playerCheckDistance && !HasLineOfSightOfWall())
                    return true;
        }

        if (facingDir == -1)
        {
            if (playerPosition < transform.position.x)
                if (distance < playerCheckDistance && !HasLineOfSightOfWall())
                    return true;
        }

        return false;
    }

    private bool HasLineOfSightOfWall()
    {
        Transform playerPosition = PlayerManager.Instance.player.transform;
        hit = Physics2D.Raycast(transform.position, DirectionToPlayer(), playerCheckDistance, whatIsGround);
        if (hit)
        {
            if (Vector2.Distance(playerPosition.position, transform.position) > (transform.position - ((Vector3)hit.point)).magnitude)
                return true;
        }
        return false;
    }

    private Vector2 DirectionToPlayer()
    {
        return (PlayerManager.Instance.player.transform.position - transform.position).normalized;
    }

    public override void Die()
    {
        base.Die();

        PlayerManager.Instance.player.Exp += EXP;
        Debug.Log(PlayerManager.Instance.player.Exp);

        if (deathFate != EnemyDeathFate.LeaveBody)
            StartCoroutine(DeathSequence());
    }

    /// <summary>
    /// Called by death states to trigger the destroy sequence manually,
    /// for example after a death animation finishes.
    /// Only needed if you want animation-driven timing instead of destroyDelay.
    /// </summary>
    public void TriggerDeathDestroy() => StartCoroutine(DeathSequence());

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(destroyDelay);

        switch (deathFate)
        {
            case EnemyDeathFate.DestroyTemporary:
                Destroy(gameObject);
                Debug.Log($"[Enemy] '{gameObject.name}' destroyed (temporary).");
                break;

            case EnemyDeathFate.DestroyPermanently:
                PersistentWorldObject pwo = GetComponent<PersistentWorldObject>();
                if (pwo != null)
                {
                    pwo.DestroyPersistently();
                    Debug.Log($"[Enemy] '{gameObject.name}' destroyed permanently.");
                }
                else
                {
                    Debug.LogWarning($"[Enemy] '{gameObject.name}' is set to DestroyPermanently " +
                                     "but has no PersistentWorldObject component! Falling back to temporary destroy.");
                    Destroy(gameObject);
                }
                break;

            case EnemyDeathFate.LeaveBody:
                // Should never reach here ¡ª guarded above in Die()
                break;
        }
    }

    public void TurnToPlayer()
    {
        if (GetTransform().position.x > transform.position.x)
        {
            if (facingDir == -1) Flip();
        }
        else if (GetTransform().position.x < transform.position.x)
        {
            if (facingDir == 1) Flip();
        }
    }

    private static Transform GetTransform()
    {
        return PlayerManager.Instance.player.transform;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (IsPlayerDetected())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)DirectionToPlayer() * playerCheckDistance);
        }
    }
}
