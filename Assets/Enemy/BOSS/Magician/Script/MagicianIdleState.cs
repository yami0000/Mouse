using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianIdleState : EnemyState
{
 protected Boss_Magician boss;
    public MagicianIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boss_Magician boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;
    }
    public override void Enter()
    {
        base.Enter();
        boss.StateTimer_Magician = Random.Range(0.5f,1.5f);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();
        boss.zerovelocity();
        if (boss.StateTimer_Magician <= 0)
        {
            if (!boss.EnterStageTwo)
            {
                if (boss.StageChange())
                {

                    stateMachine.ChangeState(boss.switchState);

                }
                else
                {
                    if (Random.value < 0.5f)
                        stateMachine.ChangeState(boss.walkState);
                    else
                        stateMachine.ChangeState(Random.value < 0.5f ? boss.spikeState : boss.barrageState);
                }
            }
            else
            {
                if (Random.value < 0.35f)
                    stateMachine.ChangeState(boss.walkState);
                else
                {
                    float attackRoll = Random.value;
                    if (attackRoll < 0.333f)
                        stateMachine.ChangeState(boss.spikeState);
                    else if (attackRoll < 0.666f)
                        stateMachine.ChangeState(boss.barrageState);
                    else
                        stateMachine.ChangeState(boss.chopState);
                }
            }
        }

    }
}
