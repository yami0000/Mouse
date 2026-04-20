using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class PlayerReadyToAttack : PlayerState
{
    private bool charging;
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
            stateMachine.ChangeState(player.attackState);//∆’Õ®π•ª˜£®≈Áª£©

        if (PlayerWeaponHolder.Instance.isHolding)
        {
            ItemData_Equipment weaponData = Inventory.Instance.GetEquipment(EquipmentType.Weapon);
            if (weaponData == null) return;

            Transform playerTransform = GetPlayer().transform;


            if (weaponData.mechanism == Mechanism.common)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (GetPlayer().Timer > 0) return;
                    weaponData.ExecuteEquipmentEffect(playerTransform, weaponData);
                    _TriggerModEffect(weaponData);
                }
            }//∆’Õ®Œ‰∆˜


            else if (weaponData.mechanism == Mechanism.charge)
            {
                if (PlayerManager.Instance.player.Timer <= 0)


                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                         
                        GetPlayer().ChargingAni();
                    }
                   
                    if (Input.GetKey(KeyCode.Mouse0))

                    {

                        PlayerManager.Instance.player.currentChargeTimer += Time.deltaTime;

                        

                    }

                    if (Input.GetKeyUp(KeyCode.Mouse0)&&GetPlayer()._ChargingAni != null)
                    {
 
                        weaponData.ExecuteEquipmentEffect(playerTransform, weaponData);

                        _TriggerModEffect(weaponData);

                        GetPlayer().DestroyChargingAni();

                        charging= false;
                    }
                }
            }//≥‰ƒ‹Œ‰∆˜


        }
    }

    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

    public void _TriggerModEffect(ItemData_Equipment equip)
    {
        if (equip.Mods != null && equip.Mods.Any(m => m?.type == Type.special))
        {
            // At least one mod is "Special", trigger the effects
            foreach (var mod in equip.Mods)
            {
                if (mod?.type == Type.special)
                {
                    mod.ApplyModEffect(equip);
                }
            }
        } 

    }
}
