using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance;

    [SerializeField] public Transform InventorySlotParent;//我的装备
    [SerializeField] public Transform ModSlotParent;//武器装备的mod
    [SerializeField] public Transform modInventorySlotsParent;//拥有的mod
   
    private UI_Upgrade[] InventorySlot;
    private UI_modsEquipped[] ModSlot;
    private UI_ModInventorySlot[] modInventorySlots;
    private int I;
    private void Awake()
    {


        if (Instance == null)
        {
            Instance = this;
            
        }
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        Inventory.Instance.OnInventoryUIUpdated += UpdateUI;
        Inventory.Instance.OnInventoryUIUpdated += invModSlot;
        InventorySlot = InventorySlotParent.GetComponentsInChildren<UI_Upgrade>();
        ModSlot = ModSlotParent.GetComponentsInChildren<UI_modsEquipped>();
        modInventorySlots = modInventorySlotsParent.GetComponentsInChildren<UI_ModInventorySlot>();
        UpdateUI();//当inventory.cs中的槽位更新每被触发一次，此UI中内容也刷新一次
        invModSlot();
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryUIUpdated -= UpdateUI;
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryUIUpdated -= invModSlot;
    }
    public void UpdateUI() 
    {

        for (int i = 0; i < InventorySlot.Length; i++)
        {
            InventorySlot[i].CleanUpSlot();
        }

        if (Inventory.Instance.equipment.Count == 0)
            I = -1;
        else
        {
            for (int i = 0; i < Inventory.Instance.equipment.Count; i++)
            {

                InventorySlot[i].UpdateSlot(Inventory.Instance.equipment[i]);
                I = i;
            }
        }
        for (int i = I+1; i < I+1+Inventory.Instance.inventory.Count; i++)
        {
            InventorySlot[i].UpdateSlot(Inventory.Instance.inventory[i-I-1]);
           
        }

    }

    public void UpdateModSlot(ItemData_Equipment E) 
    {
        for (int i = 0; i < ModSlot.Length; i++)
        {
            ModSlot[i].CleanUpSlot();
        }
        if (E == null || E.Mods == null) return;


        int count = Mathf.Min(ModSlot.Length, E.Mods.Length);

        for (int i = 0; i < count; i++)
            ModSlot[i].UpdateSlot(E.Mods[i]);

    }

    public void invModSlot() 
    {
        for (int i = 0; i < modInventorySlots.Length; i++)
            modInventorySlots[i].CleanUpSlot();

        int slotIndex = 0;

        foreach (var invItem in Inventory.Instance.stash)
        {
            if (slotIndex >= modInventorySlots.Length) break;

            if (invItem.data != null && invItem.data.ItemType == ItemType.Mods)
            {
                modInventorySlots[slotIndex].UpdateSlot(invItem);
                slotIndex++;
            }
        }
    }

    }

