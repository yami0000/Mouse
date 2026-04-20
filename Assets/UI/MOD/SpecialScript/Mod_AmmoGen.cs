using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Mod", menuName = "Data/Mod/Mod_AmmoGen")]
public class Mod_AmmoGen : ItemEffect 
{
    private int shotCount = 0;
    [SerializeField]private int shotCountToTrigger;
   

    public ItemEffect[] equipmentEffects;
    public override void ExecuteModEffect(Transform _position,ItemData_Equipment equip)
    {
        base.ExecuteModEffect(_position,equip);
        shotCount++;

        if (shotCount >= shotCountToTrigger)
        {
           foreach (var item in equipmentEffects) 
                
            {
                item.ExecuteWeaponEffect(_position,equip);
                 
            }
            shotCount = 0;
        }

    }


}
