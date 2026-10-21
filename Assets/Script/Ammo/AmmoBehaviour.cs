using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Who fired this. Decides what counts as a target and where the stats come from.</summary>
public enum AmmoSide
{
    Enemy,      // damages the player, heals other enemies
    Player      // damages enemies
}

/// <summary>What happens when the ammo reaches something it cares about.</summary>
public enum AmmoImpactMode
{
    Damage,     // normal hit
    Heal,       // heals an ally instead of damaging a target (Medic)
    SpawnOnly   // deals no damage itself, only spawns the Impact Prefab (Grenade)
}

/// <summary>How the damage multiplier is worked out.</summary>
public enum AmmoDamageMode
{
    Flat,           // always x1
    RandomRange,    // rolled between Damage Multiplier Min/Max (Revolver)
    ChargeCurve     // read off Damage Curve using how long fire was held (Charge shot)
}

/// <summary>How the ammo moves after it is fired.</summary>
public enum AmmoHomingMode
{
    None,       // whatever velocity it was launched with, plus gravity
    Homing      // coasts to a stop, then accelerates at a target
}

/// <summary>
/// Anything spawned by a projectile that then deals damage of its own: an explosion, a
/// lingering cloud, a shockwave. Implement this and whoever spawns it hands over the stats
/// the damage should come from, with no spawner needing to know the concrete type.
/// </summary>
public interface IExplosionEffect
{
    void Initialize(EntityStats _ownerStats);
}

/// <summary>
/// One behaviour script for every projectile in the game, enemy or player.
/// Set Side to say who fired it, then tick the features this particular ammo needs.
/// Everything here is virtual, so a projectile that needs genuinely new logic can
/// subclass this and override a single method instead of being rewritten from scratch.
/// </summary>
public class AmmoBehaviour : MonoBehaviour
{
    [Tooltip("Enemy ammo damages the player and can heal enemies. Player ammo damages enemies.\n" +
             "Player ammo finds the player on its own; enemy ammo is handed its owner by whoever fires it.")]
    public AmmoSide side = AmmoSide.Enemy;

    [Tooltip("Damage = normal hit. Heal = heals an ally. SpawnOnly = no damage of its own, just spawns the Impact Prefab.")]
    public AmmoImpactMode impactMode = AmmoImpactMode.Damage;
    [Tooltip("Only ever affects one target, then stops. Leave on for anything that should not multi-hit.")]
    public bool hitOnce = true;
    [Tooltip("Destroy this object after it affects a target.")]
    public bool destroyOnHit = true;
    [Tooltip("Spawned where the hit happened. Any IExplosionEffect on it (Explode_Grenade, Explode_Poison) is handed the owner's stats automatically.")]
    public GameObject impactPrefab;
    [Tooltip("Seconds before the spawned Impact Prefab is cleaned up. 0 = leave it alone.")]
    public float impactPrefabLifeTime = 0.85f;

    public AmmoDamageMode damageMode = AmmoDamageMode.Flat;
    [Tooltip("RandomRange only: the multiplier is rolled between these two.")]
    public float damageMultiplierMin = 0.8f;
    public float damageMultiplierMax = 1.4f;
    [Tooltip("ChargeCurve only: X = seconds the fire button was held, Y = damage multiplier.")]
    public AnimationCurve damageCurve = AnimationCurve.Constant(0f, 1f, 1f);

    public AmmoHomingMode homingMode = AmmoHomingMode.None;
    [Tooltip("How quickly it bleeds off its launch velocity before homing starts.")]
    public float slowDownRate = 2f;
    [Tooltip("Seconds of coasting before it starts chasing.")]
    public float homingDelay = 0.5f;
    [Tooltip("How hard it accelerates once it is chasing.")]
    public float homingAcceleration = 10f;
    [Tooltip("Top speed while chasing.")]
    public float maxSpeed = 15f;
    [Tooltip("Player ammo only: what counts as an enemy when looking for something to chase.")]
    public LayerMask whatIsEnemy;
    [Tooltip("Player ammo only: how far from the player to look for something to chase.")]
    public float enemyCheckDistance = 10f;

    [Tooltip("Destroy on touching Ground. Off = flies through / lives on (the old Ground Proof flag).")]
    public bool destroyOnGround = true;
    [Tooltip("Reflect off Ground instead, keeping the speed it was launched with.")]
    public bool ricochetOffGround = false;
    [Tooltip("Burst into Split Prefab shards on touching Ground, then die.")]
    public bool splitOnGround = false;

