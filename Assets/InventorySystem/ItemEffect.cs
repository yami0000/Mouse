using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Name", menuName = "Data/Item Effect")]
public class ItemEffect : ScriptableObject
{
    public virtual void ExecuteEffect(Transform _position)
    {

    }

    public virtual void ExecuteWeaponEffect(Transform _position,ItemData_Equipment data) 
    {
    
    }

    public virtual void ExecuteModEffect(Transform _position,ItemData_Equipment equip) 
    {
    
    }

    
}
