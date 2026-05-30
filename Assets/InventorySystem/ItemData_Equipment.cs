using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum EquipmentType
{
    Weapon,
    Armor,
    Amulet,
    Companion,
    item,
    Throwable
    
}

public enum Mechanism 
{
    common,
    charge


}

[CreateAssetMenu(fileName = "New Item Name", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData

{
    public EquipmentType equipmentType;

    public Mechanism mechanism;

    public int Level;

    public ItemEffect[] equipmentEffects;

    public Mod[] Mods;

    [Range(0, 6)]
    public int modSlotCount ;


    [Header("Major Stats")]
    public int sFirePower;
    public int Agility;
    //public int Intelligence;
    public int Vitality;

    [Header("Damage")]
    public int Damage;
    public int CriticalChance;
    public int CriticalDamage;

    [Header("Defensive Stats")]
    public int MaxHP;
    public int sArmor;
    public int FireResistance;
    public int FrostResistance;
    public int LightingResistance;
    public int PoisonResistance;

    [Header("Element")]
    public int sFireDamage;
    public int sFrostDamage;
    public int sPoisonDamage;
    public int sLightningDamage;
    
    [Header("If it's weapon")]//control ammo info in the inspector of equipment
    public float xVelocity;
    public float firingRate;
    public float effectiveTime;
    [Header("Stats can be influenced by level")]
    public int FirePower;
    public int Armor;
    public int FireDamage;
    public int FrostDamage;
    public int PoisonDamage;
    public int LightningDamage;

    public bool TryAddMod(Mod modItem)
    {
        

        if (modItem == null) return false;
        if (modItem.ItemType != ItemType.Mods) return false;

        bool isFull = true;
        for (int i = 0; i < Mods.Length; i++)
        {
            if (Mods[i] == null)
            {
                isFull = false;
                break;
            }
        }

        if (isFull) return false;


        for (int i = 0; i < Mods.Length; i++)
        {
            if (Mods[i] == null)
            {   
                Mods[i] = modItem;
                return true;
            }
            
        }

        return false; // no empty slot
    }
    public bool TryRemoveModAt(int index, out Mod removed)
    {
        removed = null;

        if (Mods == null || index < 0 || index >= Mods.Length) return false;
        if (Mods[index] == null) return false;

        removed = Mods[index];
        Mods[index] = null;
        return true;
    }
    private void OnValidate()//excute when data is changing in the inspector.
    {
        if (Mods == null || Mods.Length != modSlotCount)
        {
            System.Array.Resize(ref Mods, modSlotCount);
        }
        int maxVal = Mathf.Max(new int[] { sFireDamage, sFrostDamage, sPoisonDamage, sLightningDamage });

        FirePower = sFirePower + 5 * Level;

        if(equipmentType == EquipmentType.Armor)
        Armor = sArmor + 7 * Level;

        if (maxVal == 0)
        {
            FireDamage = FrostDamage = PoisonDamage = LightningDamage = 0;
            return;
        }

        FireDamage = sFireDamage + (sFireDamage == maxVal ? 4 * Level : 0);
        FrostDamage = sFrostDamage + (sFrostDamage == maxVal ? 4 * Level : 0);
        PoisonDamage = sPoisonDamage + (sPoisonDamage == maxVal ? 4 * Level : 0);
        LightningDamage = sLightningDamage + (sLightningDamage == maxVal ? 4 * Level : 0);
    }
    public void ExecuteEquipmentEffect(Transform _position,ItemData_Equipment data) 
    {
        foreach (var effect in equipmentEffects)
        { 
        effect.ExecuteWeaponEffect(_position,data);//实际上是子弹的效果
        }
    }
    public void AddModifiers() 
    {
       PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();

       playerStats.FirePower.AddModifier(FirePower);
    
       playerStats.Vitality.AddModifier(Vitality); 

       playerStats.Damage.AddModifier(Damage);
       playerStats.CriticalChance.AddModifier(CriticalChance);
        playerStats.CriticalDamage.AddModifier(CriticalDamage);

        playerStats.MaxHP.AddModifier(MaxHP);
        playerStats.Armor.AddModifier(Armor);   
  
        playerStats.FireResistance.AddModifier(FireResistance);
        playerStats.FrostDamage.AddModifier(FrostDamage);
        playerStats.LightingResistance.AddModifier(LightingResistance);
        playerStats.PoisonResistance.AddModifier(PoisonResistance);

        playerStats.FireDamage.AddModifier(FireDamage);
        playerStats.FrostDamage.AddModifier(FrostDamage);
        playerStats.PoisonDamage.AddModifier(PoisonDamage);
        playerStats.LightningDamage.AddModifier(LightningDamage);

        if (Mods != null)
        {
            foreach (var mod in Mods)
            {
                if (mod == null) continue;
                mod.AddModifiers(playerStats);
            }
        }
    }

    public void RemoveModifiers()
    {
        PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();


        playerStats.FirePower.RemoveModifier(FirePower);
       
        playerStats.Vitality.RemoveModifier(Vitality);

        playerStats.Damage.RemoveModifier(Damage);
        playerStats.CriticalChance.RemoveModifier(CriticalChance);
        playerStats.CriticalDamage.RemoveModifier(CriticalDamage);

        playerStats.MaxHP.RemoveModifier(MaxHP);
        playerStats.Armor.RemoveModifier(Armor);
        
        playerStats.FireResistance.RemoveModifier(FireResistance);
        playerStats.FrostDamage.RemoveModifier(FrostDamage);
        playerStats.LightingResistance.RemoveModifier(LightingResistance);
        playerStats.PoisonResistance.RemoveModifier(PoisonResistance);

        playerStats.FireDamage.RemoveModifier(FireDamage);
        playerStats.FrostDamage.RemoveModifier(FrostDamage);
        playerStats.PoisonDamage.RemoveModifier(PoisonDamage);
        playerStats.LightningDamage.RemoveModifier(LightningDamage);

        if (Mods != null)
        {
            foreach (var mod in Mods)
            {
                if (mod == null) continue;
                mod.RemoveModifiers(playerStats);
            }
        }








    }

}
