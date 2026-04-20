using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInventory : MonoBehaviour
{
    public List<ItemData> startingEquipment;

    public List<InventoryItem> inventory;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    [Header("ShopInventory UI")]
    [SerializeField] public Transform inventorySlotParent;

    private UI_ItemSlot[] InventoryItemSlot;

    private void Start()
    {

        inventory = new List<InventoryItem>();
        inventoryDictionary = new Dictionary<ItemData, InventoryItem>();

        InventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        AddStartingItems();
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingEquipment.Count; i++)
        {
            AddItem(startingEquipment[i]);

        }
    }

    public void AddItem(ItemData _item)
    {
       
        AddToInventory(_item);
        
        UpdateSlotUI();

    }

    public void RemoveItem(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            if (value.stackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictionary.Remove(_item);
            }
            else
                value.RemoveStack();

            UpdateSlotUI();
        }
    }
            private void AddToInventory(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();

        }
        else
        {
            InventoryItem newitem = new InventoryItem(_item);
            inventory.Add(newitem);
            inventoryDictionary.Add(_item, newitem);

        }
    }

    private void UpdateSlotUI() 
    {

        for (int i = 0; i < InventoryItemSlot.Length; i++)
        {
            InventoryItemSlot[i].CleanUpSlot();
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            InventoryItemSlot[i].UpdateSlot(inventory[i]);

        }


    }
}
