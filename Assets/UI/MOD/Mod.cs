using UnityEngine;

public enum _Type
{
common,
special

}

// Created from Assets > Create > Data Asset... (see Editor/DataAssetMenu.cs).
public class Mod : ItemData
{
    public _Type type;

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

    [Header("Weapon handling")]
    [Tooltip("Multiplies the weapon's X Velocity. 0 = unchanged.")]
    public float xVelocity;
    [Tooltip("Multiplies the weapon's Firing Rate (the delay between shots). 0 = unchanged.")]
    public float firingRate;
    [Tooltip("Seconds added to the weapon's Effective Time.")]
    public float effectiveTime;


    public void ApplyModEffect(ItemData_Equipment equip)
    {
        if (itemEffects == null) return;

        foreach (var effect in itemEffects)
            effect.ExecuteModEffect(PlayerManager.Instance.player.transform, equip);
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

        Apply(playerStats.FirePower, FirePower);
        Apply(playerStats.Vitality, Vitality);

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
