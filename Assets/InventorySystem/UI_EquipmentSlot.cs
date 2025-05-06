using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipmentSlot : UI_ItemSlot
{
 public EquipmentType slotType;

    private void OnValidate()
    {
        gameObject.name = "Equipment Slot -" + slotType.ToString(); 
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
        {
            return;
        }
        ItemData_Equipment equipmentData = item.data as ItemData_Equipment;

        Inventory.Instance.UnequipItem(item.data as ItemData_Equipment);
        Inventory.Instance.AddItem(item.data as ItemData_Equipment);

        if (equipmentData.equipmentType == EquipmentType.Weapon)
        {

            PlayerWeaponHolder.Instance.UnEquipWeapon();

             

        }

            CleanUpSlot();

    }
}
