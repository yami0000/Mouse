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
        if (Input.GetKey(KeyCode.Mouse0) && !PlayerWeaponHolder.Instance.isHolding)
            stateMachine.ChangeState(player.attackState);//ÆÕÍ¨¹¥»÷£¨Åç»ð£©

        if (PlayerWeaponHolder.Instance.isHolding)
        {
            ItemData_Equipment weaponData = Inventory.Instance.GetEquipment(EquipmentType.Weapon);
            if (weaponData == null) return;

            Transform playerTransform = PlayerManager.Instance.player.transform;


            if (weaponData.mechanism == Mechanism.common)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    weaponData.ExecuteEquipmentEffect(playerTransform, weaponData);
                }
            }


            else if (weaponData.mechanism == Mechanism.charge)
            { if (PlayerManager.Instance.player.Timer <= 0)
                {
                    if (Input.GetKey(KeyCode.Mouse0))

                    {

                        PlayerManager.Instance.player.currentChargeTimer += Time.deltaTime;



                    }

                    if (Input.GetKeyUp(KeyCode.Mouse0))
                    {


                        weaponData.ExecuteEquipmentEffect(playerTransform, weaponData);

                        
                    }
                }
            }
        }
    }
}
