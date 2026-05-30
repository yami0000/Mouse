using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerReadyToAttack
{
    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = .2f;
        player.Setvelocity(5*-player.facingDir,player.jumpforce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
 
        
        if (player.IsGroundDetected())
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0) 
            stateMachine.ChangeState(player.airState);
    }


}
