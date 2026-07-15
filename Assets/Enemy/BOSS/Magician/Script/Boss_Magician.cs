using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Boss_Magician : Enemy
{
    private Transform player;
    public MagicianStartState startState{ get;private set; }   
    public MagicianPrepareState prepareState { get; private set; }
    public MagicianIdleState idleState { get; private set; }
    public MagicianWalkState walkState { get; private set; }
    public MagicianSpikeState spikeState { get; private set; }
    public MagicianBarrageState barrageState { get; private set; }

    [Header("Skill_Spike")]
    [SerializeField]private GameObject spikePrefab;
    [SerializeField]private int spikeCount;
    [SerializeField] private float spikeStartOffset;    // X1: distance ahead of boss for first spike
    [SerializeField] private float spikeSpacing;        // D: distance between spikes
    [SerializeField] private float spikeInterval;       // t: seconds between each spawn
    [SerializeField] private float spikeLifetime = 5f;  // auto-cleanup, set 0 to keep forever
    [SerializeField] private bool snapToGround = true;  // raycast down so spikes sit on terrain
    [SerializeField] private float groundRayHeight = 3f;
    [SerializeField] private float groundRayLength = 10f;

    [Header("Floating Barrage")]
    [SerializeField] private GameObject barragePrefab;
    [SerializeField] private Transform barrageFirePoint;
    [SerializeField] private int barrageCount;              // X: number of projectiles
    [SerializeField] private float barrageFireInterval;     // t: seconds between shots
    [SerializeField] private float barrageAngle;            // R: degrees from horizontal facing dir
    [SerializeField] private float barrageFireSpeed;        // v: initial flight speed
    [SerializeField] private float barrageFireTime = 0.4f;  // how long they fly straight before homing to the rect
    [SerializeField] private float barrageLerpTime = 0.5f;  // glide time into floating position
    [Header("Floating Rect (local to boss)")]
    [SerializeField] private Vector2 rectCenterOffset = new Vector2(0f, 4f); // x is mirrored by facingDir
    [SerializeField] private Vector2 rectSize = new Vector2(6f, 2f);
    [Header("Launch")]
    [SerializeField] private float floatWaitMin;            // T1
    [SerializeField] private float floatWaitMax;            // T2
    [SerializeField] private float launchInterval;          // T: seconds between launches
    [SerializeField] private float launchSpeed;
    [SerializeField] private float barrageLifetime = 6f;    // max flight time after launch


    [HideInInspector] public float StateTimer_Magician;

    protected override void Awake()
    {
        base.Awake();

        startState = new MagicianStartState(this, stateMachine, "Start",this);
        prepareState = new MagicianPrepareState(this, stateMachine, "Prepare", this);
        idleState = new MagicianIdleState(this, stateMachine, "Idle", this);
        walkState = new MagicianWalkState(this, stateMachine, "Move", this);
        spikeState = new MagicianSpikeState(this, stateMachine, "Spike", this);
        barrageState = new MagicianBarrageState(this, stateMachine, "Barrage", this);
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(startState);

        player = PlayerManager.Instance.player.transform;
    }


    protected override void Update()
    {
        base.Update();
        StateTimer_Magician -= Time.deltaTime ;
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

            if (snapToGround)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    spawnPos + Vector2.up * groundRayHeight,
                    Vector2.down, groundRayLength, whatIsGround);

                if (hit.collider != null)
                    spawnPos.y = hit.point.y;
            }

            GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
            spike.GetComponent<Collider>().Initialize(this);

            if (spikeLifetime > 0f)
                Destroy(spike, spikeLifetime);

            if (spikeInterval > 0f)
                yield return new WaitForSeconds(spikeInterval);
        }
    }
    IEnumerator Spike2Coroutine() 
    {
        Vector2 spawnPos = player.position;

        if (snapToGround)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                spawnPos + Vector2.up * groundRayHeight,
                Vector2.down, groundRayLength, whatIsGround);

            if (hit.collider != null)
                spawnPos.y = hit.point.y;
        }

        GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
        spike.GetComponent<Collider>().Initialize(this);

        if (spikeLifetime > 0f)
            Destroy(spike, spikeLifetime);

        yield return null;

    }

    public void Barrage()
    {
        StartCoroutine(BarrageRoutine());
    }
    IEnumerator BarrageRoutine()
    {
        List<Transform> projectiles = new List<Transform>();
        yield return new WaitForSeconds(0.33f);
        // --- Phase 1: fire one by one ---
        for (int i = 0; i < barrageCount; i++)
        {
            Vector2 fireDir = Quaternion.Euler(0, 0, barrageAngle * facingDir) * new Vector2(facingDir, 0);

            GameObject proj = Instantiate(barragePrefab, barrageFirePoint.position,
                Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, fireDir)));

            Collider ammo = proj.GetComponent<Collider>();
            if (ammo != null)
                ammo.Initialize(this);

            // random target inside the rect, in boss-local space (x mirrored by facing)
            Vector2 localTarget = new Vector2(
                rectCenterOffset.x * facingDir + Random.Range(-rectSize.x * 0.5f, rectSize.x * 0.5f),
                rectCenterOffset.y + Random.Range(-rectSize.y * 0.5f, rectSize.y * 0.5f));

            projectiles.Add(proj.transform);
            StartCoroutine(DriveToFloat(proj.transform, fireDir, localTarget));

            yield return new WaitForSeconds(barrageFireInterval);
        }

        // --- Phase 2/3: wait for the last one to settle, then float ---
        yield return new WaitForSeconds(barrageFireTime + barrageLerpTime);
        yield return new WaitForSeconds(Random.Range(floatWaitMin, floatWaitMax));

        // --- Phase 4: launch one by one ---
        foreach (Transform proj in projectiles)
        {
            if (proj == null) continue; // already hit the player while floating

            StartCoroutine(LaunchAtPlayer(proj));
            yield return new WaitForSeconds(launchInterval);
        }
    }

    IEnumerator DriveToFloat(Transform proj, Vector2 fireDir, Vector2 localTarget)
    {
        // straight flight with the fired rotation/velocity
        float elapsed = 0f;
        while (elapsed < barrageFireTime)
        {
            if (proj == null) yield break;
            proj.position += (Vector3)(fireDir * barrageFireSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // become a child of the boss, then Lerp in LOCAL space
        // so the floating cloud moves (and flips) with the boss
        proj.SetParent(transform);
        Vector3 startLocal = proj.localPosition;
        Quaternion startRot = proj.localRotation;

        elapsed = 0f;
        while (elapsed < barrageLerpTime)
        {
            if (proj == null) yield break;
            elapsed += Time.deltaTime;
            float p = elapsed / barrageLerpTime;
            proj.localPosition = Vector3.Lerp(startLocal, localTarget, p);
            proj.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, p);
            yield return null;
        }
        proj.localPosition = localTarget;
    }

    IEnumerator LaunchAtPlayer(Transform proj)
    {
        proj.SetParent(null);

        Vector2 dir = ((Vector2)player.position - (Vector2)proj.position).normalized;
        proj.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir));

        float elapsed = 0f;
        while (elapsed < barrageLifetime)
        {
            if (proj == null) yield break; // hit something, damage script destroyed it
            proj.position += (Vector3)(dir * launchSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (proj != null)
            Destroy(proj.gameObject);
    }
    public override void Die()
    {
        base.Die();
        //stateMachine.ChangeState(deathState);
    }
}
