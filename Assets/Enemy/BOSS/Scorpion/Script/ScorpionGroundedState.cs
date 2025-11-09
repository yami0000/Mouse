using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionGroundedState : EnemyState
{
    protected Boss_Scorpion boss;
    protected Transform player;
    protected bool R;
    private float P;
    private float P1;
    private float E;
    private int moveDir;
    public ScorpionGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Boss_Scorpion boss) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = boss;

    }

    public override void Enter()
    {
        base.Enter();
        P = Random.Range(0f, 1f);
        P1 = Random.Range(0f, 1f);
        E = Random.Range(0f, 1f);
        player = PlayerManager.Instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        boss.TurnToPlayer();



        if (boss.DetectAmmo() == true && boss.canBlock >= 2 )
        { 
            boss.canBlock = 0;

            if (boss.Energy <= 25 && E <= 0.9)
                Block();
            else if (boss.Energy <= 50 && E <= 0.65)
                Block();
            else if (boss.Energy <= 75 && E <= 0.5)
                Block();
            

           
        }

      


        if (boss.CanAttack() && boss.IsPlayerDetected())
            {   boss.canBlock +=1 ;

            if (PlayerManager.Instance.player.IsGroundDetected() && Vector2.Distance(boss.transform.position, GetPlayerPosition()) < boss.swingdistance)
            {

                stateMachine.ChangeState(boss.swingState);

            }

            else if (Vector2.Distance(boss.transform.position, GetPlayerPosition()) > boss.distanceToDash)
            {
                    if (P1 <= 0.5f)
                        stateMachine.ChangeState(boss.dashState);
                    else
                    {
                        if (P < 0.35f)
                            stateMachine.ChangeState(boss.laserState);
                        else if (P < 0.8f)
                            stateMachine.ChangeState(boss.shotState);
                        else
                            stateMachine.ChangeState(boss.combo1State);
                    }

            }
            else
            {
                    if (P < 0.35f)
                        stateMachine.ChangeState(boss.laserState);
                    else if (P < 0.8f)
                        stateMachine.ChangeState(boss.shotState);
                    else
                        stateMachine.ChangeState(boss.combo1State);


            }

            }


    
    }

    private void Block()
    {
        boss.wait();
        stateMachine.ChangeState(boss.blockState);
    }

    public static Vector3 GetPlayerPosition()
    {
        return PlayerManager.Instance.player.transform.position;
    }
}
