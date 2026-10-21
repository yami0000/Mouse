using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Axe brute with two attack patterns.
/// Pattern 1 is a single state: walk into range and chop.
/// Pattern 2 is three states in a fixed order: prepare, charge, upstroke.
/// The battle state picks between them, weighted by how far away the player is.
/// </summary>
public class Enemy_Heavy : Enemy
{
    public HeavyIdleState idleState { get; private set; }
    public HeavyWalkState walkState { get; private set; }
    public HeavyBattleState battleState { get; private set; }
    public HeavyChopState chopState { get; private set; }
    public HeavyLiftState liftState { get; private set; }
    public HeavyChargeState chargeState { get; private set; }
    public HeavyUpstrokeState upstrokeState { get; private set; }
    public HeavyDeathState deathState { get; private set; }

    [Header("Battle Movement")]
    [Tooltip("Once the player is further away than this, and out of sight, the Heavy gives up and goes back to idle.")]
    public float loseTargetDistance = 16f;
    [Tooltip("How long the Heavy will give ground to make room for his chosen pattern before he gives up " +
             "and attacks from where he stands. Keeps a player who follows him from walking him backwards for ever.")]
    public float maxGiveGroundTime = 1.2f;

    [Header("Pattern Choice")]
    [Tooltip("Above this x axis distance the Heavy leans towards charging, because he has room to build up speed.")]
    public float chargePreferDistance = 8f;
    [Tooltip("Chance to pick the charge when the player is further away than Charge Prefer Distance.")]
    [Range(0f, 1f)] public float chargeChanceWhenFar = 0.75f;
    [Tooltip("Under this x axis distance the player is in his face, and the Heavy nearly always charges to shove them off.")]
    public float chargeCrowdedDistance = 3f;
    [Tooltip("Chance to pick the charge when the player is nearer than Charge Crowded Distance.")]
    [Range(0f, 1f)] public float chargeChanceWhenCrowded = 0.9f;
    [Tooltip("Chance to pick the charge in the comfortable axe range between the crowded and prefer distances.")]
    [Range(0f, 1f)] public float chargeChanceWhenNear = 0.15f;

    [Header("Pattern 1 - Chop")]
    [Tooltip("Closest x axis distance the chop is thrown from. Inside this the Heavy gives himself room first.")]
    public float chopRangeMin = 1.5f;
    [Tooltip("Furthest x axis distance the chop is thrown from.")]
    public float chopRangeMax = 3f;
    [Tooltip("Total length of the chop state. An AnimationTrigger animation event can end it earlier.")]
    public float chopDuration = 0.9f;
    [Tooltip("Seconds into the swing before the hitbox appears. An AttackPreparetion animation event spawns it earlier.")]
    public float chopHitDelay = 0.35f;
    [Tooltip("Hitbox spawned by the chop. Carries its own animator.")]
    public GameObject chopHitboxPrefab;
    [Tooltip("Seconds before the chop hitbox is cleaned up. 0 = it cleans itself up.")]
    public float chopHitboxLifeTime = 0.5f;

    [Header("Pattern 2 - Lift")]
    [Tooltip("How long the Heavy spends lifting the axe into position before he starts running. " +
             "An AnimationTrigger animation event can end it earlier.")]
    public float liftTime = 0.5f;

    [Header("Pattern 2 - Charge")]
    [Tooltip("Shortest x axis distance worth charging from, so the Heavy has room to build up speed.")]
    public float chargeMinDistance = 6f;
    [Tooltip("Run speed during the charge. Not a multiplier, this replaces Move Speed.")]
    public float chargeSpeed = 12f;
    [Tooltip("The sprint always runs at least this long before reaching the upstroke band is allowed to end it, " +
             "so a charge from close range still reads as a charge. Landing the axe or hitting a wall still ends it at once.")]
    public float minChargeTime = 0.2f;
    [Tooltip("Safety net: the charge gives up after this long even if it never reaches upstroke range.")]
    public float maxChargeTime = 3f;
    [Tooltip("How far the Heavy skids forward after the sprint ends, slowing to a stop over that distance, " +
             "before the upstroke starts. 0 = stop dead.")]
    public float brakeDistance = 1.5f;
    [Tooltip("Static hitbox carried during the run, the levelled axe. Lives exactly as long as the charge.")]
    public GameObject chargeHitboxPrefab;