    public GameObject splitPrefab;
    [Tooltip("How many shards. They fan out alternately left and right.")]
    public int splitCount = 2;
    public float splitMinSpeed = 5f;
    public float splitMaxSpeed = 10f;
    [Tooltip("Launch angle range of the shards, in degrees above horizontal.")]
    public float splitMinAngle = 30f;
    public float splitMaxAngle = 60f;
    [Tooltip("Seconds before a shard cleans itself up. 0 = leave it alone.")]
    public float splitLifeTime = 5f;

    [Tooltip("Keep the collider off until Arm Time, for hazards whose animation has a wind up.")]
    public bool useColliderWindow = false;
    [Tooltip("Seconds after spawn before the collider turns on.")]
    public float armTime = 0f;
    [Tooltip("Seconds after spawn before it turns back off. Leave at or below Arm Time to stay armed.")]
    public float disarmTime = 0f;

    [Tooltip("Seconds before it destroys itself. 0 = whoever fired it decides.")]
    public float lifeTime = 0f;

    [Tooltip("Splash spawned on a damaging hit, aimed back along the flight path.")]
    public GameObject splashEffectPrefab;
    [Tooltip("X = damage dealt, Y = how many splash particles to burst.")]
    public AnimationCurve damageToParticleCount = AnimationCurve.Constant(0f, 1f, 1f);

    [HideInInspector] public Vector2 finalDirection;
    [HideInInspector] public Vector2 lastFramePosition;
    [HideInInspector] public SpriteRenderer sp;

    /// <summary>
    /// Raised the moment this ammo damages or heals something, before it destroys itself.
    /// Whoever spawned it can react to the connection, for example a charge that should end
    /// as soon as its axe lands. Reading it off the hitbox afterwards is not an option when
    /// Destroy On Hit is set, because by then the component is gone.
    /// </summary>
    public event System.Action<AmmoBehaviour, EntityStats> OnTargetAffected;

    protected Entity owner;
    protected EntityStats ownerStats;

    protected Rigidbody2D rb;
    protected Collider2D cd;

    protected Transform homingTarget;
    protected float homingTimer;

    private Vector2 lastVelocity;
    private float launchSpeed;
    private float chargeTime;
    private readonly HashSet<GameObject> alreadyAffected = new HashSet<GameObject>();

    protected virtual void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        // Player ammo is spawned by an item effect that never calls Initialize, so it picks
        // its own owner up here. Enemy ammo is handed its owner by the enemy that fires it.
        if (side == AmmoSide.Player && ownerStats == null)
            Initialize(PlayerManager.Instance.player);

        if (rb != null)
        {
            lastVelocity = rb.velocity;
            launchSpeed = rb.velocity.magnitude;
        }

        // A charged shot reads the hold time off the player and clears it, so the next
        // shot out of the same weapon starts from zero.
        if (damageMode == AmmoDamageMode.ChargeCurve)
        {
            Player player = PlayerManager.Instance.player;

            if (player != null && player.currentChargeTimer > 0f)
            {
                chargeTime = player.currentChargeTimer;
                player.currentChargeTimer = 0f;
            }
        }

        if (homingMode == AmmoHomingMode.Homing)
            homingTarget = FindHomingTarget();

        if (useColliderWindow && cd != null)
            StartCoroutine(ColliderWindow());

        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Tells the ammo who fired it, so the damage or the healing comes off that entity's stats.
    /// Enemies, bosses and the player all go through here.
    /// </summary>
    public virtual void Initialize(Entity _owner)
    {
        owner = _owner;

        if (_owner != null)
            ownerStats = _owner.GetComponent<EntityStats>();
    }

    /// <summary>Forces the collider on or off, cancelling the arm/disarm window.</summary>
    public virtual void SetColliderEnabled(bool _on)
    {
        StopAllCoroutines();

        if (cd == null)
            cd = GetComponent<Collider2D>();

        if (cd != null)
            cd.enabled = _on;
    }

