using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSlot : UI_ItemSlot
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item?.data == null) return;

        Purchase P=GetComponent<Purchase>();
        P.CheckCurrency(item.data);
    }
}
