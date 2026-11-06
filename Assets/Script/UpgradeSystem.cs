using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The upgrade screen's three slot areas: ① the equipment that can take mods (weapons and armor),
/// ② the mods the player owns, ③ the mods installed on the equipment open in the UpgradePanel.
/// Installing, uninstalling and moving mods all go through here, so the slots, the inventory and
/// the panel's numbers stay in step whichever way it was done (drag or click).
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance;

    [SerializeField] public Transform InventorySlotParent;//我的装备
    [SerializeField] public Transform ModSlotParent;//武器装备的mod
    [SerializeField] public Transform modInventorySlotsParent;//拥有的mod

    private UI_Upgrade[] equipmentSlots;
    private UI_modsEquipped[] modSlots;
    private UI_ModInventorySlot[] ownedModSlots;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Found here rather than in Start: Inventory.Start opens the panel, which fills the mod slots,
        // and may run before this Start. Inactive ones too, since the screen starts hidden.
        equipmentSlots = InventorySlotParent.GetComponentsInChildren<UI_Upgrade>(true);
        modSlots = ModSlotParent.GetComponentsInChildren<UI_modsEquipped>(true);
        ownedModSlots = modInventorySlotsParent.GetComponentsInChildren<UI_ModInventorySlot>(true);
    }

    private void Start()
    {
        Inventory.Instance.OnInventoryUIUpdated += UpdateUI;
        UpdateUI();//当inventory.cs中的槽位更新每被触发一次，此UI中内容也刷新一次
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryUIUpdated -= UpdateUI;
    }

    /// <summary>Only weapons and armor take mods, so only they are listed in ①.</summary>
    public static bool CanUpgrade(InventoryItem _item) =>
        _item?.data is ItemData_Equipment equipment &&
        (equipment.equipmentType == EquipmentType.Weapon || equipment.equipmentType == EquipmentType.Armor);

    /// <summary>What ① lists: the worn equipment first, then the rest of the inventory.</summary>
    public List<InventoryItem> UpgradableEquipment()
    {
        var list = new List<InventoryItem>();

        foreach (InventoryItem item in Inventory.Instance.equipment)
            if (CanUpgrade(item)) list.Add(item);

        foreach (InventoryItem item in Inventory.Instance.inventory)
            if (CanUpgrade(item)) list.Add(item);

        return list;
    }

    /// <summary>Refreshes ① and ②.</summary>
    public void UpdateUI()
    {
        UpgradeSlotBase.Fill(equipmentSlots, UpgradableEquipment());

        var ownedMods = new List<InventoryItem>();
        foreach (InventoryItem item in Inventory.Instance.stash)
            if (item.data != null && item.data.ItemType == ItemType.Mods)
                ownedMods.Add(item);

        UpgradeSlotBase.Fill(ownedModSlots, ownedMods);
    }

    /// <summary>Shows _equipment's mods in ③, or empties it.</summary>
    public void UpdateModSlot(ItemData_Equipment _equipment)
    {
        for (int i = 0; i < modSlots.Length; i++)
        {
            Mod mod = _equipment?.Mods != null && i < _equipment.Mods.Length ? _equipment.Mods[i] : null;
            modSlots[i].UpdateSlot(mod);
        }
    }

    #region Install and Uninstall

    private static ItemData_Equipment Open => UpgradePanel.Instance != null ? UpgradePanel.Instance.Equipment : null;

    /// <summary>
    /// Installs one of the owned _mod on the open equipment: in slot _slot, swapping whatever is there
    /// back to the owned mods, or in the first free slot for -1. False when nothing changed.
    /// </summary>
    public bool InstallMod(Mod _mod, int _slot = -1)
    {
        ItemData_Equipment equipment = Open;
        if (equipment?.Mods == null || _mod == null) return false;
        if (_slot >= equipment.Mods.Length) return false; // more slots on screen than this equipment has

        // Take the old one off first so the slot is free; it's put back if the new one doesn't fit.
        Mod replaced = null;
        if (_slot >= 0 && equipment.Mods[_slot] != null && !equipment.UninstallModAt(_slot, out replaced))
            return false;

        if (!equipment.InstallMod(_mod, _slot))
        {
            if (replaced != null)
                equipment.InstallMod(replaced, _slot);
            return false;
        }

        Inventory.Instance.RemoveItem(_mod);
        if (replaced != null)
            Inventory.Instance.AddItem(replaced, notify: false); // not a pickup, so no "new item" toast

        OnModsChanged(equipment);
        return true;
    }

    /// <summary>Takes the mod in slot _slot off the open equipment and back to the owned mods.</summary>
    public bool UninstallMod(int _slot)
    {
        ItemData_Equipment equipment = Open;
        if (equipment == null || !equipment.UninstallModAt(_slot, out Mod removed)) return false;

        Inventory.Instance.AddItem(removed, notify: false);

        OnModsChanged(equipment);
        return true;
    }

    /// <summary>Swaps two of the open equipment's mod slots. The same mods stay installed.</summary>
    public bool MoveMod(int _from, int _to)
    {
        ItemData_Equipment equipment = Open;
        if (equipment == null || !equipment.MoveMod(_from, _to)) return false;

        UpdateModSlot(equipment);
        return true;
    }

    private void OnModsChanged(ItemData_Equipment _equipment)
    {
        UpdateModSlot(_equipment);
        UpgradePanel.Instance.ModsChanged();
    }

    #endregion
}
