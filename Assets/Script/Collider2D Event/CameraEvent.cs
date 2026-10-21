using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum CameraEventType
{
    Dialogue,
    EnemyWaves,
    Move,
    Reveal,
    KillEnemies,
    ForceState,
}

public enum EventCameraMode
{
    FollowPlayer,
    ConfineToArea,
    LockOnEvent,
}

public enum SpawnSide
{
    Left,
    Right,
    Random,
}

/// <summary>Something that runs in an ordered list: wait Delay, start, then wait for it unless it runs with the next one.</summary>
public interface ISequenceStep
{
    float Delay { get; }
    bool RunWithNext { get; }
}

[System.Serializable]
public class WaveSpawn
{
    public Enemy enemyPrefab;
    public SpawnSide side = SpawnSide.Random;
    [Min(1)] public int count = 1;
    [Tooltip("Seconds to wait before each enemy of this entry spawns.")]
    [Min(0)] public float delay = 0.5f;
    [Tooltip("Spawn height above the ground. Raise it for flying enemies.")]
    public float heightOffset = 0.5f;
}

[System.Serializable]
public class EnemyWave
{
    [Tooltip("Seconds to wait before this wave starts. For the first wave it counts from the moment the step starts, " +
             "for the others from the moment the last enemy of the previous wave dies.")]
    [Min(0)] public float delayBeforeWave = 1f;
    public List<WaveSpawn> spawns = new List<WaveSpawn>();
}

/// <summary>One part of a CameraEvent: a dialogue, some enemy waves, or something moving.</summary>
[System.Serializable]
public class EventStep : ISequenceStep
{
    [Tooltip("Dialogue = play a Yarn node.\nEnemy Waves = enemies walk in from off camera, wave by wave.\nMove = move an object, e.g. shut a door.\n" +
             "Reveal = fade hidden objects in, e.g. a tutorial sign.\n" +
             "Kill Enemies = wait until enemies already in the scene are all dead.\n" +
             "Force State = put enemies / NPCs into a chosen state.")]
    public CameraEventType type;

    [Tooltip("Seconds to wait before this step starts, counted from when the previous step finished " +
             "(or started, if that one runs with this one).")]
    [Min(0)] public float delay;

    [Tooltip("Start the next step right away instead of waiting for this one to finish, so both happen at the same time.")]
    public bool runWithNext;

    [Tooltip("Yarn node to play.")]
    public string dialogueName;

    public List<EnemyWave> waves = new List<EnemyWave>();

    public MoveAction move = new MoveAction();

    public RevealAction reveal = new RevealAction();

    [Tooltip("Enemies in the scene to kill. The step finishes once every one of them is dead (or gone).")]
    public List<Enemy> killTargets = new List<Enemy>();

    public ForceStateAction forceState = new ForceStateAction();

    public float Delay => delay;
    public bool RunWithNext => runWithNext;
}

// Runs after the player's scripts, so the area clamp in FixedUpdate has the last word on velocity.
[DefaultExecutionOrder(100)]
public class CameraEvent : DETECTION, ISerializationCallbackReceiver
{
    [Tooltip("What the camera does from when the player walks in until the event is completed.\n" +
             "Follow Player = nothing changes.\n" +
             "Confine To Area = keeps following the player, but can't show past this collider. " +
             "If the collider is smaller than the view, the camera sits still, centred on it.\n" +
             "Lock On Event = fixed on this object (Camera Follow Player in the consequences frees it).")]
    [SerializeField] private EventCameraMode cameraMode = EventCameraMode.ConfineToArea;

    [Tooltip("Seconds the camera takes to ease into the area and back out, instead of snapping.")]
    [Min(0)] [SerializeField] private float confineBlend = 0.5f;

    [Tooltip("Keep the player inside this collider until the event is completed.")]
    [SerializeField] private bool restrictPlayer = false;

    [Tooltip("What happens when the player walks in, in order. The event is completed once every step has finished.")]
    [SerializeField] private List<EventStep> steps = new List<EventStep>();

    // ---- Enemy wave settings, shared by every Enemy Waves step ----
    [Tooltip("How far outside the collider's left / right edge enemies spawn.")]
    [FormerlySerializedAs("offscreenMargin")]
    [SerializeField] private float spawnMargin = 1.5f;

