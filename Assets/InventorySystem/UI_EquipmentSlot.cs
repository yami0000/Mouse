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
        // Inventory undoes the stats, passives and look, and empties this slot.
        if (item?.data is ItemData_Equipment equipmentData)
            Inventory.Instance.UnequipToInventory(equipmentData);
    }

}
