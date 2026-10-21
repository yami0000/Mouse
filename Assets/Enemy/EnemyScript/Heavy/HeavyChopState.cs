using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Pattern 1, all of it: plant the feet and bring the axe down.</summary>
public class HeavyChopState : EnemyState
{
    protected Enemy_Heavy enemy;

    private float elapsed;
    private bool hasSpawnedHitbox;

    public HeavyChopState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Heavy enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.lastTimeAttacked = Time.time;
        elapsed = 0f;
        hasSpawnedHitbox = false;

        // Aim before the swing, then stay put for the whole animation.
        enemy.TurnToPlayer();
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
        if (!hasSpawnedHitbox && (ReadyToAttack || elapsed >= enemy.chopHitDelay))
        {
            enemy.SpawnHitbox(enemy.chopHitboxPrefab, enemy.chopHitboxLifeTime, true);
            hasSpawnedHitbox = true;
        }

        // Likewise triggerCalled comes from an AnimationTrigger event at the end of the clip.
        if (hasSpawnedHitbox && (triggerCalled || elapsed >= enemy.chopDuration))
            stateMachine.ChangeState(enemy.battleState);
    }
}
