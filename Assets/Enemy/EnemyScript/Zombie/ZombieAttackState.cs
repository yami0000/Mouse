using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackState : EnemyState
{
    protected Enemy_Zombie enemy;

    private float attackTimeElapsed;
    private float horizontalSpeed;

    public ZombieAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Zombie enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.lastTimeAttacked = Time.time;
        attackTimeElapsed = 0f;
        enemy.stateTimer_Zombie = enemy.attackDuration;

        // Aim the leap at the player, the battle state no longer turns while close.
        enemy.TurnToPlayer();

        horizontalSpeed = enemy.attackMoveSpeed;

        // Launch. From here gravity owns the vertical motion, so the zombie keeps flying
        // past the player and lands further behind instead of dropping on the spot.
        if (enemy.attackJumpForce > 0f)
            enemy.Setvelocity(rb.velocity.x, enemy.attackJumpForce);

        if (enemy.aimThroughPlayer && enemy.attackDuration > 0f)
            horizontalSpeed = SolveSpeedThroughPlayer();
    }

    public override void Update()
    {
        base.Update();

        attackTimeElapsed += Time.deltaTime;

        float normalizedTime = enemy.attackDuration <= 0f ? 1f : attackTimeElapsed / enemy.attackDuration;

        // The curve drives the horizontal speed for its own duration only. After that the zombie
        // keeps whatever momentum it has left, so the rest of the arc is pure ballistics.
        if (normalizedTime < 1f)
            enemy.Setvelocity(enemy.attackMoveCurve.Evaluate(normalizedTime) * horizontalSpeed * enemy.facingDir, rb.velocity.y);

        // Wait for the landing, so the leap is never cut short in mid air.
        if (enemy.stateTimer_Zombie < 0 && (enemy.IsGroundDetected() || attackTimeElapsed > enemy.attackDuration * 3f))
        {

            stateMachine.ChangeState(enemy.battleState);


        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.Setvelocity(0f, rb.velocity.y);
    }

    /// <summary>
    /// Works out the horizontal speed that puts the player's position on the flight path:
    /// finds when the arc drops back to the player's height, then scales the curve so the
    /// zombie has covered exactly the distance to the player by that moment.
    /// </summary>
    private float SolveSpeedThroughPlayer()
    {
        Transform player = PlayerManager.Instance.player.transform;

        float distanceX = Mathf.Abs(player.position.x - enemy.transform.position.x);
        float gravity = -Physics2D.gravity.y * rb.gravityScale;

        float timeToPlayer;

        if (enemy.attackJumpForce > 0f && gravity > 0f)
        {
            float heightDifference = player.position.y - enemy.transform.position.y;
            float discriminant = (enemy.attackJumpForce * enemy.attackJumpForce) - (2f * gravity * heightDifference);

            // Negative means the player is above the apex, so aim for the apex instead.
            timeToPlayer = discriminant > 0f
                ? (enemy.attackJumpForce + Mathf.Sqrt(discriminant)) / gravity
                : enemy.attackJumpForce / gravity;
        }
        else
        {
            timeToPlayer = enemy.attackDuration;
        }

        // Distance covered per unit of speed: the curve while it lasts, then constant momentum.
        float curvedTime = Mathf.Min(timeToPlayer, enemy.attackDuration);
        float travelPerSpeed = enemy.attackDuration * CurveArea(curvedTime / enemy.attackDuration);

        if (timeToPlayer > enemy.attackDuration)
            travelPerSpeed += enemy.attackMoveCurve.Evaluate(1f) * (timeToPlayer - enemy.attackDuration);

        if (travelPerSpeed < 0.0001f)
            return enemy.attackMoveSpeed;

        return Mathf.Min(distanceX / travelPerSpeed, enemy.attackMoveSpeed);
    }

    /// <summary>Area under the speed curve from 0 to _upperTime, both in normalized time.</summary>
    private float CurveArea(float _upperTime)
    {
        const int samples = 32;
        float area = 0f;

        for (int i = 0; i < samples; i++)
            area += enemy.attackMoveCurve.Evaluate(_upperTime * (i + 0.5f) / samples);

        return (area / samples) * _upperTime;
    }
}