    [Tooltip("How far inside the collider's edge enemies walk before their own AI takes over.")]
    [SerializeField] private float walkInDistance = 3f;

    [Tooltip("Walk in speed. 0 = each enemy's own Move Speed.")]
    [SerializeField] private float walkInSpeed = 0f;

    [Tooltip("Ground layers. Each spawn drops onto the ground found below the player's height at the spawn X. " +
             "Nothing = spawn level with the player.")]
    [SerializeField] private LayerMask groundLayer;

    // ---- Consequences ----
    [Tooltip("What happens once the event is completed, in order.")]
    [SerializeField] private List<ConsequenceStep> consequences = new List<ConsequenceStep>();

    // Old data, folded into steps / consequences on load (see OnAfterDeserialize).
    // lockCamera: true (also what events saved before it existed load as) = the old camera lock, now Confine To Area;
    // false = Follow Player. Reset to true once read, so it never overrides Camera Mode again.
    [SerializeField, HideInInspector] private bool lockCamera = true;
    [SerializeField, HideInInspector] private CameraEventType eventType = CameraEventType.Dialogue;
    [SerializeField, HideInInspector] private string DialogueName;
    [SerializeField, HideInInspector] private float t;
    [SerializeField, HideInInspector] private List<EnemyWave> waves = new List<EnemyWave>();
    [SerializeField, HideInInspector] private EventAction onComplete = new EventAction();
    [SerializeField, HideInInspector] private List<LegacyAction> actions = new List<LegacyAction>();
    [SerializeField, HideInInspector] private Entity E;

    [System.Serializable]
    private class LegacyAction
    {
        public int actionType = -1;
    }

    // Long enough to cross any screen, short enough that an enemy stuck on a wall still joins in.
    private const float WalkInTimeout = 8f;

    public bool IsCompleted { get; private set; }
    public event System.Action Completed;

    private bool _hasTriggered = false;
    private Collider2D cd;
    private bool restricting;
    private bool confining;
    private Bounds area;
    private int exitCount;

    // The collider stays on after triggering (Reveal steps watch for the player leaving it), so the
    // E prompt would hang over the player the whole time they are inside.
    protected override bool ShowPrompt => false;

    private void Start()
    {
        cd = GetComponent<Collider2D>();

        // Reveal targets start hidden, unless revealed for good earlier this session.
        foreach (EventStep step in steps)
            if (step != null && step.type == CameraEventType.Reveal)
                step.reveal?.SetStartState();
    }

    public override void Event()
    {
        if (_hasTriggered) return;
        _hasTriggered = true;

        base.Event();

        area = cd.bounds;
        restricting = restrictPlayer;

        switch (cameraMode)
        {
            case EventCameraMode.ConfineToArea:
                CameraSetter.Instance.ConfineTo(area, confineBlend);
                confining = true;
                break;

            case EventCameraMode.LockOnEvent:
                CameraSetter.Instance.EnablePosition(this.transform);
                break;
        }

        StartCoroutine(Run());
    }

    protected override void OnPlayerExit() => exitCount++;

    // The event itself only runs once, but Reveal steps that have run can show again on the way back in.
    // Steps not reached yet are left alone, so walking out and in early doesn't skip ahead.
    protected override void OnPlayerEnter()
    {
        if (!_hasTriggered) return;

        foreach (EventStep step in steps)
        {
            RevealAction reveal = step?.reveal;

            if (step != null && step.type == CameraEventType.Reveal && reveal != null &&
                reveal.showAgainOnReenter && !reveal.permanent && reveal.HasRun)
                StartCoroutine(RunReveal(reveal));
        }
    }

    private IEnumerator RunReveal(RevealAction _reveal)
    {
        int exitsAtStart = exitCount;
        yield return _reveal.Run(this, () => exitCount != exitsAtStart);
    }

    private IEnumerator Run()
    {
        yield return RunSequence(steps, RunStep);

        IsCompleted = true;
        restricting = false;

        if (confining)
        {
            CameraSetter.Instance.ResetConfiner(confineBlend);
            confining = false;
        }

        Completed?.Invoke();

        yield return RunSequence(consequences, RunConsequence);
    }

