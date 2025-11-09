using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionBlockState : EnemyState
{
    private Boss_Scorpion boss;
    private float originalArmor;
    private bool StatsAdded;
    public ScorpionBlockState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;

    }

    public override void Enter()
    {
        base.Enter();

        StatsAdded = false;
        boss.stateTimer_Scorpion = 2.66f;
        originalArmor = boss.stats.Armor.GetValue();
         
    }


    public override void Update()
    {
        base.Update();

        
        if (boss.stats.absorbedDAMAGE != 0)
        {
           
            boss.Energy += (float)boss.stats.absorbedDAMAGE/3; 
            boss.stats.absorbedDAMAGE = 0;
        }

        if (boss.stateTimer_Scorpion <= 0f)
        {
            boss.stats.Armor.AddModifier(-60);
            stateMachine.ChangeState(boss.idleState);
        }
        else if (!StatsAdded && boss.stateTimer_Scorpion > 0f)
        {
            boss.stats.Armor.AddModifier(60);
            StatsAdded = true;
        }
    }
    public override void Exit()
    {
        base.Exit();
    }
}
