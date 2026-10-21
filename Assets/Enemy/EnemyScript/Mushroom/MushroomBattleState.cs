using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomBattleState : EnemyState
{
    protected Enemy_Mushroom enemy;
    private Transform player;

    private bool isRetreating;
    private float retreatStartX;
    private float retreatRunDistance;

    public MushroomBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Mushroom enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.Instance.player.transform;
        isRetreating = false;

        // base.Enter has just raised the move bool, so start in step with it.
        isMoving = true;
    }

    public override void Exit()
    {
        base.Exit();

        isRetreating = false;
        ClearMoveAnimBools();
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        // Player got too close, so hop away a short random distance. This is a distance run,
        // not a separation to reach, so a player who keeps chasing gets a string of short
        // hops with a stand-and-fight gap between them rather than one long flight.
        if (!isRetreating && enemy.retreatCooldownTimer <= 0f && distanceToPlayer < enemy.retreatTriggerDistance)
        {
            isRetreating = true;
            retreatStartX = enemy.transform.position.x;
            retreatRunDistance = Random.Range(enemy.retreatDistanceMin, enemy.retreatDistanceMax);
        }

        if (isRetreating)
        {
            // Face away first, the ground and wall checks read whichever way the mushroom is facing.
            enemy.TurnAwayFromPlayer();

            bool hopFinished = Mathf.Abs(enemy.transform.position.x - retreatStartX) >= retreatRunDistance;
            bool nowhereToRun = enemy.IsWallDetected() || !enemy.IsGroundDetected();

            // Either way it stops here and the gap timer starts, so the mushroom always gets
            // a window to throw before it considers backing off again.
            if (hopFinished || nowhereToRun)
            {
                isRetreating = false;
                enemy.retreatCooldownTimer = enemy.retreatRetryTime;
                StandStill();
            }
            else
            {
                MoveAt(enemy.moveSpeed * enemy.retreatSpeedMultiplier);
                return;
            }
        }

        enemy.TurnToPlayer();

        if (distanceToPlayer < enemy.attackDistance && enemy.CanAttack())
        {
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        // Player got away, go back to patrolling. Same 360 degree check the patrol states use,
        // so a mushroom in battle does not lose a player it can plainly see behind it.
        if (!enemy.IsPlayerNoticed() && distanceToPlayer > enemy.loseTargetDistance)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        // Close in, but never walk off a ledge or into a wall to do it.
        if (distanceToPlayer > enemy.battleStopDistance && enemy.IsGroundDetected() && !enemy.IsWallDetected())
            MoveAt(enemy.moveSpeed);
        else
            StandStill();
    }
}
