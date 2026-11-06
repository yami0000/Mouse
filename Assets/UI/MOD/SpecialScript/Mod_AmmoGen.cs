using System;
using UnityEngine;

/// <summary>Every Shot Count To Trigger shots, fires an extra bullet (and any extra effects) from the modded weapon.</summary>
[Serializable]
public class Mod_AmmoGen : ModEffect
{
    [Min(1)]
    [SerializeField] private int shotCountToTrigger = 3;

    [Tooltip("The extra bullet, fired from the weapon's muzzle along the aim with its X Velocity and Effective Time.")]
    [SerializeField] private GameObject ammoPrefab;

    [Tooltip("Extra effects fired alongside it, e.g. a Shotgun spread.")]
    public ItemEffect[] equipmentEffects;

    private class Counter
    {
        public int shots;
    }

    public override void OnShot(ItemData_Equipment _weapon)
    {
        Counter counter = StateFor<Counter>(_weapon);
        counter.shots++;

        if (counter.shots < shotCountToTrigger) return;

        counter.shots = 0;

        _weapon.FireAmmo(ammoPrefab, PlayerWeaponHolder.Instance.WeaponTransform);

        if (equipmentEffects == null) return;

        Transform player = PlayerManager.Instance.player.transform;
        foreach (ItemEffect effect in equipmentEffects)
            if (effect != null)
                effect.ExecuteWeaponEffect(player, _weapon);
    }
}
