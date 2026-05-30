using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;




public class UI_ItemSlot : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler
{
    [SerializeField] private InvDescription invDescription;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;
    private bool isHovered = false;

    public InventoryItem item;
    [HideInInspector]public  ItemData_Equipment equip;
    

    public void Start()
    {
      
    }
    
    public void UpdateSlot(InventoryItem _newItem)
    {
        if (_newItem == null || _newItem.data == null)
        {
            CleanUpSlot();
            return;
        }

        item = _newItem;
        itemImage.color = Color.white;
        itemImage.preserveAspect = true;
        itemImage.sprite = item.data.icon;

        if (item.data is ItemData_Equipment equipmentData)
        {
            equip = equipmentData;
        }

        itemText.text = item.stackSize > 1 ? item.stackSize.ToString() : "";

    }

    public void CleanUpSlot()
    {
        item = null;
        equip = null;

        itemImage.sprite = null;
        itemImage.color = Color.clear;
        itemText.text = "";

    }
    private void OnDisable()
    {
        
        isHovered = false;

        invDescription?.Hide();
        
        StopAllCoroutines();
    }//当UI被关掉时，说明栏也会随之消失
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
        {
            
            return;
        }

        if (item.data.ItemType == ItemType.Equipment)
        {
            ItemData_Equipment equipmentData = item.data as ItemData_Equipment;//这是件新的装备，但不一定是武器




            if (equipmentData.equipmentType == EquipmentType.Throwable)
                Inventory.Instance.EquipItem(item.data, transferAll: true);
            else
            {
                GetPlayer().GetComponent<PassiveSkillHandler>().RegisterItem(item.data);
              
                Inventory.Instance.EquipItem(item.data);
            
            }


          
              if (equipmentData.equipmentType == EquipmentType.Weapon) //装备武器
              PlayerWeaponHolder.Instance.EquipWeapon(equipmentData);

              if(equipmentData.equipmentType == EquipmentType.Armor)//装备护甲/帽子？
              PlayerArmorHolder.Instance.EquipArmor(equipmentData);

            if (equipmentData.equipmentType == EquipmentType.Companion)
              PlayerCompanion.Instance.EquipCompanion(equipmentData);

             



        }

        
        if (item?.data.ItemType == ItemType.UsableObject)
        {
            ItemData data = item.data;
            Inventory.Instance.RemoveItem(data);
            data.ExecuteItemEffect(GetPlayer().transform);

        }


    }


    public virtual void OnPointerEnter(PointerEventData eventData)
    {

        isHovered = true;
        if (item?.data == null) return;

        StartCoroutine(WaitTillShow());

    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (invDescription == null) return;
        invDescription.Hide();
    }
    IEnumerator WaitTillShow()
    {
        yield return new WaitForSeconds(1f);
        if (item?.data == null) yield break;
        if (isHovered)
            invDescription.Show(item,equip);
           
              

    }
    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

    
}