    #region Sequence

    /// <summary>
    /// Runs the entries in order. Each waits its Delay, then starts; the next one waits for it to finish
    /// unless it is set to Run With Next. Returns once every entry has finished, the parallel ones included.
    /// </summary>
    private IEnumerator RunSequence<T>(List<T> _entries, System.Func<T, IEnumerator> _run) where T : class, ISequenceStep
    {
        int running = 0;

        foreach (T entry in _entries)
        {
            if (entry == null) continue;

            if (entry.Delay > 0)
                yield return new WaitForSeconds(entry.Delay);

            if (entry.RunWithNext)
            {
                running++;
                StartCoroutine(RunThen(_run(entry), () => running--));
            }
            else
            {
                yield return _run(entry);
            }
        }

        while (running > 0)
            yield return null;
    }

    private IEnumerator RunThen(IEnumerator _routine, System.Action _done)
    {
        yield return _routine;
        _done();
    }

    private IEnumerator RunStep(EventStep _step)
    {
        switch (_step.type)
        {
            case CameraEventType.Dialogue:
                yield return PlayDialogue(_step.dialogueName);
                break;

            case CameraEventType.EnemyWaves:
                yield return RunWaves(_step.waves);
                break;

            case CameraEventType.Move:
                if (_step.move != null)
                    yield return _step.move.Run();
                break;

            case CameraEventType.Reveal:
                if (_step.reveal != null)
                    yield return RunReveal(_step.reveal);
                break;

            case CameraEventType.KillEnemies:
                // Destroyed counts as dead, so an enemy removed some other way can't stall the event.
                if (_step.killTargets != null)
                    yield return new WaitUntil(() => _step.killTargets.TrueForAll(e => e == null || e.IsDead));
                break;

            case CameraEventType.ForceState:
                if (_step.forceState != null)
                    yield return _step.forceState.Run();
                break;
        }
    }

    private IEnumerator RunConsequence(ConsequenceStep _step)
    {
        _step.Execute(null, transform);

        while (_step.IsBusy)
            yield return null;
    }

    private IEnumerator PlayDialogue(string _node)
    {
        // An empty node would never report back and stall the whole event.
        if (string.IsNullOrEmpty(_node)) yield break;

        bool finished = false;
        NarrativeManager.Instance.RequestDialogue(_node, 0f, () => finished = true);

        while (!finished)
            yield return null;
    }

    #endregion

    #region Area restriction

    private void FixedUpdate()
    {
        if (!restricting) return;

        Player player = PlayerManager.Instance.player;
        Rigidbody2D rb = player.rb;
        if (rb == null) return;

        float halfWidth = player.cd != null ? player.cd.bounds.extents.x : 0f;
        float min = area.min.x + halfWidth;
        float max = area.max.x - halfWidth;

        Vector2 pos = rb.position;

        if (pos.x < min || pos.x > max)
        {
            pos.x = Mathf.Clamp(pos.x, min, max);
            rb.position = pos;
        }

        // Trim the velocity so this physics step ends on the edge instead of past it.
        Vector2 velocity = rb.velocity;
        float next = pos.x + velocity.x * Time.fixedDeltaTime;

        if (next < min || next > max)
        {
            velocity.x = (Mathf.Clamp(next, min, max) - pos.x) / Time.fixedDeltaTime;
            rb.velocity = velocity;
        }
    }

    #endregion

    #region Enemy waves

    private IEnumerator RunWaves(List<EnemyWave> _waves)
    {
        var alive = new List<Enemy>();

        foreach (var wave in _waves)
        {
            if (wave == null) continue;

            if (wave.delayBeforeWave > 0)
                yield return new WaitForSeconds(wave.delayBeforeWave);

            alive.Clear();

            foreach (var spawn in wave.spawns)
            {
                if (spawn == null || spawn.enemyPrefab == null) continue;

                for (int i = 0; i < spawn.count; i++)
                {
                    if (spawn.delay > 0)
                        yield return new WaitForSeconds(spawn.delay);

                    alive.Add(Spawn(spawn));
                }
            }

            // Destroyed counts as dead, so an enemy removed some other way can't stall the event.
            yield return new WaitUntil(() => alive.TrueForAll(e => e == null || e.IsDead));
        }
    }

