using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_modsEquipped : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image itemImage;
    public Mod mod;

    public void CleanUpSlot()
    {
        mod = null;
        itemImage.sprite = null;
        itemImage.color = Color.clear;
    }


    public void UpdateSlot(Mod newMod)
    {
        mod = newMod;

        if (mod == null)
        {
            CleanUpSlot();
            return;
        }

        itemImage.color = Color.white;
        itemImage.sprite = mod.icon;
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();


        var panel = UpgradePanel.Instance;
        if (panel == null || panel.Equipment == null) return;

        int index = transform.GetSiblingIndex(); // index of clicked slot in the UI
        bool removed = panel.Equipment.TryRemoveModAt(index, out Mod removedMod);
        if (!removed) return;

        bool isEquipped = Inventory.Instance.equipmentDictionary.ContainsKey(panel.Equipment);//ÒÆ³ýÊýÖµ
        if (isEquipped)
            removedMod.RemoveModifiers(playerStats);
           
        Inventory.Instance.AddItem(removedMod);

        UpgradeSystem.Instance.UpdateModSlot(panel.Equipment);
        panel.upgradeData(panel.Equipment);

         
    }
}

