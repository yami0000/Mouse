using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public List<ItemData> startingEquipment;

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
        foreach (ItemData item in startingEquipment)
            AddItem(item, 1, notify: false);
    }

    public ItemData_Equipment Throwable()
    {
        var entry = equipmentDictionary.FirstOrDefault(item => item.Key.equipmentType == EquipmentType.Throwable);
        return entry.Key; // null if not found, FirstOrDefault handles it
    }
    public void EquipItem(ItemData _item, UI_EquipmentSlot targetSlot = null, bool transferAll = false)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        if (newEquipment == null)return;
 
        UI_EquipmentSlot slot = targetSlot
            ?? GetFirstEmptySlotOfType(newEquipment.equipmentType)
            ?? GetFirstSlotOfType(newEquipment.equipmentType);

        if (slot == null) return;

        // Unequip whatever is already in that slot and return it to inventory
        if (slot.currentItem?.data != null)
        {
            ItemData_Equipment oldEquipment = slot.currentItem.data as ItemData_Equipment;


            if (oldEquipment != null)
            {
                // Unequip and recover the full stack back to inventory in one pass
                if (UnequipItem(oldEquipment, out int recoveredStack))
                    AddItem(oldEquipment, recoveredStack, notify: false);

                slot.currentItem = null;
            }
        }

        // Find source stack
        if (!inventoryDictionary.TryGetValue(_item, out InventoryItem source))
            stashDictionary.TryGetValue(_item, out source);

        if (source == null)
        {
            Debug.LogError($"Item not found in inventory or stash: {_item?.name}");
            return;
        }

        int stackToTransfer = transferAll ? source.stackSize : 1;

        // Build the equipped item with the correct stack BEFORE removing from inventory
        InventoryItem newItem = new InventoryItem(_item);
        for (int i = 1; i < stackToTransfer; i++)
            newItem.AddStack();

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        slot.currentItem = newItem;

        if (!transferAll)
            newEquipment.AddModifiers(); // modifiers don't apply to consumables

        // Safe to remove now that equipment is registered
        if (transferAll)
            RemoveItem(_item, removeAll: true);
        else
            RemoveItem(_item);

        UpdateSlotUI();
    }
    public bool UnequipItem(ItemData_Equipment itemToRemove, out int stackSize)
    {
        stackSize = 0;
        if (itemToRemove == null) return false;

        if (equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem value))
        {
            stackSize = value.stackSize;
            equipment.Remove(value);
            equipmentDictionary.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
            return true;
        }
        return false;
    }
    public bool UnequipItem(ItemData_Equipment itemToRemove) => UnequipItem(itemToRemove, out _);
    public int UnequipAllItem(ItemData_Equipment itemToRemove)
    {
        UnequipItem(itemToRemove, out int stackSize);
        return stackSize;
    }
    public void RemoveEquipItem(ItemData _item, bool removeAll = false)
    {
        ItemData_Equipment E = _item as ItemData_Equipment;
        if (E == null) return;

        if (!equipmentDictionary.TryGetValue(E, out InventoryItem equipmentValue)) return;
        if (E.equipmentType != EquipmentType.Throwable) return;

        if (removeAll || equipmentValue.stackSize <= 1)
        {
            // Full removal: clear dictionary, list, and the slot that owns this item
            equipment.Remove(equipmentValue);
            equipmentDictionary.Remove(E);
            E.RemoveModifiers();

            UI_EquipmentSlot owningSlot = System.Array.Find(equipmentSlot,
                s => s.currentItem == equipmentValue);
            if (owningSlot != null)
            {
                owningSlot.currentItem = null;
                owningSlot.CleanUpSlot();
            }
        }
        else
        {
            equipmentValue.RemoveStack();
        }

        UpdateSlotUI();
    }
    public int RemoveItem(ItemData _item, bool removeAll = false)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            int removed = removeAll ? value.stackSize : 1;
            if (removeAll || value.stackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictionary.Remove(_item);
            }
            else
            {
                value.RemoveStack();
            }
            UpdateSlotUI();
            return removed;
        }

        if (stashDictionary.TryGetValue(_item, out InventoryItem stashValue))
        {
            int removed = removeAll ? stashValue.stackSize : 1;
            if (removeAll || stashValue.stackSize <= 1)
            {
                stash.Remove(stashValue);
                stashDictionary.Remove(_item);
            }
            else
            {
                stashValue.RemoveStack();
            }
            UpdateSlotUI();
            return removed;
        }

        return 0;
    }
    public void AddItem(ItemData _item, int count = 1, bool notify = true)
    {
        if (count <= 0) return;

        if (_item.ItemType == ItemType.Equipment)
            AddToInventory(_item, count);
        else
            AddToStash(_item, count);

        if (notify)
            OnItemAdded?.Invoke(_item, count);

        UpdateSlotUI();
    }
    private void AddToInventory(ItemData _item, int count = 1)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            for (int i = 0; i < count; i++) value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            for (int i = 1; i < count; i++) newItem.AddStack();  
            inventory.Add(newItem);
            inventoryDictionary.Add(_item, newItem);
        }
    }
    private void AddToStash(ItemData _item, int count = 1)
    {
        if (stashDictionary.TryGetValue(_item, out InventoryItem value))
        {
            for (int i = 0; i < count; i++) value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            for (int i = 1; i < count; i++) newItem.AddStack();
            stash.Add(newItem);
            stashDictionary.Add(_item, newItem);
        }
    }

    #region Slot and UI
    private UI_EquipmentSlot GetFirstEmptySlotOfType(EquipmentType type)
    {
        return System.Array.Find(equipmentSlot,
            s => s.slotType == type && s.currentItem.data == null);
    }
    private UI_EquipmentSlot GetFirstSlotOfType(EquipmentType type)
    {
        return System.Array.Find(equipmentSlot,
            s => s.slotType == type);
    }
    public ItemData_Equipment GetEquipment(EquipmentType _type) 
    {
        ItemData_Equipment equipedItem = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary) 
        {
            if (item.Key.equipmentType == _type)
                equipedItem = item.Key;

        }

        return equipedItem;
    }
    private void UpdateSlotUI() 
    {
        foreach (UI_EquipmentSlot slot in equipmentSlot)
        {
            if (slot.currentItem != null)
                slot.UpdateSlot(slot.currentItem);
            else
                slot.CleanUpSlot();
        }

        
        for (int i = 0; i < InventoryItemSlot.Length; i++)
        {
            InventoryItemSlot[i].CleanUpSlot();
        }

        for (int i = 0; i < stashItemSlot.Length; i++)
        {
            stashItemSlot[i].CleanUpSlot();
        }



        for (int i = 0; i < inventory.Count; i++) 
        {
            InventoryItemSlot[i].UpdateSlot(inventory[i]);

        }

        for (int i = 0; i < stash.Count; i++)
        {
            stashItemSlot[i].UpdateSlot(stash[i]);  

        }

        OnInventoryUIUpdated?.Invoke();


    }
    #endregion
}
