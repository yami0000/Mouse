using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Boss_Magician : Enemy
{
    private Transform player;
    public MagicianStartState startState { get; private set; }
    public MagicianPrepareState prepareState { get; private set; }
    public MagicianIdleState idleState { get; private set; }
    public MagicianWalkState walkState { get; private set; }
    public MagicianSpikeState spikeState { get; private set; }
    public MagicianBarrageState barrageState { get; private set; }
    public MagicianSwitchState switchState { get; private set; }
    public MagicianChopState chopState { get; private set; }

    private EnemyStats es;
    public bool EnterStageTwo = false;

    [Header("Skill_Spike")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private int spikeCount;
    [SerializeField] private float spikeStartOffset;    // X1: distance ahead of boss for first spike
    [SerializeField] private float spikeSpacing;        // D: distance between spikes
    [SerializeField] private float spikeInterval;       // t: seconds between each spawn
    [SerializeField] private float spikeLifetime = 5f;  // auto-cleanup, set 0 to keep forever
    [SerializeField] private float groundRayHeight = 5f;  // how far above the query point the ray starts
    [SerializeField] private float groundRayLength = 20f; // how far BELOW the query point the ray probes

    [Header("Floating Barrage - Fire")]
    [SerializeField] private GameObject barragePrefab;
    [SerializeField] private Transform barrageFirePoint;
    [SerializeField] private int barrageCount;              // X: number of projectiles
    [SerializeField] private float barrageFireInterval;     // t: seconds between shots
    [SerializeField] private float barrageAngleMin;         // R range: degrees from horizontal facing dir
    [SerializeField] private float barrageAngleMax;
    [SerializeField] private float barrageFireSpeed;        // v: initial flight speed
    [SerializeField] private float barrageTravelTime = 0.5f;// time from muzzle onto the orbit (keep short)
    [Header("Floating Barrage - Orbit")]
    [SerializeField] private float orbitRadius = 3f;            // R: circle radius around the boss
    [SerializeField] private float orbitSpeed = 60f;            // v: degrees per second (negative = clockwise)
    [SerializeField] private Vector2 orbitCenterOffset;         // offset from boss position, (0,0) = exactly the boss
    [Tooltip("How firmly projectiles are pulled into their equal-spacing slots. Higher = settles faster. ~2-6")]
    [SerializeField] private float redistributeStrength = 4f;
    [Tooltip("Max deg/s a projectile may deviate from orbitSpeed while redistributing. Caps how 'fierce' it can look.")]
    [SerializeField] private float maxCatchUpSpeed = 90f;
    [Header("Floating Barrage - Launch")]
    [SerializeField] private float launchCheckInterval = 1f;    // T: roll the dice every T seconds
    [Range(0f, 100f)]
    [SerializeField] private float launchChance = 40f;          // P: % chance per roll to fire a volley
    [SerializeField] private float launchStartSpeed = 2f;
    [SerializeField] private float launchAcceleration = 30f;
    [SerializeField] private float launchMaxSpeed = 25f;
    [SerializeField] private float barrageLifetime = 6f;        // max flight time after launch

    // --- orbit management (centralized driver) ---
    private class Orbiter
    {
        public Transform tf;
        public float angle;       // current angle on the circle, degrees
        public float angularVel;  // current angular velocity, deg/s
        public float slotOffset;  // assigned slot, degrees relative to orbitBaseAngle
    }
    private readonly List<Orbiter> orbiters = new List<Orbiter>();
    private Coroutine orbitDriver;
    private float orbitBaseAngle; // shared reference angle, advances at orbitSpeed

    [Header("Side Dash")]
    [SerializeField] private float sideDashTime = 0.25f;        // t: dash duration
    [SerializeField] private float sideDashDistance = 2f;       // X: gap to the player on the x axis
    [Header("Side Dash - Afterimage")]
    [SerializeField] private float ghostInterval = 0.03f;       // seconds between ghost snapshots
    [SerializeField] private float ghostFadeTime = 0.25f;       // how long each ghost lingers
    [SerializeField] private Color ghostTint = new Color(1f, 1f, 1f, 0.5f);

    [HideInInspector] public float StateTimer_Magician;

    protected override void Awake()
    {
        base.Awake();

        startState = new MagicianStartState(this, stateMachine, "Start", this);
        prepareState = new MagicianPrepareState(this, stateMachine, "Prepare", this);
        idleState = new MagicianIdleState(this, stateMachine, "Idle", this);
        walkState = new MagicianWalkState(this, stateMachine, "Move", this);
        spikeState = new MagicianSpikeState(this, stateMachine, "Spike", this);
        barrageState = new MagicianBarrageState(this, stateMachine, "Barrage", this);
        switchState = new MagicianSwitchState(this, stateMachine, "Switch", this);
        chopState = new MagicianChopState(this, stateMachine, "Chop", this);
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(startState);
        es = GetComponent<EnemyStats>();

        player = PlayerManager.Instance.player.transform;
    }


    protected override void Update()
    {
        base.Update();
        StateTimer_Magician -= Time.deltaTime;
    }

    // Returns true and outputs the ground y under nearPos.x. Casts from well above
    // the given position so a jumping player can't out-range the ray.
    private bool TryGetGroundY(Vector2 nearPos, out float groundY)
    {
        Vector2 rayOrigin = new Vector2(nearPos.x, nearPos.y + groundRayHeight);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down,
            groundRayHeight + groundRayLength, whatIsGround);

        groundY = hit.collider != null ? hit.point.y : 0f;
        return hit.collider != null;
    }

    public void Spike() => StartCoroutine(Random.value > 0.5f ? Spike1Coroutine() : Spike2Coroutine());
    IEnumerator Spike1Coroutine()
    {
        // Lock in position and direction at trigger time so the wave
        // doesn't bend if the boss moves or flips mid-attack
        Vector2 origin = transform.position;
        int dir = facingDir;

        for (int i = 0; i < spikeCount; i++)
        {
            Vector2 spawnPos = origin + new Vector2(dir * (spikeStartOffset + spikeSpacing * i), 0f);

            if (TryGetGroundY(spawnPos, out float groundY))
                spawnPos.y = groundY;
            else
                continue; // no ground under this x (pit/ledge) ¡ª skip this spike

            GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
            spike.GetComponent<Collider>()?.Initialize(this);

            if (spikeLifetime > 0f)
                Destroy(spike, spikeLifetime);

            if (spikeInterval > 0f)
                yield return new WaitForSeconds(spikeInterval);
        }
    }
    IEnumerator Spike2Coroutine()
    {
        Vector2 spawnPos = player.position;

        if (TryGetGroundY(spawnPos, out float groundY))
            spawnPos.y = groundY;
        else
            yield break; // player is over a pit ¡ª no spike rather than a floating one

        GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
        spike.GetComponent<Collider>()?.Initialize(this);

        if (spikeLifetime > 0f)
            Destroy(spike, spikeLifetime);

        yield return null;

    }

    private Vector2 OrbitCenter => (Vector2)transform.position + orbitCenterOffset;

    private static Vector2 AngleToDir(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public void Barrage()
    {
        StartCoroutine(BarrageRoutine());
    }
    IEnumerator BarrageRoutine()
    {
        yield return new WaitForSeconds(0.33f);

        // --- Phase 1: fire one by one ---
        for (int i = 0; i < barrageCount; i++)
        {
            float angle = Random.Range(barrageAngleMin, barrageAngleMax);
            Vector2 fireDir = Quaternion.Euler(0, 0, angle * facingDir) * new Vector2(facingDir, 0);

            GameObject proj = Instantiate(barragePrefab, barrageFirePoint.position,
                Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, fireDir)));

            Collider ammo = proj.GetComponent<Collider>();
            if (ammo != null)
                ammo.Initialize(this);

            StartCoroutine(DriveToOrbit(proj.transform, fireDir));

            yield return new WaitForSeconds(barrageFireInterval);
        }

        // wait for the last one to reach the orbit
        yield return new WaitForSeconds(barrageTravelTime);

        // --- Phase 2: every T seconds, P% chance to fire a random number of projectiles ---
        while (orbiters.Count > 0)
        {
            yield return new WaitForSeconds(launchCheckInterval);
            if (orbiters.Count == 0) break;

            if (Random.value < launchChance * 0.01f)
            {
                int volley = Random.Range(1, orbiters.Count + 1);
                for (int j = 0; j < volley; j++)
                {
                    int pick = Random.Range(0, orbiters.Count);
                    Transform tf = orbiters[pick].tf;
                    orbiters.RemoveAt(pick);

                    if (tf != null)
                        StartCoroutine(LaunchAtPlayer(tf));
                }
                ReassignSlots(); // survivors smoothly accelerate into their new spacing
            }
        }
    }

    IEnumerator DriveToOrbit(Transform proj, Vector2 fireDir)
    {
        Vector2 start = proj.position;
        Quaternion startRot = proj.rotation;
        float tangentSign = orbitSpeed >= 0f ? 1f : -1f;

        // Enter the orbit where the projectile's own momentum is headed:
        // the quickest, most natural point to merge onto the circle
        Vector2 predicted = start + fireDir * (barrageFireSpeed * barrageTravelTime);
        float entryAngle = Vector2.SignedAngle(Vector2.right, predicted - OrbitCenter);

        float elapsed = 0f;
        while (elapsed < barrageTravelTime)
        {
            if (proj == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / barrageTravelTime);
            float w = t * t * (3f - 2f * t); // smoothstep blend weight

            // the entry point itself rotates, so the handover is seamless
            entryAngle += orbitSpeed * Time.deltaTime;

            // vector 1: where pure straight flight would have taken it
            Vector2 straight = start + fireDir * (barrageFireSpeed * elapsed);
            // vector 2: the (moving) entry point on the orbit
            Vector2 target = OrbitCenter + AngleToDir(entryAngle) * orbitRadius;

            // blend the two -> one continuous curve instead of two line segments
            proj.position = Vector2.Lerp(straight, target, w);
            proj.rotation = Quaternion.Lerp(startRot,
                Quaternion.Euler(0, 0, entryAngle + 90f * tangentSign), w);
            yield return null;
        }

        if (proj == null) yield break;

        // join the orbit already rotating at formation speed;
        // redistribution happens purely through gentle acceleration
        orbiters.Add(new Orbiter { tf = proj, angle = entryAngle, angularVel = orbitSpeed });
        ReassignSlots();

        if (orbitDriver == null)
            orbitDriver = StartCoroutine(OrbitDriver());
    }

    // Re-deal the equal-spacing slots (360/N apart) while preserving the current
    // circular order, so nobody has to cross over anyone else to reach their slot.
    private void ReassignSlots()
    {
        int n = orbiters.Count;
        if (n == 0) return;

        orbiters.Sort((a, b) =>
            Mathf.Repeat(a.angle - orbitBaseAngle, 360f)
            .CompareTo(Mathf.Repeat(b.angle - orbitBaseAngle, 360f)));

        float step = 360f / n;
        for (int i = 0; i < n; i++)
            orbiters[i].slotOffset = i * step;
    }

    IEnumerator OrbitDriver()
    {
        float tangentSign = orbitSpeed >= 0f ? 1f : -1f;

        while (orbiters.Count > 0)
        {
            float dt = Time.deltaTime;
            orbitBaseAngle = Mathf.Repeat(orbitBaseAngle + orbitSpeed * dt, 360f);
            Vector2 center = OrbitCenter;

            bool anyDestroyed = false;
            for (int i = orbiters.Count - 1; i >= 0; i--)
            {
                Orbiter o = orbiters[i];
                if (o.tf == null) { orbiters.RemoveAt(i); anyDestroyed = true; continue; }

                // Critically damped spring ON VELOCITY, not position:
                // a projectile behind its slot gently accelerates, one ahead gently
                // brakes, and the deviation from orbitSpeed is hard-capped so the
                // motion always reads as "same orbit, slightly different speed".
                float targetAngle = orbitBaseAngle + o.slotOffset;
                float error = Mathf.DeltaAngle(o.angle, targetAngle);
                float relVel = o.angularVel - orbitSpeed;

                float accel = redistributeStrength * redistributeStrength * error
                              - 2f * redistributeStrength * relVel;

                relVel = Mathf.Clamp(relVel + accel * dt, -maxCatchUpSpeed, maxCatchUpSpeed);
                o.angularVel = orbitSpeed + relVel;
                o.angle += o.angularVel * dt;

                o.tf.position = center + AngleToDir(o.angle) * orbitRadius;
                o.tf.rotation = Quaternion.Euler(0, 0, o.angle + 90f * tangentSign);
            }

            if (anyDestroyed)
                ReassignSlots();

            yield return null;
        }
        orbitDriver = null;
    }

    IEnumerator LaunchAtPlayer(Transform proj)
    {
        proj.GetComponent<Collider>()?.SetColliderEnabled(true); // force-arm regardless of A/B window

        Vector2 dir = ((Vector2)player.position - (Vector2)proj.position).normalized;
        proj.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir));

        float speed = launchStartSpeed;
        float elapsed = 0f;
        while (elapsed < barrageLifetime)
        {
            if (proj == null) yield break;

            speed = Mathf.Min(speed + launchAcceleration * Time.deltaTime, launchMaxSpeed);
            proj.position += (Vector3)(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (proj != null)
            Destroy(proj.gameObject);
    }

    public void SideDash() => StartCoroutine(SideDashCoroutine());

    IEnumerator SideDashCoroutine()
    {
        TurnToPlayer();

        yield return new WaitForSeconds(1.65f-sideDashTime);

        TurnToPlayer();

        Vector2 startPos = transform.position;
        // land beside the player: X gap on the x axis, on the boss's approach side
        Vector2 targetPos = new Vector2(Random.value<=0.5f? player.position.x - sideDashDistance: player.position.x + sideDashDistance, startPos.y);

       

        float elapsed = 0f;
        float ghostTimer = 0f;

        while (elapsed < sideDashTime)
        {
            elapsed += Time.deltaTime;
            float easedT = Mathf.SmoothStep(0f, 1f, elapsed / sideDashTime);
            transform.position = Vector2.Lerp(startPos, targetPos, easedT);
            
            // afterimage trail = the 2D "motion blur"
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
            {
                SpawnGhosts();
                ghostTimer = ghostInterval;
            }

            yield return null;
        }
        TurnToPlayer();
        zerovelocity();


    }

    // Snapshot every SpriteRenderer under the boss into a fading ghost copy
    private void SpawnGhosts()
    {
        foreach (SpriteRenderer src in GetComponentsInChildren<SpriteRenderer>())
        {
            if (!src.enabled || src.sprite == null) continue;

            GameObject ghost = new GameObject("DashGhost");
            ghost.transform.SetPositionAndRotation(src.transform.position, src.transform.rotation);
            ghost.transform.localScale = src.transform.lossyScale;

            SpriteRenderer gr = ghost.AddComponent<SpriteRenderer>();
            gr.sprite = src.sprite;
            gr.flipX = src.flipX;
            gr.flipY = src.flipY;
            gr.sortingLayerID = src.sortingLayerID;
            gr.sortingOrder = src.sortingOrder - 1; // behind the real boss
            gr.color = src.color * ghostTint;

            ghost.AddComponent<AfterimageGhost>().StartFade(ghostFadeTime);
        }
    }
    public bool StageChange()
    { 
        
        return es.CurrentHP / es.GetMaxHealth() <= 0.6f ? true : false; 
    }
    public override void Die()
    {
        base.Die();
        //stateMachine.ChangeState(deathState);
    }
}