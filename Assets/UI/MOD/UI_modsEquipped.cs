using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_modsEquipped : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image itemImage;
    public Mod mod;

    public void CleanUpSlot()
    {
        mod = null;
        itemImage.sprite = null;
        itemImage.color = Color.clear;
    }


    public void UpdateSlot(Mod newMod)
    {
        mod = newMod;

        if (mod == null)
        {
            CleanUpSlot();
            return;
        }

        itemImage.color = Color.white;
        itemImage.sprite = mod.icon;
    }

    // Clicking an installed mod takes it off the equipment and back into the inventory.
    public void OnPointerDown(PointerEventData eventData)
    {
        UpgradePanel panel = UpgradePanel.Instance;
        if (panel == null || panel.Equipment == null) return;

        int index = transform.GetSiblingIndex(); // index of clicked slot in the UI
        if (!panel.Equipment.UninstallModAt(index, out Mod removedMod)) return; // undoes its stats and weapon handling

        Inventory.Instance.AddItem(removedMod);

        UpgradeSystem.Instance.UpdateModSlot(panel.Equipment);
        panel.upgradeData(panel.Equipment);
    }
}
