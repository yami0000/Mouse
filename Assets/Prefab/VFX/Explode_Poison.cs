using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A lingering cloud spawned by an enemy projectile. Unlike Explode_Grenade, which hits
/// once the moment it appears, this keeps hurting the player for as long as they stand in
/// its trigger, ticking every Damage Interval until Duration runs out.
/// </summary>
public class Explode_Poison : MonoBehaviour, IExplosionEffect
{
    [Header("Damage")]
    [Tooltip("Seconds between damage ticks while the player stands in the cloud.")]
    [SerializeField] private float damageInterval = 1f;
    [Tooltip("Multiplier on each tick, so a cloud can chip away instead of hitting for a full shot.")]
    [SerializeField] private float damageMultiplier = 0.4f;
    [Tooltip("Hit the player the moment they walk in, rather than making them wait out the first interval.")]
    [SerializeField] private bool damageOnEnter = true;

    [Header("Lifetime")]
    [Tooltip("How long the cloud lasts before it destroys itself. 0 = let whoever spawned it decide.")]
    [SerializeField] private float duration = 5f;

    private EntityStats ownerStats;

    private readonly List<PlayerStats> targetsInside = new List<PlayerStats>();
    private float lifeTimer;
    private float tickTimer;

    /// <summary>Handed the stats of whoever fired the projectile that spawned this cloud.</summary>
    public void Initialize(EntityStats _ownerStats)
    {
        ownerStats = _ownerStats;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (duration > 0f && lifeTimer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // The clock only runs while there is somebody standing in it, so walking out and
        // back in does not hand out a free tick from a timer that expired while empty.
        if (targetsInside.Count == 0)
            return;

        tickTimer -= Time.deltaTime;

        if (tickTimer > 0f)
            return;

        tickTimer = damageInterval;
        DamageEveryoneInside();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerStats target = GetTargetStats(collision);

        if (target == null)
            return;

        // A second collider on the same target must not count twice.
        if (targetsInside.Contains(target))
            return;

        targetsInside.Add(target);

        if (damageOnEnter)
        {
            DamageTarget(target);
            tickTimer = damageInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerStats target = GetTargetStats(collision);

        if (target == null)
            return;

        targetsInside.Remove(target);
    }

    private void DamageEveryoneInside()
    {
        // Backwards, so a target that dies or is destroyed mid tick can be dropped safely.
        for (int i = targetsInside.Count - 1; i >= 0; i--)
        {
            if (targetsInside[i] == null)
            {
                targetsInside.RemoveAt(i);
                continue;
            }

            DamageTarget(targetsInside[i]);
        }
    }

    private void DamageTarget(PlayerStats _target)
    {
        // Spawned without Initialize, so there is nobody to credit the damage to.
        if (ownerStats == null)
            return;

        ownerStats.DoDamage(_target, damageMultiplier);
    }

    private PlayerStats GetTargetStats(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null)
            return null;

        return collision.GetComponent<PlayerStats>();
    }
}
