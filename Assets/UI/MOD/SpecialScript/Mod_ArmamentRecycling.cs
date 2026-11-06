using System;
using UnityEngine;

/// <summary>
/// A bullet that times out (reaches its full range) or hits the ground has a Chance to be fired again
/// from the weapon straight away, along the current aim. It's free: no click, no attack, no cooldown,
/// and shot-counting mods such as Ammo Gen don't count it.
/// </summary>
[Serializable]
public class Mod_ArmamentRecycling : ModEffect
{
    [Tooltip("Chance a spent bullet comes back. 0.3 = 30%.")]
    [Range(0f, 1f)]
    [SerializeField] private float chance = 0.3f;

    [Tooltip("How many times one bullet can come back in a row. 0 = no limit.")]
    [Min(0)]
    [SerializeField] private int maxRepeats = 3;

    [Tooltip("Play the weapon's Fire Effects (muzzle flash...) when it comes back. No recoil either way.")]
    [SerializeField] private bool playFireEffects = true;

    public override void OnAmmoEnded(ItemData_Equipment _equipment, AmmoBehaviour _ammo, AmmoEndReason _reason)
    {
        if (_reason != AmmoEndReason.Expired && _reason != AmmoEndReason.Ground) return;
        if (maxRepeats > 0 && _ammo.recycledCount >= maxRepeats) return;
        if (UnityEngine.Random.value >= chance) return;

        PlayerWeaponHolder holder = PlayerWeaponHolder.Instance;
        ItemData_Equipment weapon = WornWeapon;
        if (holder == null || !holder.isHolding || weapon == null) return;

        // The same bullet again (a Shotgun pellet stays a pellet); the weapon's own when its spawner didn't say.
        GameObject prefab = _ammo.sourcePrefab != null ? _ammo.sourcePrefab : weapon.ammoPrefab;

        GameObject ammo = weapon.FireAmmo(prefab, holder.WeaponTransform);
        if (ammo == null) return;

        if (ammo.TryGetComponent(out AmmoBehaviour copy))
            copy.recycledCount = _ammo.recycledCount + 1;

        if (playFireEffects)
            holder.PlayFireFX(weapon);
    }
}
