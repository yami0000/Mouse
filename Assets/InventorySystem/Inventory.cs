using System.Collections;
using System.Collections.Generic;
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
        for (int i = 0; i < startingEquipment.Count; i++)
        {
            AddItem(startingEquipment[i]);

        }
    }

    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEqipment = _item as ItemData_Equipment;//as的作用：如果 _item 是 ItemData_Equipment 或其派生类，转换成功，返回该对象。  如果 _item 不是 ItemData_Equipment 类型，返回 null（不会抛出异常）。

        InventoryItem newItem = new InventoryItem(_item);

        ItemData_Equipment oldEquipment = null;

        ItemData_Equipment currentEquipment = null;//

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)//检查装备类型是否相同
        {
            if (item.Key.equipmentType == newEqipment.equipmentType)
                oldEquipment = item.Key;

        }
        if(oldEquipment != null)//如果类型相同，则替换装备
        {
        UnequipItem(oldEquipment);
        AddItem(oldEquipment);
        }

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)//检查装备类型是否相同
        {
            if (item.Key.equipmentType == EquipmentType.Weapon)
                currentEquipment = item.Key;
           // if (currentEquipment.Mods != null)
              //  currentEquipment.ExecuteModEffect(PlayerManager.Instance.player.transform);
          

        }
 
        equipment.Add(newItem);
        equipmentDictionary.Add(newEqipment, newItem);
        newEqipment.AddModifiers();

        RemoveItem(_item);

        UpdateSlotUI();
    }

    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
      

        if (equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem value)&&value != null)
        {
            equipment.Remove(value);
            equipmentDictionary.Remove(itemToRemove);
            itemToRemove.RemoveModifiers(); 

        }
    }

    private void UpdateSlotUI() 
    {
         

        for (int i = 0; i < equipmentSlot.Length;i++) 
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                    equipmentSlot[i].UpdateSlot(item.Value);

            }
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
        
        
        
        }

        if (stashDictionary.TryGetValue(_item, out InventoryItem stashValue))
        {
        if(stashValue.stackSize <= 1)
            {
                stash.Remove(stashValue);   
                stashDictionary.Remove(_item);  
            }
        else
                stashValue.RemoveStack();    
        }


        UpdateSlotUI();
     }

    public int RemoveAllItem(ItemData _item) 
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.RemoveStack();
            UpdateSlotUI();
            return value.stackSize;
        }

        else if (stashDictionary.TryGetValue(_item, out InventoryItem stashValue))
        {
            stashValue.RemoveStack();
            UpdateSlotUI();
            return stashValue.stackSize;
        }
        else return 0;
       

       
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
     public void AddItem(ItemData _item)
    {  if (_item.ItemType == ItemType.Equipment)
       AddToInventory(_item);
       else  
       AddToStash(_item);

       
       UpdateSlotUI();

    }

    private void AddToStash(ItemData _item)
    {
        if (stashDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();

        }
        else
        {
            InventoryItem newitem = new InventoryItem(_item);
            stash.Add(newitem);
            stashDictionary.Add(_item, newitem);

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
}
