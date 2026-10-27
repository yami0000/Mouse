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
        GetPlayer().lineRenderer.enabled = false;
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKey(KeyCode.Mouse0) && !PlayerWeaponHolder.Instance.isHolding)
            stateMachine.ChangeState(player.attackState);//普通攻击（喷火）

        ItemData_Equipment weaponData = PlayerWeaponHolder.Instance.isHolding
            ? Inventory.Instance.GetEquipment(EquipmentType.Weapon)
            : null;

        // No early returns in here: the throwable input below has to run every frame too.
        if (weaponData != null)
        {
            if (weaponData.mechanism == Mechanism.common)
            {
                if (Input.GetKey(KeyCode.Mouse0) && GetPlayer().Timer <= 0)
                    FireWeapon(weaponData);
            }//普通武器


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
                        FireWeapon(weaponData);

                        GetPlayer().DestroyChargingAni();

                        charging= false;
                    }
                }
            } 


        }//有武器

        if (Input.GetKey(KeyCode.Q))
        {
            if (Inventory.Instance.GetEquipment(EquipmentType.Throwable) != null)
            {
                Vector2 aimDir = GameManager.Instance.GetMouse(false);  
                GetPlayer().DrawProjection(aimDir);
            }
            
            

        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            ItemData_Equipment E = Inventory.Instance.GetEquipment(EquipmentType.Throwable);
            GetPlayer().lineRenderer.enabled = false;
            Vector2 aimDir = GameManager.Instance.GetMouse(false);
            GetPlayer().FireInAHole(aimDir, E);
            Inventory.Instance.RemoveEquipItem(E);

        }
    }

    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

    // Everything that happens when a weapon goes off: the shot, special mods, fire FX on the weapon,
    // recoil, and the wait until the next shot.
    private void FireWeapon(ItemData_Equipment weaponData)
    {
        Player player = GetPlayer();
        Vector2 aim = GameManager.Instance.GetMouse(true);

        weaponData.FireAmmo(weaponData.ammoPrefab, PlayerWeaponHolder.Instance.WeaponTransform);   // straight shot from the muzzle, if the weapon has one
        weaponData.ExecuteEquipmentEffect(player.transform);            // spread, arc and other special firing
        _TriggerModEffect(weaponData);

        PlayerWeaponHolder.Instance.PlayFireFX(weaponData);
        player.ApplyRecoil(aim, weaponData.recoil);

        // One fire rate for every weapon, however it shoots. Reaper shortens the gap while active.
        bool reaper = SK.Instance.Skill.EquippedSkill == "Reaper" && player.UsingSkill;
        player.Timer = reaper ? weaponData.firingRate / 1.4f : weaponData.firingRate;//如果技能发动，子弹发射时间间隙/1.4
    }

    public void _TriggerModEffect(ItemData_Equipment equip)
    {
        if (equip.Mods != null && equip.Mods.Any(m => m?.type == _Type.special))
        {
            // At least one mod is "Special", trigger the effects
            foreach (var mod in equip.Mods)
            {
                if (mod?.type == _Type.special)
                {
                    mod.ApplyModEffect(equip);
                }
            }
        } 

    }
}
