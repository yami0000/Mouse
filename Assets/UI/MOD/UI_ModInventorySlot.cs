using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI;

public class UI_ModInventorySlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI stackText; // optional

    public InventoryItem item { get; private set; }

    public void CleanUpSlot()
    {
        item = null;
        icon.sprite = null;
        icon.color = Color.clear;
        if (stackText != null) stackText.text = "";
    }

    public void UpdateSlot(InventoryItem newItem)
    {
        item = newItem;

        if (item == null || item.data == null)
        {
            CleanUpSlot();
            return;
        }

        icon.color = Color.white;
        icon.sprite = item.data.icon;

        if (stackText != null)
            stackText.text = item.stackSize > 1 ? item.stackSize.ToString() : "";
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();

        if (item == null || item.data == null)
        {

            return;
        }
        Mod E = item.data as Mod;

         
        bool added = UpgradePanel.Instance.Equipment.TryAddMod(E);
        if (!added || UpgradePanel.Instance.Equipment == null)
            return;

        bool isEquipped = Inventory.Instance.equipmentDictionary.ContainsKey(UpgradePanel.Instance.Equipment);//ÒÆ³ýÊýÖµ
        if (isEquipped)
            E.AddModifiers(playerStats);

        Inventory.Instance.RemoveItem(E);
        UpgradeSystem.Instance.UpdateModSlot(UpgradePanel.Instance.Equipment);
        UpgradePanel.Instance.upgradeData(UpgradePanel.Instance.Equipment);

    }
    }
