using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pattern 2, step three of three: brake out of the run, swing the axe up and put it away.
/// </summary>
public class HeavyUpstrokeState : EnemyState
{
    protected Enemy_Heavy enemy;

    private float elapsed;
    private bool hasSpawnedHitbox;

    public HeavyUpstrokeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.lastTimeAttacked = Time.time;
        elapsed = 0f;
        hasSpawnedHitbox = false;

        // The levelled axe comes off here, in case the charge state did not get to clean up.
        enemy.DespawnChargeHitbox();

        // Planted for the swing. No TurnToPlayer: a charge that ran past should swing where
        // it ended up, not spin around first.
        enemy.zerovelocity();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.zerovelocity();
    }

    public override void Update()
    {
        base.Update();

        elapsed += Time.deltaTime;

        enemy.zerovelocity();

        // ReadyToAttack comes from an AttackPreparetion animation event, the timer is the
        // fallback so the hitbox still appears on clips with no event on them.
        if (!hasSpawnedHitbox && (ReadyToAttack || elapsed >= enemy.upstrokeHitDelay))
        {
            enemy.SpawnHitbox(enemy.upstrokeHitboxPrefab, enemy.upstrokeHitboxLifeTime, true);
            hasSpawnedHitbox = true;
        }

         
        if (hasSpawnedHitbox && (elapsed >= enemy.upstrokeDuration))
            stateMachine.ChangeState(enemy.battleState);
    }
}