    private Enemy Spawn(WaveSpawn spawn)
    {
        bool fromLeft = spawn.side == SpawnSide.Left ||
                        (spawn.side == SpawnSide.Random && Random.value < 0.5f);

        // Just outside the collider's left / right edge (area is cached in Event, as the collider is off by now).
        float x = fromLeft ? area.min.x - spawnMargin : area.max.x + spawnMargin;
        float y = FindGroundY(x) + spawn.heightOffset;

        Enemy enemy = Instantiate(spawn.enemyPrefab, new Vector3(x, y, transform.position.z), spawn.enemyPrefab.transform.rotation);

        float targetX = fromLeft ? area.min.x + walkInDistance : area.max.x - walkInDistance;
        enemy.WalkIn(targetX, walkInSpeed, WalkInTimeout);

        return enemy;
    }

    public bool HasStep(CameraEventType _type) => steps.Exists(s => s != null && s.type == _type);

    // Marks where enemies spawn (red) and where they stop walking in (yellow) while selected.
    private void OnDrawGizmosSelected()
    {
        if (!HasStep(CameraEventType.EnemyWaves)) return;

        Collider2D box = GetComponent<Collider2D>();
        if (box == null) return;

        Bounds b = Application.isPlaying && _hasTriggered ? area : box.bounds;
        float y = b.center.y;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(b.min.x - spawnMargin, y), 0.4f);
        Gizmos.DrawWireSphere(new Vector3(b.max.x + spawnMargin, y), 0.4f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(b.min.x + walkInDistance, b.min.y), new Vector3(b.min.x + walkInDistance, b.max.y));
        Gizmos.DrawLine(new Vector3(b.max.x - walkInDistance, b.min.y), new Vector3(b.max.x - walkInDistance, b.max.y));
    }

    private float FindGroundY(float x)
    {
        float playerY = PlayerManager.Instance.player.transform.position.y;

        if (groundLayer.value == 0)
            return playerY;

        // Cast from a little above the player so platforms and ceilings higher up are skipped.
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, playerY + 2f), Vector2.down, 30f, groundLayer);
        return hit ? hit.point.y : playerY;
    }

    #endregion

    #region Serialization

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        steps ??= new List<EventStep>();
        consequences ??= new List<ConsequenceStep>();
        onComplete ??= new EventAction();

        if (!lockCamera)
        {
            cameraMode = EventCameraMode.FollowPlayer;
            lockCamera = true;
        }

        // Oldest format: ActionType list (CameraFollowPlayer = 0, EnterBattleMode = 1, InScriptDeath = 2,
        // which are bits 0, 1 and 2 of Consequences) plus an E target for Enter Battle Mode.
        if (actions != null && actions.Count > 0)
        {
            foreach (var action in actions)
                if (action != null && action.actionType >= 0)
                    onComplete.consequences |= (Consequences)(1 << action.actionType);

            actions.Clear();
        }

        if (!ReferenceEquals(E, null))
        {
            if (ReferenceEquals(onComplete.battleTarget, null))
                onComplete.battleTarget = E as Enemy;

            E = null;
        }

        // Single event type with its own fields: becomes the first step.
        if (steps.Count == 0)
        {
            if (eventType == CameraEventType.Dialogue && !string.IsNullOrEmpty(DialogueName))
                steps.Add(new EventStep { type = CameraEventType.Dialogue, dialogueName = DialogueName, delay = t });
            else if (eventType == CameraEventType.EnemyWaves && waves != null && waves.Count > 0)
                steps.Add(new EventStep { type = CameraEventType.EnemyWaves, waves = new List<EnemyWave>(waves) });
        }

        eventType = CameraEventType.Dialogue;
        DialogueName = null;
        t = 0f;
        waves?.Clear();

        // Single set of consequences: becomes the first entry.
        if (onComplete.consequences != Consequences.None)
        {
            consequences.Insert(0, new ConsequenceStep(onComplete));
            onComplete = new EventAction();
        }
    }

    #endregion
}
