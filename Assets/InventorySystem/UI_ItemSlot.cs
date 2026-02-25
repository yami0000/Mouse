using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;




public class UI_ItemSlot : MonoBehaviour ,IPointerDownHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;

    public InventoryItem item;
  

    public void Start()
    {
    }
    public void UpdateSlot(InventoryItem _newitem)
    {
        item = _newitem;

        itemImage.color = Color.white;  

        if (item != null)
        {
            itemImage.sprite = item.data.icon;
            if (item.stackSize > 1)
            {
                itemText.text = item.stackSize.ToString();
            }
            else
            {
                itemText.text = "";

            }


        }
       
    }

    public void CleanUpSlot()
    {
        item = null;

        itemImage.sprite = null;
        itemImage.color = Color.clear;
        itemText.text = "";

    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
        {
            
            return;
        }

        if (item.data.ItemType == ItemType.Equipment)
        {
            ItemData_Equipment equipmentData = item.data as ItemData_Equipment;//这是件新的装备，但不一定是武器

            ItemData_Equipment oldequipment = null;//检查装备槽里的装备

            foreach(KeyValuePair<ItemData_Equipment,InventoryItem> item in Inventory.Instance.equipmentDictionary)//如果装备槽中有武器，找出并赋值“oldequipment”
            {
                if (item.Key.equipmentType == EquipmentType.Weapon)
                    oldequipment = item.Key;
                else
                    oldequipment = null ;

            }

            Inventory.Instance.EquipItem(item.data);


          
              if (equipmentData.equipmentType == EquipmentType.Weapon) //装备武器
              PlayerWeaponHolder.Instance.EquipWeapon(equipmentData);

              if(equipmentData.equipmentType == EquipmentType.Armor)//装备护甲/帽子？
              PlayerArmorHolder.Instance.EquipArmor(equipmentData);

            if (equipmentData.equipmentType == EquipmentType.Companion)
              PlayerCompanion.Instance.EquipCompanion(equipmentData);



        }

        if (item == null || item.data == null)
        {

            return;
        }
        if (item.data.ItemType == ItemType.UsableObject)
        {
            ItemData data = item.data;
            Inventory.Instance.RemoveItem(data);
            data.ExecuteItemEffect(PlayerManager.Instance.player.transform);
             
        }
    }

 


}