    [Header("Pattern 2 - Upstroke")]
    [Tooltip("Inner edge of the band the upstroke is thrown in. Nearer than this means he overshot, and he swings late.")]
    public float upstrokeRangeMin = 1f;
    [Tooltip("Outer edge of the band the upstroke is thrown in. Reaching this ends the run.")]
    public float upstrokeRangeMax = 2.5f;
    [Tooltip("Total length of the upstroke state. An AnimationTrigger animation event can end it earlier.")]
    public float upstrokeDuration = 0.9f;
    [Tooltip("Seconds into the upstroke before the hitbox appears. An AttackPreparetion animation event spawns it earlier.")]
    public float upstrokeHitDelay = 0.2f;
    [Tooltip("Hitbox spawned by the upstroke. Carries its own animator.")]
    public GameObject upstrokeHitboxPrefab;
    [Tooltip("Seconds before the upstroke hitbox is cleaned up. 0 = it cleans itself up.")]
    public float upstrokeHitboxLifeTime = 0.5f;

    [Header("Hitboxes")]
    [Tooltip("Where spawned hitboxes appear. Falls back to the Heavy himself when empty.")]
    public Transform hitboxMountPoint;

    private GameObject chargeHitbox;
    private AmmoBehaviour chargeHitboxAmmo;

