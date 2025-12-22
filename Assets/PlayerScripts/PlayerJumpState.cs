using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerReadyToAttack
{
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        rb.velocity = new Vector2(rb.velocity.x, player.jumpforce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        

        if (rb.velocity.y < 0)
            stateMachine.ChangeState(player.airState);

        if (player.xInput != 0)
            player.Setvelocity(player.movespeed * 0.8f * player.xInput, rb.velocity.y);

        if (SK.Instance.Skill.DoubleJump)
            if (Input.GetKeyDown(KeyCode.Space) && player.CanDoubleJump)
            {
                player.CanDoubleJump = false;
                stateMachine.ChangeState(player.jumpState);
            }
    }
}
