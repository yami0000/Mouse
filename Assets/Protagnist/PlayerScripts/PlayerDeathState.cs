using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }

    public override void Enter()
    {
        base.Enter();

        if (player.inScriptedDefeat)
        {

            UI_Active UI = player.ReviveUI.GetComponent<UI_Active>();

            if (!player.inScriptedDefeat)
                UI.OpenMenu();

            player.NotifyScriptedDefeat();
            return;
        }

        GM.Instance.GameManager.isUIOpened = true;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        player.zerovelocity();
    }
}
