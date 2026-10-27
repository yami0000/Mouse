using UnityEngine;

/// <summary>
/// Base for anything an item does. Override the hook that fits: ExecuteEffect for a used item,
/// ExecuteWeaponEffect for a weapon shot, ExecuteModEffect for a special mod.
/// Its subclasses are created from Assets > Create > Data Asset... (see Editor/DataAssetMenu.cs).
/// </summary>
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
