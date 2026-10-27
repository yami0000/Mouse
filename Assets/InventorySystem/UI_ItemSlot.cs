using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;




public class UI_ItemSlot : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler
{
    [SerializeField] private InvDescription invDescription;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;
    private bool isHovered = false;
    private Coroutine showRoutine;

    public InventoryItem item;
    [HideInInspector]public  ItemData_Equipment equip;

    /// <summary>Shows _items in _slots in order and empties the slots left over.</summary>
    public static void Fill(UI_ItemSlot[] _slots, List<InventoryItem> _items)
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].UpdateSlot(i < _items.Count ? _items[i] : null);
    }

    public void UpdateSlot(InventoryItem _newItem)
    {
        if (_newItem == null || _newItem.data == null)
        {
            CleanUpSlot();
            return;
        }

        item = _newItem;
        equip = item.data as ItemData_Equipment;

        itemImage.color = Color.white;
        itemImage.preserveAspect = true;
        itemImage.sprite = item.data.icon;

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
    }//当UI隐藏的时候说明栏也会随之消失
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        // Inventory works out the slot, the stats, the passives and what shows on the player.
        if (item.data.ItemType == ItemType.Equipment)
            Inventory.Instance.EquipItem(item.data);

        else if (item.data.ItemType == ItemType.UsableObject)
        {
            ItemData data = item.data;
            Inventory.Instance.RemoveItem(data);
            data.ExecuteItemEffect(GetPlayer().transform);

        }


    }


    public virtual void OnPointerEnter(PointerEventData eventData)
    {

        isHovered = true;
        StopShowRoutine();
        if (item?.data == null) return;

        showRoutine = StartCoroutine(WaitTillShow());

    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StopShowRoutine();
        if (invDescription == null) return;
        invDescription.Hide();
    }
    IEnumerator WaitTillShow()
    {
        // Realtime so the tooltip still appears if the inventory menu sets Time.timeScale = 0
        yield return new WaitForSecondsRealtime(1f);
        showRoutine = null;
        if (item?.data == null) yield break;
        if (isHovered)
            invDescription.Show(item,equip);

    }

    private void StopShowRoutine()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
    }
    protected static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }


}
