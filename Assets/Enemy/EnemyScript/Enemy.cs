using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public enum EnemyDeathFate
{
    DestroyTemporary,   // Destroyed this session only �� respawns next scene load
    DestroyPermanently, // Destroyed forever via PersistentWorldObject
    LeaveBody           // Never destroyed �� body stays in scene
}

public class Enemy : Entity
{
    [SerializeField] protected LayerMask whatIsPlayer;
    public float playerCheckDistance;
    [Tooltip("Range of IsPlayerNoticed when the player is behind this enemy. Normally shorter than Player Check Distance, " +
             "which is the range the way it faces. Only matters to enemies that call IsPlayerNoticed.")]
    public float behindCheckDistance = 8f;

    [Header("Move Animation")]
    [Tooltip("Animator bool held while moving, in states that swap between moving and standing still. " +
             "Only matters to states that use EnemyState.MoveAt and StandStill.")]
    public string battleMoveAnimBool = "Walk";
    [Tooltip("Animator bool held while standing still in those same states. " +
             "Set it to the same name as Battle Move Anim Bool to switch the swapping off.")]
    public string battleIdleAnimBool = "Idle";

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

        if (IsWalkingIn)
        {
            UpdateWalkIn();
            return;
        }

        stateMachine.currentState.Update();
    }

    #region Walk in
    // Scripted entrance for CameraEvent enemy waves: the enemy walks straight to a point with its
    // own AI paused, then the state machine carries on from the state it was in.

    public bool IsWalkingIn { get; private set; }
    private float walkInTargetX;
    private float walkInSpeed;
    private float walkInTimer;
    private int walkInDir;
    private bool walkInAnimSwapped;

    /// <summary>Walks to _targetX, then hands back to the AI. _speed 0 = Move Speed. Gives up after _maxDuration seconds.</summary>
    public void WalkIn(float _targetX, float _speed, float _maxDuration)
    {
        walkInTargetX = _targetX;
        walkInSpeed = _speed > 0 ? _speed : moveSpeed;
        walkInTimer = _maxDuration;
        walkInDir = 0;
        walkInAnimSwapped = false;
        IsWalkingIn = true;
    }

    private void UpdateWalkIn()
    {
        // Done here rather than in WalkIn, which runs before Start has set up the animator and state.
        if (!walkInAnimSwapped)
        {
            SwapWalkInAnim(true);
            walkInAnimSwapped = true;
        }

        float dx = walkInTargetX - transform.position.x;
        int dir = dx >= 0 ? 1 : -1;
        if (walkInDir == 0) walkInDir = dir;

        walkInTimer -= Time.deltaTime;

        // Arrived, overshot, or stuck for too long.
        if (Mathf.Abs(dx) <= 0.1f || dir != walkInDir || walkInTimer <= 0f)
        {
            EndWalkIn();
            return;
        }

        Setvelocity(walkInDir * walkInSpeed, rb.velocity.y);
    }

    private void EndWalkIn()
    {
        StopWalkIn();
        zerovelocity();
        TurnToPlayer();
        BattleState = true;
    }

    private void StopWalkIn()
    {
        if (!IsWalkingIn) return;
        IsWalkingIn = false;

        if (walkInAnimSwapped)
            SwapWalkInAnim(false);
    }

    private void SwapWalkInAnim(bool _walking)
    {
        string stateBool = stateMachine.currentState?.AnimBoolName;

        if (_walking)
        {
            SetAnimBoolIfExists(stateBool, false);
            SetAnimBoolIfExists(battleMoveAnimBool, true);
        }
        else
        {
            SetAnimBoolIfExists(battleMoveAnimBool, false);
            SetAnimBoolIfExists(stateBool, true);
        }
    }

    // Flying enemies have no Walk bool, and a missing parameter would spam warnings.
    private void SetAnimBoolIfExists(string _name, bool _value)
    {
        if (anim == null || string.IsNullOrEmpty(_name)) return;

        foreach (var parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == _name)
            {
                anim.SetBool(_name, _value);
                return;
            }
        }
    }
    #endregion

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

    /// <summary>
    /// Notices the player in any direction, including from behind, which IsPlayerDetected
    /// cannot do because it only looks the way the enemy is facing. Behind uses its own
    /// shorter range, and walls block it either way, so nothing is spotted through ground.
    /// </summary>
    public virtual bool IsPlayerNoticed()
    {
        Transform player = PlayerManager.Instance.player.transform;
        Vector2 toPlayer = player.position - transform.position;

        float distance = toPlayer.magnitude;

        // Level with the enemy, or in front of it, counts as facing range.
        bool isBehind = (player.position.x - transform.position.x) * facingDir < 0f;

        if (distance > (isBehind ? behindCheckDistance : playerCheckDistance))
            return false;

        return Physics2D.Raycast(transform.position, toPlayer.normalized, distance, whatIsGround).collider == null;
    }

    /// <summary>Faces away from the player, so the ground and wall checks read the way the enemy is retreating.</summary>
    public void TurnAwayFromPlayer()
    {
        Transform player = PlayerManager.Instance.player.transform;

        if (player.position.x > transform.position.x)
        {
            if (facingDir == 1) Flip();
        }
        else if (player.position.x < transform.position.x)
        {
            if (facingDir == -1) Flip();
        }
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
        // Give the animator back to the state machine before the death state takes over.
        StopWalkIn();

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
                // Should never reach here �� guarded above in Die()
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

    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {

            return true;
        }

        return false;

    }
}
