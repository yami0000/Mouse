using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentObject : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData_Equipment itemEquipmentData;
  ¡¡



    private void SetUpEquipmentVisuals()
    {
        if (itemEquipmentData == null)
            return;

        GetComponent<SpriteRenderer>().sprite = itemEquipmentData.icon;
        gameObject.name = "Item object -" + itemEquipmentData.itemName;
    }



    public void SetUpEquipment(ItemData_Equipment _itemData )
    {
        itemEquipmentData = _itemData;
         

        SetUpEquipmentVisuals();
    }
}
