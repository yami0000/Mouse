using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Lives on the entry prefab (left-side list row).
public class UI_ShopEntry : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ItemData item;
    private UI_Shop shopUI;

    public void Setup(ItemData _item, UI_Shop _shopUI)
    {
        item = _item;
        shopUI = _shopUI;

        iconImage.sprite = item.icon;
        iconImage.preserveAspect = true;
        iconImage.color = Color.white;

        nameText.text = item.itemName;
        priceText.text = item.price.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;
        shopUI.ShowInfo(item);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (item == null) return;
        shopUI.TryPurchase(item);
    }
}
