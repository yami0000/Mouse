using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShieldState :PlayerState
{
    public PlayerShieldState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Timer = 0.8f;
        player.shield();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (player.Timer <= 0)
            stateMachine.ChangeState(player.idleState);
    }
}
