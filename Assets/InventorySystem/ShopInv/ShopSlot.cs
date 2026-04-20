using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSlot : UI_ItemSlot
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        Purchase P=GetComponent<Purchase>();
        P.CheckCurrency(item.data);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }
}
