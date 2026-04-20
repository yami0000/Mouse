using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour//UIÊÓ¾õ
{
 public static UpgradePanel Instance;

    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private TextMeshProUGUI DPS;
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private TextMeshProUGUI CC;
    [SerializeField] private TextMeshProUGUI CD;
    [SerializeField] private TextMeshProUGUI Fi;
    [SerializeField] private TextMeshProUGUI Fr;
    [SerializeField] private TextMeshProUGUI Po;
    [SerializeField] private TextMeshProUGUI Li;
    [HideInInspector] public ItemData_Equipment Equipment;
    public Image Image;
    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        Image.sprite = null;
        Image.color = Color.clear;
        Equipment = null;
        Initialize();

    }

    public void Initialize()
    {
        if (GetFirstEquipment() != null)
        {
            Equipment = GetFirstEquipment();
            upgradeVisual(Equipment);
            upgradeData(Equipment);
            UpgradeSystem.Instance.UpdateModSlot(Equipment);
        }
    }

    private ItemData_Equipment GetFirstEquipment()
    {
        if (Inventory.Instance == null || Inventory.Instance.inventoryDictionary == null)
            return null;

        foreach (var kvp in Inventory.Instance.inventoryDictionary)
        {
            if (kvp.Key is ItemData_Equipment equipment)
            {
                return equipment;
            }
        }

        return null;
    }

    public void upgradeVisual(ItemData_Equipment E) 
    {
    Equipment = E;//
    Image equipment = Image.GetComponent<Image>();  
    
    if(equipment != null)
        {
            equipment.sprite = E.icon;
            equipment.color = Color.white;  
        }
    
    }

    public void upgradeData(ItemData_Equipment E) 
    {
        int firePower = E.FirePower;
        int damage = E.Damage;
        int critChance = E.CriticalChance;
        int critDamage = E.CriticalDamage;
        int FiDamage = E.FireDamage;
        int FrDamage = E.FrostDamage;
        int PoDamage = E.PoisonDamage;
        int LiDamage = E.LightningDamage;

        if (E.Mods != null)
        {
            foreach (var m in E.Mods)
            {
                if (m == null) continue;

                firePower += m.FirePower;
                damage += m.Damage;
                critChance += m.CriticalChance;
                critDamage += m.CriticalDamage;
                FiDamage += m.FireDamage;
                FrDamage += m.FrostDamage;
                PoDamage += m.PoisonDamage;
                LiDamage += m.LightningDamage;
            }
        }

        WeaponName.text = E.itemName;
        Level.text = E.Level.ToString();
        float fr = Mathf.Max(0.0001f, E.firingRate); // avoid divide-by-zero
        DPS.text = ((int)((firePower + damage) / fr)).ToString();
        CC.text = critChance.ToString() + "%";
        CD.text = critDamage.ToString() + "%";
        Fi.text = FiDamage.ToString();
        Fr.text = FrDamage.ToString();
        Po.text = PoDamage.ToString();
        Li.text = LiDamage.ToString();
    }
}
