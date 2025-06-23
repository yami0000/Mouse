using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum EquipmentType
{
    Weapon,
    Armor,
    Amulet,
    item
}



[CreateAssetMenu(fileName = "New Item Name", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData

{
 


    public EquipmentType equipmentType;

    public ItemEffect[] equipmentEffects;

    [Header("Major Stats")]
    public int FirePower;
    public int Agility;
    public int Intelligence;
    public int Vitality;

    [Header("Damage")]
    public int Damage;
    public int CriticalChance;
    public int CriticalDamage;

    [Header("Defensive Stats")]
    public int MaxHP;
    public int Armor;
    //public int Evasion;
    public int FireResistance;
    public int FrostResistance;
    public int LightingResistance;
    public int PoisonResistance;

    [Header("Element")]
    public int FireDamage;
    public int FrostDamage;
    public int PoisonDamage;
    public int LightningDamage;

    public void ExecuteEquipmentEffect(Transform _position) 
    {
        foreach (var effect in equipmentEffects)
        { 
        effect.ExecuteEffect(  _position);
        }
    }
    public void AddModifiers() 
    {
       PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();

       playerStats.FirePower.AddModifier(FirePower);
      // playerStats.Agility.AddModifier(Agility);    
      // playerStats.Intelligence.AddModifier(Intelligence);
       playerStats.Vitality.AddModifier(Vitality); 

       playerStats.Damage.AddModifier(Damage);
       playerStats.CriticalChance.AddModifier(CriticalChance);
        playerStats.CriticalDamage.AddModifier(CriticalDamage);

        playerStats.MaxHP.AddModifier(MaxHP);
        playerStats.Armor.AddModifier(Armor);   
       // playerStats.Evasion.AddModifier(Evasion);
        playerStats.FireResistance.AddModifier(FireResistance);
        playerStats.FrostDamage.AddModifier(FrostDamage);
        playerStats.LightingResistance.AddModifier(LightingResistance);
        playerStats.PoisonResistance.AddModifier(PoisonResistance);

        playerStats.FireDamage.AddModifier(FireDamage);
        playerStats.FrostDamage.AddModifier(FrostDamage);
        playerStats.PoisonDamage.AddModifier(PoisonDamage);
        playerStats.LightningDamage.AddModifier(LightningDamage);


    }

    public void RemoveModifiers()
    {
        PlayerStats playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();


        playerStats.FirePower.RemoveModifier(FirePower);
        //playerStats.Agility.RemoveModifier(Agility);
        // playerStats.Intelligence.RemoveModifier(Intelligence);
        playerStats.Vitality.RemoveModifier(Vitality);

        playerStats.Damage.RemoveModifier(Damage);
        playerStats.CriticalChance.RemoveModifier(CriticalChance);
        playerStats.CriticalDamage.RemoveModifier(CriticalDamage);

        playerStats.MaxHP.RemoveModifier(MaxHP);
        playerStats.Armor.RemoveModifier(Armor);
        // playerStats.Evasion.RemoveModifier(Evasion);s
        playerStats.FireResistance.RemoveModifier(FireResistance);
        playerStats.FrostDamage.RemoveModifier(FrostDamage);
        playerStats.LightingResistance.RemoveModifier(LightingResistance);
        playerStats.PoisonResistance.RemoveModifier(PoisonResistance);

        playerStats.FireDamage.RemoveModifier(FireDamage);
        playerStats.FrostDamage.RemoveModifier(FrostDamage);
        playerStats.PoisonDamage.RemoveModifier(PoisonDamage);
        playerStats.LightningDamage.RemoveModifier(LightningDamage);










    }

}
