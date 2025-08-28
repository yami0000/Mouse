using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderIdleState : SpiderGroundedState
{
    
    public SpiderIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Spider enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
         
    }

    public override void Enter()
    {
        base.Enter();
        
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();


        

        if (stateTimer < 0)
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.walkState);
        } 
    }


}
  /*  IEnumerator idleSequence()
    {
        float t = 0f;

        while (t < enemy.idleTime)
        {
            t += Time.deltaTime;
            yield return null;
        }
        enemy.Flip();
        stateMachine.ChangeState(enemy.walkState);


    }*/