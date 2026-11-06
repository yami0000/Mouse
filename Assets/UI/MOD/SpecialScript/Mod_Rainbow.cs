using System;
using UnityEngine;

/// <summary>
/// Every bullet gets one random element, fire, frost, poison or lightning (1 in 4 each), worth Share
/// of the weapon's Damage, added to its hits.
/// </summary>
[Serializable]
public class Mod_Rainbow : ModEffect
{
    [Tooltip("Element damage added to each bullet, as a share of the weapon's Damage. 0.3 = 30%.")]
    [Min(0)]
    [SerializeField] private float share = 0.3f;

    [Tooltip("Tint each bullet with its element's color below.")]
    [SerializeField] private bool tintAmmo = true;

    [SerializeField] private Color fireColor = new Color(1f, 0.45f, 0.15f);
    [SerializeField] private Color frostColor = new Color(0.45f, 0.85f, 1f);
    [SerializeField] private Color poisonColor = new Color(0.55f, 1f, 0.3f);
    [SerializeField] private Color lightningColor = new Color(1f, 0.95f, 0.35f);

    public override void OnAmmoFired(ItemData_Equipment _equipment, AmmoBehaviour _ammo)
    {
        int amount = Mathf.RoundToInt(WeaponDamage() * share);
        if (amount <= 0) return;

        Color color;

        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0: _ammo.elementBonus.fire += amount; color = fireColor; break;
            case 1: _ammo.elementBonus.frost += amount; color = frostColor; break;
            case 2: _ammo.elementBonus.poison += amount; color = poisonColor; break;
            default: _ammo.elementBonus.lightning += amount; color = lightningColor; break;
        }

        if (!tintAmmo) return;

        SpriteRenderer sprite = _ammo.sp != null ? _ammo.sp : _ammo.GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
            sprite.color = color;
    }
}
