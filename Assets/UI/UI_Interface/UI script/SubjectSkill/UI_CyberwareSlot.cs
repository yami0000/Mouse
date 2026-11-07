using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>One cyberware in the panel: its usual item description on hover, left-click to (un)install.</summary>
public class UI_CyberwareSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI count;

    private UI_CyberwarePanel panel;
    private HoverDelay hover;

    public InventoryItem Item { get; private set; }

    private HoverDelay Hover => hover ??= new HoverDelay(this);

    public void Show(UI_CyberwarePanel _panel, InventoryItem _item)
    {
        panel = _panel;
        Item = _item;

        icon.sprite = _item != null ? _item.data.icon : null;
        icon.enabled = icon.sprite != null;
        icon.preserveAspect = true;

        if (count != null)
            count.text = _item != null && _item.stackSize > 1 ? _item.stackSize.ToString() : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover.Stop();
        if (Item == null) return;

        Hover.Start(() =>
        {
            if (Item != null && InvDescription.Main != null)
                InvDescription.Main.Show(Item);
        });
    }

    public void OnPointerExit(PointerEventData eventData) => HideDescription();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || panel == null) return;

        HideDescription();
        panel.SlotClicked(this);
    }

    private void OnDisable() => HideDescription();

    private void HideDescription()
    {
        Hover.Stop();

        if (InvDescription.Main != null)
            InvDescription.Main.Hide();
    }
}
