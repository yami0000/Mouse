using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Shared by the upgrade screen's slots (UI_Upgrade, UI_ModInventorySlot, UI_modsEquipped): shows an
/// item's icon and stack size, opens its tooltip after HoverDelay.Seconds, and can be dragged (UI_DragSlot).
/// </summary>
public abstract class UpgradeSlotBase : UI_DragSlot, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Old names: UI_ModInventorySlot called these icon and stackText.
    [FormerlySerializedAs("icon")]
    [SerializeField] protected Image itemImage;
    [FormerlySerializedAs("stackText")]
    [Tooltip("Optional. Shows the stack size when there's more than one.")]
    [SerializeField] protected TextMeshProUGUI itemText;

    [Tooltip("Optional. Empty = the scene's item tooltip.")]
    [SerializeField] private InvDescription invDescription;

    private HoverDelay hover;

    /// <summary>What the slot shows, or null when it's empty.</summary>
    public InventoryItem Item { get; private set; }

    private InvDescription Description => invDescription != null ? invDescription : InvDescription.Main;
    private HoverDelay Hover => hover ??= new HoverDelay(this);

    /// <summary>Whether this slot's item can be picked up and dragged.</summary>
    protected virtual bool CanDrag => true;

    protected override Image DragImage => CanDrag && Item?.data != null ? itemImage : null;

    /// <summary>Shows _items in _slots in order and empties the slots left over.</summary>
    public static void Fill<T>(T[] _slots, List<InventoryItem> _items) where T : UpgradeSlotBase
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].UpdateSlot(i < _items.Count ? _items[i] : null);
    }

    public void UpdateSlot(InventoryItem _item)
    {
        if (_item?.data == null)
        {
            CleanUpSlot();
            return;
        }

        Item = _item;

        itemImage.color = Color.white;
        // Keep the icon's own proportions inside the slot.
        itemImage.preserveAspect = true;
        itemImage.sprite = _item.data.icon;

        if (itemText != null)
            itemText.text = _item.stackSize > 1 ? _item.stackSize.ToString() : "";
    }

    public void CleanUpSlot()
    {
        Item = null;

        itemImage.sprite = null;
        itemImage.color = Color.clear;

        if (itemText != null)
            itemText.text = "";
    }

    /// <summary>A plain click (not the end of a drag) on the slot.</summary>
    public virtual void OnPointerClick(PointerEventData _event) { }

    public void OnPointerEnter(PointerEventData _event)
    {
        Hover.Stop();

        // No tooltips over the slots a drag passes across.
        if (Item?.data == null || Dragging != null) return;

        Hover.Start(() =>
        {
            if (Item?.data != null && Description != null)
                Description.Show(Item);
        });
    }

    public void OnPointerExit(PointerEventData _event) => HideTooltip();

    protected override void OnDragStarted()
    {
        HideTooltip();
        // Dimmed while it's being carried, so it reads as "picked up".
        itemImage.color = new Color(1f, 1f, 1f, 0.4f);
    }

    protected override void OnDragEnded()
    {
        // A successful drop has already refreshed the slot; this only undoes the dimming.
        itemImage.color = Item?.data != null ? Color.white : Color.clear;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        HideTooltip();
    }

    private void HideTooltip()
    {
        Hover.Stop();

        if (Description != null)
            Description.Hide();
    }
}
