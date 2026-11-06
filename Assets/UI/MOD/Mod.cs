using System.Collections.Generic;
using UnityEngine;

public enum _Type
{
common,
special

}

// Created from Assets > Create > Data Asset... (see Editor/DataAssetMenu.cs).
public class Mod : ItemData
{
    // Set by OnValidate: special when the mod has Effects. Hidden in the Inspector (see ModEditor).
    public _Type type;

    [Header("Stat bonuses from this Mod")]
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

    [Header("Weapon handling")]
    [Tooltip("Multiplies the weapon's X Velocity. 0 = unchanged.")]
    public float xVelocity;
    [Tooltip("Multiplies the weapon's Firing Rate (the delay between shots). 0 = unchanged.")]
    public float firingRate;
    [Tooltip("Seconds added to the weapon's Effective Time.")]
    public float effectiveTime;

    [Header("Special Effects")]
    [Tooltip("Mechanisms this mod adds to the weapon it's installed on. Press +, then pick the type from the " +
             "dropdown on the right; its settings open below it. Any effect makes the mod Special.")]
    [SerializeReference] public List<ModEffect> effects = new List<ModEffect>();

    private void OnValidate()
    {
        // A mod is always a Mod; the Inspector hides the field (see ModEditor).
        ItemType = ItemType.Mods;

        type = _Type.common;
        if (effects != null)
            foreach (ModEffect effect in effects)
                if (effect != null)
                    type = _Type.special;
    }

    public override string GetTooltipText(TooltipField _field)
    {
        if (_field == TooltipField.Type)
            return $"Mod  {type}";

        return base.GetTooltipText(_field);
    }

    public override bool TryGetTooltipNumber(TooltipField _field, out float _value)
    {
        switch (_field)
        {
            case TooltipField.Damage: _value = Damage; return true;
            case TooltipField.CriticalChance: _value = CriticalChance; return true;
            case TooltipField.CriticalDamage: _value = CriticalDamage; return true;
            case TooltipField.MaxHP: _value = MaxHP; return true;
            case TooltipField.Armor: _value = Armor; return true;
            case TooltipField.FireResistance: _value = FireResistance; return true;
            case TooltipField.FrostResistance: _value = FrostResistance; return true;
            case TooltipField.LightningResistance: _value = LightingResistance; return true;
            case TooltipField.PoisonResistance: _value = PoisonResistance; return true;
            case TooltipField.FireDamage: _value = FireDamage; return true;
            case TooltipField.FrostDamage: _value = FrostDamage; return true;
            case TooltipField.PoisonDamage: _value = PoisonDamage; return true;
            case TooltipField.LightningDamage: _value = LightningDamage; return true;

            // Weapon handling, as ApplyWeaponModifiers changes it. 0 = unchanged.
            case TooltipField.BulletSpeedMultiplier: _value = xVelocity; return true;
            case TooltipField.ShotDelayMultiplier: _value = firingRate; return true;
            case TooltipField.BulletLifetimeBonus: _value = effectiveTime; return true;

            default: return base.TryGetTooltipNumber(_field, out _value);
        }
    }

    public void AddModifiers(PlayerStats playerStats) => ApplyModifiers(playerStats, true);
    public void RemoveModifiers(PlayerStats playerStats) => ApplyModifiers(playerStats, false);

    // One list for both directions, so adding and removing can never drift apart.
    private void ApplyModifiers(PlayerStats playerStats, bool _add)
    {
        void Apply(Stats _stat, int _value)
        {
            if (_add) _stat.AddModifier(_value);
            else _stat.RemoveModifier(_value);
        }

        Apply(playerStats.Damage, Damage);
        Apply(playerStats.CriticalChance, CriticalChance);
        Apply(playerStats.CriticalDamage, CriticalDamage);

        Apply(playerStats.MaxHP, MaxHP);
        Apply(playerStats.Armor, Armor);

        Apply(playerStats.FireResistance, FireResistance);
        Apply(playerStats.FrostResistance, FrostResistance);
        Apply(playerStats.LightingResistance, LightingResistance);
        Apply(playerStats.PoisonResistance, PoisonResistance);

        Apply(playerStats.FireDamage, FireDamage);
        Apply(playerStats.FrostDamage, FrostDamage);
        Apply(playerStats.PoisonDamage, PoisonDamage);
        Apply(playerStats.LightningDamage, LightningDamage);
    }

    /// <summary>Changes how the weapon shoots. Undone by RemoveWeaponModifiers.</summary>
    public void ApplyWeaponModifiers(ItemData_Equipment equip)
    {
        if (xVelocity != 0)
            equip.xVelocity *= xVelocity;
        if (firingRate != 0)
            equip.firingRate *= firingRate;
        equip.effectiveTime += effectiveTime;

    }

    public void RemoveWeaponModifiers(ItemData_Equipment equip)
    {
        if (xVelocity != 0)
            equip.xVelocity /= xVelocity;
        if (firingRate != 0)
            equip.firingRate /= firingRate;
        equip.effectiveTime -= effectiveTime;

    }
}
