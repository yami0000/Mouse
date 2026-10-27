using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingItem
{
    public ItemData item;
    [Min(1)] public int amount = 1;
}

public class Inventory : MonoBehaviour, ISerializationCallbackReceiver
{
    public static Inventory Instance;

    public List<StartingItem> startingItems = new List<StartingItem>();

    // Old item-only list, folded into startingItems (amount 1) on load.
    [SerializeField, HideInInspector] private List<ItemData> startingEquipment = new List<ItemData>();

    public List<InventoryItem> inventory;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    public List <InventoryItem> stash;
    public Dictionary<ItemData,InventoryItem> stashDictionary;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictionary;


    [Header("inventory UI")]

    [SerializeField]public Transform inventorySlotParent;
    [SerializeField]public Transform stashSlotParent;
    [SerializeField]public Transform equipmentSlotParent;

    private UI_ItemSlot[] InventoryItemSlot;
    private UI_ItemSlot[] stashItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;

    public event System.Action OnInventoryUIUpdated;
    public event System.Action<ItemData, int> OnItemAdded;

    // asset -> the copy the inventory actually holds (see ToRuntime)
    private readonly Dictionary<ItemData_Equipment, ItemData_Equipment> runtimeCopies = new Dictionary<ItemData_Equipment, ItemData_Equipment>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        inventory = new List<InventoryItem>();
        inventoryDictionary = new Dictionary<ItemData, InventoryItem>();

        stash = new List<InventoryItem>();
        stashDictionary = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictionary = new Dictionary<ItemData_Equipment, InventoryItem>();

        InventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        stashItemSlot = stashSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();

