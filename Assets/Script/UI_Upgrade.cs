using UnityEngine.EventSystems;

/// <summary>
/// A slot in the upgrade screen's equipment list (weapons and armor, see UpgradeSystem.UpdateUI).
/// Clicking it opens that equipment in the upgrade panel and its mods in the mod slots.
/// </summary>
public class UI_Upgrade : UpgradeSlotBase
{
    // Equipment stays in this list; nothing to drag it onto yet.
    protected override bool CanDrag => false;

    public override void OnPointerClick(PointerEventData _event)
    {
        if (Item?.data is ItemData_Equipment equipment)
            UpgradePanel.Instance.Select(equipment);
    }
}
