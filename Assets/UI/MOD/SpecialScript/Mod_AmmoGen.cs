using UnityEngine;


// Every Shot Count To Trigger shots, fires an extra bullet (and any extra effects) from the modded weapon.
public class Mod_AmmoGen : ItemEffect
{
    private int shotCount = 0;
    [SerializeField]private int shotCountToTrigger;

    [Tooltip("The extra bullet, fired from the weapon's muzzle along the aim with its X Velocity and Effective Time.")]
    [SerializeField] private GameObject ammoPrefab;

    [Tooltip("Extra effects fired alongside it, e.g. a Shotgun spread.")]
    public ItemEffect[] equipmentEffects;
    public override void ExecuteModEffect(Transform _position,ItemData_Equipment equip)
    {
        base.ExecuteModEffect(_position,equip);
        shotCount++;

        if (shotCount >= shotCountToTrigger)
        {
            equip.FireAmmo(ammoPrefab, PlayerWeaponHolder.Instance.WeaponTransform);

            foreach (var item in equipmentEffects)
                item.ExecuteWeaponEffect(_position,equip);

            shotCount = 0;
        }

    }


}