    protected override void Awake()
    {
        base.Awake();

        idleState = new HeavyIdleState(this, stateMachine, "Idle", this);
        walkState = new HeavyWalkState(this, stateMachine, "Walk", this);
        battleState = new HeavyBattleState(this, stateMachine, "Walk", this);
        chopState = new HeavyChopState(this, stateMachine, "Chop", this);
        liftState = new HeavyLiftState(this, stateMachine, "Lift", this);
        chargeState = new HeavyChargeState(this, stateMachine, "Charge", this);
        upstrokeState = new HeavyUpstrokeState(this, stateMachine, "Upstroke", this);
        deathState = new HeavyDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Die()
    {
        base.Die();

        // The levelled axe must not outlive him, LeaveBody corpses included.
        DespawnChargeHitbox();
        stateMachine.ChangeState(deathState);
    }

    /// <summary>Gap to the player on the x axis only, which is what every range in here is measured on.</summary>
    public float DistanceToPlayerX()
    {
        Transform player = PlayerManager.Instance.player.transform;
        return Mathf.Abs(player.position.x - transform.position.x);
    }

    /// <summary>True once the player has ended up behind the Heavy, so a charge has run past them.</summary>
    public bool IsPlayerBehind()
    {
        Transform player = PlayerManager.Instance.player.transform;
        return (player.position.x - transform.position.x) * facingDir < 0f;
    }

    /// <summary>
    /// Rolls the pattern for this attack across three bands of distance. Far away favours the
    /// charge because there is room to build up speed. Crowded favours it even harder, as a way
    /// to shove the player off him. The comfortable middle is where the axe does the work.
    /// Every weight is exposed, so the split can be retuned without touching the states.
    /// </summary>
    public bool WantsToCharge(float _distanceX)
    {
        float chance;

        if (_distanceX < chargeCrowdedDistance)
            chance = chargeChanceWhenCrowded;
        else if (_distanceX > chargePreferDistance)
            chance = chargeChanceWhenFar;
        else
            chance = chargeChanceWhenNear;

        return Random.value < chance;
    }

    /// <summary>
    /// Spawns one of the axe hitboxes and hands it this enemy, which is what lets the
    /// AmmoBehaviour on it work out whose stats the damage comes off. All three ways the Heavy
    /// deals damage go through here: the chop, the levelled axe during the charge, the upstroke.
    /// </summary>
    public GameObject SpawnHitbox(GameObject _prefab, float _lifeTime, bool _followTheHeavy)
    {
        if (_prefab == null)
            return null;

        Transform mount = hitboxMountPoint != null ? hitboxMountPoint : transform;

        GameObject hitbox;

        if (_followTheHeavy)
            // Parented, keeping the prefab's own transform as a local offset, so an axe placed
            // out in front in the prefab stays out in front. It also inherits the Heavy's
            // 180 degree flip, so it lands on the side he faces with no extra maths.
            hitbox = Instantiate(_prefab, mount);
        else
            hitbox = Instantiate(_prefab, mount.position, mount.rotation);

        // Same hookup the Magician uses for its hazards. Children included, so it still works
        // when the collider sits on a child of the prefab rather than its root.
        AmmoBehaviour[] hazards = hitbox.GetComponentsInChildren<AmmoBehaviour>(true);

        foreach (AmmoBehaviour hazard in hazards)
            hazard.Initialize(this);

        if (hazards.Length == 0)
            Debug.LogWarning($"[Enemy_Heavy] '{_prefab.name}' has no AmmoBehaviour on it or its children, so it will never deal damage.");

        if (_lifeTime > 0f)
            Destroy(hitbox, _lifeTime);

        return hitbox;
    }

    /// <summary>True once the levelled axe has connected with the player during this run.</summary>
    public bool ChargeHitboxHasHit { get; private set; }

    /// <summary>Puts the axe out in front for the run. Kept alive by hand, not by a timer.</summary>
    public void SpawnChargeHitbox()
    {
        DespawnChargeHitbox();

        ChargeHitboxHasHit = false;
        chargeHitbox = SpawnHitbox(chargeHitboxPrefab, 0f, true);

        if (chargeHitbox == null)
            return;

        // The flag has to live out here on the Heavy: the hitbox has Destroy On Hit set, so it
        // is already gone by the time the charge state next looks at it.
        chargeHitboxAmmo = chargeHitbox.GetComponentInChildren<AmmoBehaviour>(true);

        if (chargeHitboxAmmo != null)
            chargeHitboxAmmo.OnTargetAffected += OnChargeHitboxConnected;
    }

    /// <summary>Puts the axe back. Safe to call when there is nothing out.</summary>
    public void DespawnChargeHitbox()
    {
        if (chargeHitboxAmmo != null)
            chargeHitboxAmmo.OnTargetAffected -= OnChargeHitboxConnected;

        chargeHitboxAmmo = null;

        if (chargeHitbox == null)
            return;

        Destroy(chargeHitbox);
        chargeHitbox = null;
    }

    private void OnChargeHitboxConnected(AmmoBehaviour _hitbox, EntityStats _target)
    {
        // Only a flag here. The charge state reads it in its own Update, so the state change
        // never happens in the middle of a physics callback.
        ChargeHitboxHasHit = true;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Vector3 origin = transform.position;
        Vector3 ahead = Vector3.right * facingDir;

        // Chop band, the sweet spot for pattern 1.
        Gizmos.color = Color.yellow;
        DrawRangeMarker(origin + (ahead * chopRangeMin));
        DrawRangeMarker(origin + (ahead * chopRangeMax));

        // Upstroke band, where pattern 2 ends its run.
        Gizmos.color = Color.magenta;
        DrawRangeMarker(origin + (ahead * upstrokeRangeMin));
        DrawRangeMarker(origin + (ahead * upstrokeRangeMax));

        // Where charging becomes worthwhile, and where it becomes the preferred pattern.
        Gizmos.color = Color.cyan;
        DrawRangeMarker(origin + (ahead * chargeMinDistance));

        Gizmos.color = Color.green;
        DrawRangeMarker(origin + (ahead * chargePreferDistance));

        // Crowded: inside here he nearly always charges to shove the player off.
        Gizmos.color = Color.red;
        DrawRangeMarker(origin + (ahead * chargeCrowdedDistance));
    }

    private static void DrawRangeMarker(Vector3 _position)
    {
        Gizmos.DrawLine(_position + (Vector3.up * 0.5f), _position - (Vector3.up * 0.5f));
    }
}
