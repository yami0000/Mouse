using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Mushroom : Enemy
{
    public MushroomIdleState idleState { get; private set; }
    public MushroomWalkState walkState { get; private set; }
    public MushroomBattleState battleState { get; private set; }
    public MushroomAttackState attackState { get; private set; }
    public MushroomDeathState deathState { get; private set; }

    [Header("Battle Movement")]
    [Tooltip("Inside this distance the mushroom stops closing in on the player.")]
    public float battleStopDistance = 6f;
    [Tooltip("Once the player is further away than this, the mushroom gives up and goes back to idle.")]
    public float loseTargetDistance = 16f;

    [Header("Keep Away")]
    [Tooltip("Player closer than this and the mushroom backs off instead of standing its ground.")]
    public float retreatTriggerDistance = 4f;
    [Tooltip("Shortest distance the mushroom actually runs when it backs off. This is how far it travels, not how far away it ends up.")]
    public float retreatDistanceMin = 2.5f;
    [Tooltip("Longest distance the mushroom actually runs when it backs off.")]
    public float retreatDistanceMax = 4.5f;
    [Tooltip("Move speed while backing off, as a multiple of Move Speed.")]
    public float retreatSpeedMultiplier = 1.2f;
    [Tooltip("After a hop away, or after being cornered by a wall or a ledge, stand and fight for this long before backing off again.")]
    public float retreatRetryTime = 1f;

    [Header("Bomb Throw")]
    [Tooltip("The bomb prefab to throw. Needs a Rigidbody2D; an AmmoBehaviour on it is hooked up automatically.")]
    public GameObject bombPrefab;
    [Tooltip("Where the bomb leaves the mushroom. Falls back to the mushroom's own position when empty.")]
    public Transform throwPoint;
    [Tooltip("Seconds into the attack before the bomb leaves the hand. An AttackPreparetion animation event throws it earlier.")]
    public float throwDelay = 0.4f;
    [Tooltip("Total length of the attack state. An AnimationTrigger animation event can end it earlier.")]
    public float attackDuration = 0.9f;
    [Tooltip("Random launch angle range in degrees. Higher = higher, slower arc.")]
    public float throwAngleMin = 35f;
    public float throwAngleMax = 65f;
    [Tooltip("Speed cap on the throw. A target too far to reach at this speed is undershot instead of overshot.")]
    public float maxThrowSpeed = 14f;
    [Tooltip("Sideways spread of the landing spot, so bombs land on the player or around him.")]
    public float throwScatter = 1.5f;
    [Tooltip("Seconds before an unexploded bomb is cleaned up. 0 = never.")]
    public float bombLifeTime = 5f;

    // Lives on the enemy rather than the battle state, so throwing a bomb and coming back
    // does not reset the gap between hops.
    [HideInInspector] public float retreatCooldownTimer;

    protected override void Awake()
    {
        base.Awake();

        idleState = new MushroomIdleState(this, stateMachine, "Idle", this);
        walkState = new MushroomWalkState(this, stateMachine, "Walk", this);
        battleState = new MushroomBattleState(this, stateMachine, "Walk", this);
        attackState = new MushroomAttackState(this, stateMachine, "Attack", this);
        deathState = new MushroomDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        retreatCooldownTimer -= Time.deltaTime;
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }

    /// <summary>
    /// Lobs a bomb at the player, or a little to either side of him. The launch angle is picked at
    /// random inside the angle range and the speed is solved so the arc passes through that spot,
    /// so the bomb always travels on a curve rather than a straight line.
    /// </summary>
    public void ThrowBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogWarning($"[Enemy_Mushroom] '{gameObject.name}' has no Bomb Prefab assigned.");
            return;
        }

        Transform player = PlayerManager.Instance.player.transform;

        Vector2 startPos = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;
        Vector2 targetPos = (Vector2)player.position + new Vector2(Random.Range(-throwScatter, throwScatter), 0f);

        float dx = targetPos.x - startPos.x;
        float dy = targetPos.y - startPos.y;

        int throwDir = dx >= 0f ? 1 : -1;
        dx = Mathf.Abs(dx);

        // The bomb's own gravity scale decides how steeply it falls, so solve against that.
        Rigidbody2D prefabRb = bombPrefab.GetComponent<Rigidbody2D>();
        float gravity = Mathf.Abs(Physics2D.gravity.y * (prefabRb != null ? prefabRb.gravityScale : 1f));

        float angleRad = Random.Range(throwAngleMin, throwAngleMax) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float denominator = 2f * cos * cos * ((dx * Mathf.Tan(angleRad)) - dy);
        float speedSqr = denominator > 0f ? (gravity * dx * dx) / denominator : -1f;

        // Unreachable at this angle (target above the arc) or no gravity to curve the flight:
        // just throw at full strength instead of skipping the attack.
        float speed = speedSqr > 0f && gravity > 0f
            ? Mathf.Min(Mathf.Sqrt(speedSqr), maxThrowSpeed)
            : maxThrowSpeed;

        GameObject bomb = Instantiate(bombPrefab, startPos, Quaternion.identity);

        Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
        if (bombRb != null)
            bombRb.velocity = new Vector2(speed * cos * throwDir, speed * sin);

        // Same hookup the other enemy projectiles use, so the damage comes from this enemy's stats.
        AmmoBehaviour ammo = bomb.GetComponent<AmmoBehaviour>();
        if (ammo != null)
            ammo.Initialize(this);

        if (bombLifeTime > 0f)
            Destroy(bomb, bombLifeTime);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, retreatTriggerDistance);

        // Sight reach ahead against sight reach behind, so the two ranges can be eyeballed.
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.right * facingDir * playerCheckDistance));

        Gizmos.color = Color.grey;
        Gizmos.DrawLine(transform.position, transform.position - (Vector3.right * facingDir * behindCheckDistance));
    }
}