        AddStartingItems();
        UpgradePanel.Instance.Initialize();
    }

    private void AddStartingItems()
    {
        foreach (StartingItem entry in startingItems)
        {
            if (entry == null || entry.item == null) continue;

            // A fresh list element can come in at 0, which would add nothing.
            AddItem(entry.item, Mathf.Max(1, entry.amount), notify: false);
        }
    }

    private void OnDestroy()
    {
        foreach (ItemData_Equipment copy in runtimeCopies.Values)
            if (copy != null)
                Destroy(copy);

        runtimeCopies.Clear();
    }

    /// <summary>
    /// Equipment is held as a copy of its asset, made the first time the asset is seen, so mods and
    /// weapon tweaks never write back into the project. Every call with the same asset gets the same
    /// copy, so stacking and lookups behave as before. Anything else, or a copy, comes back unchanged.
    /// Use this before looking an item up in the dictionaries yourself.
    /// </summary>
    public ItemData ToRuntime(ItemData _item)
    {
        if (_item is not ItemData_Equipment equipment || equipment.IsRuntimeCopy)
            return _item;

        if (!runtimeCopies.TryGetValue(equipment, out ItemData_Equipment copy) || copy == null)
        {
            copy = equipment.CreateRuntimeCopy();
            runtimeCopies[equipment] = copy;
        }

        return copy;
    }

    /// <summary>How many of _item are carried, counting the inventory and the stash. Takes the asset or its copy.</summary>
    public int GetStackSize(ItemData _item)
    {
        _item = ToRuntime(_item);
        int count = 0;

        if (inventoryDictionary.TryGetValue(_item, out InventoryItem inventoryValue))
            count += inventoryValue.stackSize;

        if (stashDictionary.TryGetValue(_item, out InventoryItem stashValue))
            count += stashValue.stackSize;

        return count;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (startingEquipment == null || startingEquipment.Count == 0) return;

        startingItems ??= new List<StartingItem>();

        foreach (ItemData item in startingEquipment)
            startingItems.Add(new StartingItem { item = item, amount = 1 });

        startingEquipment.Clear();
    }

    #region Equip

    /// <summary>
    /// Moves an item from the inventory or stash into an equipment slot, sending whatever was in
    /// that slot back. Throwables move their whole stack and carry no stats or passives; anything
    /// else moves one at a time and starts applying its stats, passives and look on the player.
    /// </summary>
    public void EquipItem(ItemData _item, UI_EquipmentSlot targetSlot = null)
    {
        _item = ToRuntime(_item);
        if (_item is not ItemData_Equipment newEquipment) return;

        bool isThrowable = newEquipment.equipmentType == EquipmentType.Throwable;

        UI_EquipmentSlot slot = targetSlot
            ?? GetFirstEmptySlotOfType(newEquipment.equipmentType)
            ?? GetFirstSlotOfType(newEquipment.equipmentType);

        if (slot == null) return;

        // Unequip whatever is already in that slot and return it to inventory
        if (slot.currentItem?.data is ItemData_Equipment oldEquipment)
            UnequipToInventory(oldEquipment);

        // Find source stack
        if (!inventoryDictionary.TryGetValue(_item, out InventoryItem source))
            stashDictionary.TryGetValue(_item, out source);

        if (source == null)
        {
            Debug.LogError($"Item not found in inventory or stash: {_item.name}");
            return;
        }

        // Build the equipped item with the correct stack BEFORE removing from inventory
        InventoryItem newItem = new InventoryItem(_item, isThrowable ? source.stackSize : 1);

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        slot.currentItem = newItem;

        if (!isThrowable) // modifiers and passives don't apply to consumables
        {
            GetPassives().RegisterItem(newEquipment);
            newEquipment.AddModifiers();
        }

        ShowOnPlayer(newEquipment);

        // Safe to remove now that equipment is registered; this also refreshes the slots.
        RemoveItem(_item, removeAll: isThrowable);
    }

    /// <summary>
    /// Takes the item off: undoes its stats, passives and look, and empties its slot.
    /// stackSize is how many were equipped, so the caller can hand them back.
    /// </summary>
    public bool UnequipItem(ItemData_Equipment itemToRemove, out int stackSize)
    {
        stackSize = 0;
        itemToRemove = ToRuntime(itemToRemove) as ItemData_Equipment;
        if (itemToRemove == null) return false;
        if (!equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem value)) return false;

        stackSize = value.stackSize;
        equipment.Remove(value);
        equipmentDictionary.Remove(itemToRemove);

        if (itemToRemove.equipmentType != EquipmentType.Throwable)
        {
            itemToRemove.RemoveModifiers();
            GetPassives().UnregisterItem(itemToRemove);
        }

        HideFromPlayer(itemToRemove);

        UI_EquipmentSlot owningSlot = System.Array.Find(equipmentSlot, s => s.currentItem == value);
        if (owningSlot != null)
            owningSlot.currentItem = null;

        return true;
    }
    public bool UnequipItem(ItemData_Equipment itemToRemove) => UnequipItem(itemToRemove, out _);

    /// <summary>Unequips the item and puts everything that was equipped back into the inventory.</summary>
    public void UnequipToInventory(ItemData_Equipment _item)
    {
        if (UnequipItem(_item, out int stackSize))
            AddItem(_item, stackSize, notify: false);
    }

    /// <summary>Uses up one equipped throwable (or the whole stack), emptying its slot when none are left.</summary>
    public void RemoveEquipItem(ItemData _item, bool removeAll = false)
    {
        _item = ToRuntime(_item);
        if (_item is not ItemData_Equipment throwable || throwable.equipmentType != EquipmentType.Throwable) return;
        if (!equipmentDictionary.TryGetValue(throwable, out InventoryItem stack)) return;

        if (removeAll || stack.stackSize <= 1)
            UnequipItem(throwable);
        else
            stack.RemoveStack();

        UpdateSlotUI();
    }

    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        foreach (ItemData_Equipment equipped in equipmentDictionary.Keys)
        {
            if (equipped.equipmentType == _type)
                return equipped;
        }

        return null;
    }

    private static PassiveSkillHandler GetPassives() => PlayerManager.Instance.player.GetComponent<PassiveSkillHandler>();

    private static void ShowOnPlayer(ItemData_Equipment _item)
    {
        switch (_item.equipmentType)
        {
            case EquipmentType.Weapon: PlayerWeaponHolder.Instance.EquipWeapon(_item); break;     //装备武器
            case EquipmentType.Armor: PlayerArmorHolder.Instance.EquipArmor(_item); break;        //装备护甲/帽子
            case EquipmentType.Companion: PlayerCompanion.Instance.EquipCompanion(_item); break;
        }
    }

    private static void HideFromPlayer(ItemData_Equipment _item)
    {
        switch (_item.equipmentType)
        {
            case EquipmentType.Weapon: PlayerWeaponHolder.Instance.UnEquipWeapon(); break;
            case EquipmentType.Armor: PlayerArmorHolder.Instance.UnEquipArmor(); break;
            case EquipmentType.Companion: PlayerCompanion.Instance.UnEquipCompanion(); break;
        }
    }

    #endregion

    #region Add and Remove

    public int RemoveItem(ItemData _item, bool removeAll = false)
    {
        _item = ToRuntime(_item);

        if (!InventoryItem.RemoveFrom(inventory, inventoryDictionary, _item, removeAll, out int removed) &&
            !InventoryItem.RemoveFrom(stash, stashDictionary, _item, removeAll, out removed))
            return 0;

        UpdateSlotUI();
        return removed;
    }
    public void AddItem(ItemData _item, int count = 1, bool notify = true)
    {
        if (count <= 0) return;

        _item = ToRuntime(_item);

        // Equipment goes to the inventory, everything else (mods, materials, currency) to the stash.
        if (_item.ItemType == ItemType.Equipment)
            InventoryItem.AddTo(inventory, inventoryDictionary, _item, count);
        else
            InventoryItem.AddTo(stash, stashDictionary, _item, count);

        if (notify)
            OnItemAdded?.Invoke(_item, count);

        UpdateSlotUI();
    }

    #endregion

    #region Slot and UI
    private UI_EquipmentSlot GetFirstEmptySlotOfType(EquipmentType type)
    {
        return System.Array.Find(equipmentSlot,
            s => s.slotType == type && s.currentItem?.data == null);
    }
    private UI_EquipmentSlot GetFirstSlotOfType(EquipmentType type)
    {
        return System.Array.Find(equipmentSlot,
            s => s.slotType == type);
    }
    private void UpdateSlotUI()
    {
        foreach (UI_EquipmentSlot slot in equipmentSlot)
            slot.UpdateSlot(slot.currentItem); // an empty slot cleans itself up

        UI_ItemSlot.Fill(InventoryItemSlot, inventory);
        UI_ItemSlot.Fill(stashItemSlot, stash);

        OnInventoryUIUpdated?.Invoke();
    }
    #endregion
}
