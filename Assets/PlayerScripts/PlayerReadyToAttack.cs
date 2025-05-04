using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerReadyToAttack : PlayerState
{
    public PlayerReadyToAttack(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKey(KeyCode.Mouse0) && PlayerWeaponHolder.Instance.previousWeapon == null)
            stateMachine.ChangeState(player.attackState);

        if (Input.GetKey(KeyCode.Mouse0) && PlayerWeaponHolder.Instance.previousWeapon != null)
        { 
            ItemData_Equipment weaponData = Inventory.Instance.GetEquipment(EquipmentType.Weapon);
            Transform player = PlayerManager.Instance.player.transform;

            if(weaponData != null ) 
            weaponData.ExecuteItemEffect(player.transform);


        }
    }
}
