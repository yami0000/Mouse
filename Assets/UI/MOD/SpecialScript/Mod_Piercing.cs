using System;
using UnityEngine;

/// <summary>Bullets go through enemies instead of stopping at the first one, hitting up to Extra Targets more.</summary>
[Serializable]
public class Mod_Piercing : ModEffect
{
    [Tooltip("How many more enemies each bullet can hit after the first. Each enemy is only hit once.")]
    [Min(1)]
    [SerializeField] private int extraTargets = 2;

    public override void OnAmmoFired(ItemData_Equipment _equipment, AmmoBehaviour _ammo)
    {
        // Grenades and other Spawn Only ammo should still go off on contact.
        if (_ammo.impactMode != AmmoImpactMode.Damage) return;

        _ammo.hitOnce = false;
        _ammo.destroyOnHit = false;

        int hits = 0;
        _ammo.OnTargetAffected += (ammo, target) =>
        {
            hits++;
            if (hits > extraTargets && ammo != null)
                ammo.DestroyAmmo(AmmoEndReason.Target);
        };
    }
}
