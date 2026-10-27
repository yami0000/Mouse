using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ModInventorySlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI stackText; // optional

    public InventoryItem item { get; private set; }

    public void CleanUpSlot()
    {
        item = null;
        icon.sprite = null;
        icon.color = Color.clear;
        if (stackText != null) stackText.text = "";
    }

    public void UpdateSlot(InventoryItem newItem)
    {
        item = newItem;

        if (item == null || item.data == null)
        {
            CleanUpSlot();
            return;
        }

        icon.color = Color.white;
        icon.sprite = item.data.icon;

        if (stackText != null)
            stackText.text = item.stackSize > 1 ? item.stackSize.ToString() : "";
    }

    // Clicking an owned mod installs it on the equipment open in the upgrade panel.
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item?.data is not Mod mod) return;

        UpgradePanel panel = UpgradePanel.Instance;
        if (panel == null || panel.Equipment == null) return;

        if (!panel.Equipment.InstallMod(mod)) return; // applies its stats and weapon handling

        Inventory.Instance.RemoveItem(mod);
        UpgradeSystem.Instance.UpdateModSlot(panel.Equipment);
        panel.upgradeData(panel.Equipment);
    }
}
