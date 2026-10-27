using UnityEngine;

/// <summary>
/// Shows a string field as a dropdown of the states of the object being edited (see StateLookup),
/// e.g. Idle State, Battle State, Death State.
/// </summary>
public class StateNameAttribute : PropertyAttribute { }

/// <summary>
/// One effect an Entity spawns when it enters a state: Delay seconds after entering, at Spawn Point,
/// gone after Lifetime seconds, after one loop of the effect, or when the state ends.
/// Several can target the same state.
/// </summary>
[System.Serializable]
public class StateFX
{
    public enum Lifetime
    {
        Seconds,
        OneLoop,
        UntilStateEnds,
    }

    // Ticked off with the box in the entry's header. Stored as "off" so existing entries stay on.
    [HideInInspector] public bool disabled;

    [StateName]
    [Tooltip("The state that spawns the effect, each time it is entered.")]
    public string state;

    [Tooltip("The effect prefab: particles, an animated sprite, anything.")]
    public GameObject fxPrefab;

    [Tooltip("Where the effect appears, with its rotation (so it turns with the entity). Empty = the entity itself.")]
    public Transform spawnPoint;

    [Tooltip("Moves along with the Spawn Point. Off = stays where it appeared.")]
    public bool followSpawnPoint;

    [Tooltip("Seconds after entering the state. If the state ends before then, nothing spawns.")]
    [Min(0)] public float delay;

    [Tooltip("Seconds = gone after Seconds.\n" +
             "One Loop = gone once its animation / particles have played through once.\n" +
             "Until State Ends = gone when the entity leaves the state.")]
    public Lifetime lifetime = Lifetime.Seconds;

    [Tooltip("Seconds until it is destroyed.")]
    [Min(0)] public float seconds = 1f;

    /// <summary>Spawns the effect at the spawn point (or _fallback) and schedules its end unless it waits for the state.</summary>
    public GameObject Spawn(Transform _fallback)
    {
        if (disabled || fxPrefab == null) return null;

        Transform point = spawnPoint != null ? spawnPoint : _fallback;
        GameObject fx = Object.Instantiate(fxPrefab, point.position, point.rotation, followSpawnPoint ? point : null);

        if (lifetime == Lifetime.Seconds)
            Object.Destroy(fx, seconds);
        else if (lifetime == Lifetime.OneLoop)
            Object.Destroy(fx, OneLoopLength(fx, seconds));

        return fx;
    }

    /// <summary>
    /// How long the effect takes to play through once: its Animator's first state, or its particles'
    /// duration plus their lifetime so the last ones fade out. Falls back to _fallback.
    /// Public so other one-shot effects (weapon fire FX) time themselves the same way.
    /// </summary>
    public static float OneLoopLength(GameObject _fx, float _fallback)
    {
        float length = 0f;

        Animator animator = _fx.GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Not evaluated until its first frame, so step it once to read the state it starts in.
            animator.Update(0f);
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            length = Mathf.Max(length, info.length / Mathf.Max(0.0001f, Mathf.Abs(animator.speed)));
        }

        foreach (ParticleSystem particles in _fx.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = particles.main;
            length = Mathf.Max(length, main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);
        }

        return length > 0f ? length : _fallback;
    }
}