    protected virtual IEnumerator ColliderWindow()
    {
        cd.enabled = false;

        yield return new WaitForSeconds(armTime);
        cd.enabled = true;

        // Only disarm when there is a real window to close. The old version subtracted
        // blindly, so a disarm time below the arm time switched the collider straight back off.
        if (disarmTime > armTime)
        {
            yield return new WaitForSeconds(disarmTime - armTime);
            cd.enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (homingMode != AmmoHomingMode.Homing || rb == null)
            return;

        homingTimer += Time.deltaTime;

        if (homingTarget == null)
            return;

        if (homingTimer < homingDelay)
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, slowDownRate * Time.deltaTime);
        else
        {
            Vector2 direction = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
            rb.velocity = Vector2.MoveTowards(rb.velocity, direction * maxSpeed, homingAcceleration * Time.deltaTime);
        }
    }

    protected virtual void FixedUpdate()
    {
        lastFramePosition = transform.position;

        // Kept one physics step behind, so a ricochet reflects the velocity it came in with.
        if (rb != null && rb.velocity.sqrMagnitude > 0.0001f)
            lastVelocity = rb.velocity;
    }

    /// <summary>Enemy ammo chases the player; player ammo chases the nearest enemy.</summary>
    protected virtual Transform FindHomingTarget()
    {
        if (side == AmmoSide.Enemy)
            return PlayerManager.Instance.player.transform;

        Transform player = PlayerManager.Instance.player.transform;
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, enemyCheckDistance, whatIsEnemy);

        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            if (hit.GetComponent<Enemy>() == null)
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = hit.transform;
            }
        }

        return closest;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Shield"))
        {
            OnBlockedByShield();
            return;
        }

        if (impactMode == AmmoImpactMode.Heal)
        {
            if (TryHeal(collision))
                return;
        }
        else if (TryHit(collision))
            return;

        if (collision.CompareTag("Ground"))
            OnTouchedGround(transform.position);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground"))
            return;

        if (ricochetOffGround && rb != null)
        {
            Vector2 wallNormal = collision.contacts[0].normal;

            rb.velocity = Vector2.Reflect(lastVelocity.normalized, wallNormal) * launchSpeed;
            lastVelocity = rb.velocity;
            return;
        }

        // Ground reached through a solid collider rather than a trigger.
        if (destroyOnGround || splitOnGround)
            OnTouchedGround(collision.contacts[0].point);
    }

    /// <summary>Damages a hostile, if this collider is one. Returns true when it did something.</summary>
    protected virtual bool TryHit(Collider2D collision)
    {
        EntityStats target = GetHostileStats(collision);

        if (target == null)
            return false;

        if (hitOnce && alreadyAffected.Count > 0)
            return false;

        if (alreadyAffected.Contains(collision.gameObject))
            return false;

        alreadyAffected.Add(collision.gameObject);

        if (impactMode == AmmoImpactMode.Damage && ownerStats != null)
        {
            int damageDealt = ApplyDamage(target);

            finalDirection = ((Vector2)transform.position - lastFramePosition).normalized;
            PlayImpactFX(finalDirection, damageDealt);
        }

        SpawnImpactPrefab(collision.transform.position);

        OnTargetAffected?.Invoke(this, target);

        if (destroyOnHit)
            Destroy(gameObject);

        return true;
    }

    /// <summary>Heals an ally, if this collider is one. Returns true when it did something.</summary>
    protected virtual bool TryHeal(Collider2D collision)
    {
        EntityStats target = GetAllyStats(collision);

        if (target == null || ownerStats == null)
            return false;

        ownerStats.Heal(target);
        PlayHealFX();
        SpawnImpactPrefab(collision.transform.position);

        OnTargetAffected?.Invoke(this, target);

        if (destroyOnHit)
            Destroy(gameObject);

        return true;
    }

    /// <summary>
    /// Works out the multiplier for this shot and applies the damage once.
    /// The old Collide_ scripts called DoDamage twice over, which dealt double damage.
    /// </summary>
    protected virtual int ApplyDamage(EntityStats _target)
    {
        float damageMultiplier = 1f;

        switch (damageMode)
        {
            case AmmoDamageMode.RandomRange:
                damageMultiplier = Random.Range(damageMultiplierMin, damageMultiplierMax);
                break;

            case AmmoDamageMode.ChargeCurve:
                damageMultiplier = damageCurve.Evaluate(chargeTime);
                chargeTime = 0f;
                break;
        }

        return ownerStats.DoDamage(_target, damageMultiplier);
    }

    protected virtual void OnTouchedGround(Vector2 _contactPoint)
    {
        if (splitOnGround)
        {
            Split(_contactPoint);
            Destroy(gameObject);
            return;
        }

        if (destroyOnGround)
        {
            SpawnImpactPrefab(transform.position);
            Destroy(gameObject);
        }
    }

    protected virtual void Split(Vector2 _spawnPos)
    {
        if (splitPrefab == null)
            return;

        for (int i = 0; i < splitCount; i++)
        {
            GameObject shard = Instantiate(splitPrefab, _spawnPos, Quaternion.identity);

            float angleRad = Random.Range(splitMinAngle, splitMaxAngle) * Mathf.Deg2Rad;
            float speed = Random.Range(splitMinSpeed, splitMaxSpeed);

            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            if (i % 2 == 1)
                direction.x *= -1f;   // fan out alternately, so a pair splits left and right

            Rigidbody2D shardRb = shard.GetComponent<Rigidbody2D>();
            if (shardRb != null)
                shardRb.velocity = direction * speed;

            AmmoBehaviour shardAmmo = shard.GetComponent<AmmoBehaviour>();
            if (shardAmmo != null)
                shardAmmo.Initialize(owner);

            if (splitLifeTime > 0f)
                Destroy(shard, splitLifeTime);
        }
    }

    protected virtual void SpawnImpactPrefab(Vector2 _position)
    {
        if (impactPrefab == null)
            return;

        GameObject spawned = Instantiate(impactPrefab, _position, Quaternion.identity);

        // Whatever it spawned does its own damage, so it needs to know whose stats to use.
        // Anything implementing IExplosionEffect is handed them, so a new kind of explosion
        // or lingering cloud works here without this method being touched.
        foreach (IExplosionEffect explosion in spawned.GetComponents<IExplosionEffect>())
            explosion.Initialize(ownerStats);

        if (impactPrefabLifeTime > 0f)
            Destroy(spawned, impactPrefabLifeTime);
    }

    /// <summary>The player's shield eats the shot, and refunds a little health with Metabolism.</summary>
    protected virtual void OnBlockedByShield()
    {
        if (SK.Instance != null && SK.Instance.Skill != null && SK.Instance.Skill.Metabolism)
        {
            EntityStats playerStats = PlayerManager.Instance.player.stats;

            playerStats.CurrentHP += (int)(0.015f * playerStats.GetMaxHealth());

            // The old copies of this threw the Clamp result away, so the refund could overheal.
            playerStats.CurrentHP = Mathf.Clamp(playerStats.CurrentHP, -100, playerStats.GetMaxHealth());
        }

        Destroy(gameObject);
    }

    protected virtual void PlayImpactFX(Vector2 _lastVelocity, int _damage)
    {
        if (splashEffectPrefab == null)
            return;

        Vector2 reverseDir = -_lastVelocity.normalized;
        float angle = Mathf.Atan2(reverseDir.y, reverseDir.x) * Mathf.Rad2Deg;

        GameObject effect = Instantiate(splashEffectPrefab, transform.position, Quaternion.Euler(angle, -90, 0));

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            Destroy(effect);
            return;
        }

        short particleCount = (short)Mathf.Clamp(damageToParticleCount.Evaluate(_damage), 0, 25);

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.SetBurst(0, new ParticleSystem.Burst(0, particleCount));

        ps.Play();
        Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    /// <summary>The Medic keeps its healing particles on a helper that follows the enemy it is treating.</summary>
    protected virtual void PlayHealFX()
    {
        Enemy_Medic medic = owner as Enemy_Medic;

        if (medic != null && medic.ParticleSystem != null)
            medic.ParticleSystem.ExcuteHealingFX();
    }

    protected virtual EntityStats GetHostileStats(Collider2D collision)
    {
        if (side == AmmoSide.Enemy)
            return collision.GetComponent<Player>() != null ? collision.GetComponent<PlayerStats>() : null;

        return collision.GetComponent<Enemy>() != null ? collision.GetComponent<EnemyStats>() : null;
    }

    protected virtual EntityStats GetAllyStats(Collider2D collision)
    {
        if (side == AmmoSide.Enemy)
        {
            Enemy ally = collision.GetComponent<Enemy>();

            if (ally == null)
                return null;

            // Never heal whoever fired it.
            if (owner != null && ally.gameObject == owner.gameObject)
                return null;

            return collision.GetComponent<EnemyStats>();
        }

        return collision.GetComponent<Player>() != null ? collision.GetComponent<PlayerStats>() : null;
    }
}
