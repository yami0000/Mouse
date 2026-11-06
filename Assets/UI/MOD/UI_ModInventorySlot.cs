using UnityEngine.EventSystems;

/// <summary>
/// A mod the player owns, in the upgrade screen. Drag it onto one of the open equipment's mod
/// slots (UI_modsEquipped) to install it there, or click it to install it in the first free one.
/// Dropping an installed mod here takes it off the equipment.
/// </summary>
public class UI_ModInventorySlot : UpgradeSlotBase
{
    public override void OnPointerClick(PointerEventData _event)
    {
        if (Item?.data is Mod mod)
            UpgradeSystem.Instance.InstallMod(mod);
    }

    protected override bool Receive(UI_DragSlot _from)
    {
        if (_from is not UI_modsEquipped installed) return false;

        return UpgradeSystem.Instance.UninstallMod(installed.Index);
    }
}
