using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName ="Mod",menuName ="Data/Mod/ModEffect")]
public class Mod : ItemData
{
    
    [Header("Stat bonuses from this Mod")]
    public int FirePower;
    public int Vitality;
    public int Damage;
    public int CriticalChance;
    public int CriticalDamage;
    public int MaxHP;
    public int Armor;

    public int FireResistance;
    public int FrostResistance;
    public int LightingResistance;
    public int PoisonResistance;

    public int FireDamage;
    public int FrostDamage;
    public int PoisonDamage;
    public int LightningDamage;

    public void AddModifiers(PlayerStats playerStats)
    {
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
    }

    public void RemoveModifiers(PlayerStats playerStats)
    {
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
    }
}
