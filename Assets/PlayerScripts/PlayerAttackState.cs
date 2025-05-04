using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerState

    {

    private int comboCounter;

    private float lastTimeAttacked;
    private float comboWindow = 1;
    public PlayerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        xInput = 0;

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
            comboCounter = 0;

        player.anim.SetInteger("ComboCounter", comboCounter);
        // player.anim.speed = 1.5f;

          float attackDir = player.facingDir;
          if (xInput != 0) 
            attackDir = xInput; 

           

       

        player.Setvelocity(0.0000001f * attackDir , rb.velocity.y  );




        player.isBackingup = true;

    
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", .1f);

        comboCounter++;
        lastTimeAttacked = Time.time;

        player.isBackingup = false;

       // player.anim.speed = 1;

    }

    public override void Update()
    {
        base.Update();



        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
            stateMachine.ChangeState(player.jumpState);





        if (Keyframe)
        {
            player.Setvelocity(player.attackMovement[comboCounter] * player.facingDir, rb.velocity.y);
            Keyframe = false;
            return;
        }

       // if (stateTimer < 0)
           // rb.velocity = new Vector2(0, 0);

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
