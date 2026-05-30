using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Upgrade : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;

    public InventoryItem item;


 
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
        ItemData_Equipment E = item.data as ItemData_Equipment;

        UpgradePanel.Instance.upgradeVisual(E);
        UpgradePanel.Instance.upgradeData(E);

        if (E.Mods == null)
        {
            Debug.Log("No mods!");
            return;
            
        }
        else
        {
            UpgradeSystem.Instance.UpdateModSlot(E);
        
        
        }




    }
}