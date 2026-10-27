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

        foreach (ItemData item in startingEquipment)
            InventoryItem.AddTo(inventory, inventoryDictionary, item);

        UpdateSlotUI();
    }

    public void AddItem(ItemData _item)
    {
        InventoryItem.AddTo(inventory, inventoryDictionary, _item);
        UpdateSlotUI();
    }

    public void RemoveItem(ItemData _item)
    {
        if (InventoryItem.RemoveFrom(inventory, inventoryDictionary, _item, false, out _))
            UpdateSlotUI();
    }

    private void UpdateSlotUI() => UI_ItemSlot.Fill(InventoryItemSlot, inventory);
}
