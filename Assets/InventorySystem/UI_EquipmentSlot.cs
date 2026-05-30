using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipmentSlot : UI_ItemSlot 
{
     public EquipmentType slotType;
     public InventoryItem currentItem;

    private void OnValidate()
    {
        gameObject.name = "Equipment Slot -" + slotType.ToString(); 
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item?.data == null)
        {
            return;
        }
        ItemData_Equipment equipmentData = item.data as ItemData_Equipment;


        if (equipmentData.equipmentType == EquipmentType.Throwable)
        {
            if (Inventory.Instance.UnequipItem(item.data as ItemData_Equipment, out int n))
        Inventory.Instance.AddItem(item.data as ItemData_Equipment, n);
        }
        else
        {
            
            Inventory.Instance.UnequipItem(item.data as ItemData_Equipment);
            Inventory.Instance.AddItem(item.data as ItemData_Equipment);
            GetPlayer().GetComponent<PassiveSkillHandler>().UnregisterItem(item.data);
        }
               

        if (equipmentData.equipmentType == EquipmentType.Weapon)
        PlayerWeaponHolder.Instance.UnEquipWeapon();
        if (equipmentData.equipmentType == EquipmentType.Armor)
        PlayerArmorHolder.Instance.UnEquipArmor();
        if (equipmentData.equipmentType == EquipmentType.Companion)
        PlayerCompanion.Instance.UnEquipCompanion();

        currentItem.data = null;


        CleanUpSlot();

    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }
    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

}
