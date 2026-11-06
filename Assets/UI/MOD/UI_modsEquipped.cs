using UnityEngine.EventSystems;

/// <summary>
/// One of the mod slots of the equipment open in the upgrade panel. Takes owned mods dropped on it
/// (swapping out whatever was there), swaps places with another of these slots, and goes back to
/// the owned mods when dragged onto them or clicked.
/// </summary>
public class UI_modsEquipped : UpgradeSlotBase
{
    /// <summary>The equipment's mod slot this shows (its place among the slots in the ModSlot parent).</summary>
    public int Index => transform.GetSiblingIndex();

    public Mod mod => Item?.data as Mod;

    public void UpdateSlot(Mod _mod) => UpdateSlot(_mod != null ? new InventoryItem(_mod) : null);

    public override void OnPointerClick(PointerEventData _event)
    {
        if (mod != null)
            UpgradeSystem.Instance.UninstallMod(Index);
    }

    protected override bool Receive(UI_DragSlot _from)
    {
        switch (_from)
        {
            case UI_ModInventorySlot owned when owned.Item?.data is Mod newMod:
                return UpgradeSystem.Instance.InstallMod(newMod, Index);

            case UI_modsEquipped other:
                return UpgradeSystem.Instance.MoveMod(other.Index, Index);

            default:
                return false;
        }
    }
}
