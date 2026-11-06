using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A mechanism a mod adds to the equipment it's installed on. It lives inside the Mod asset:
/// pick its type in the Mod's Effects list, no separate asset needed. Override the hooks it needs;
/// ModEffects calls them for the mods on the worn weapon and armor.
/// New mechanisms are a new subclass, marked [Serializable]; it shows up in the dropdown on its own.
/// </summary>
[Serializable]
public abstract class ModEffect
{
    /// <summary>The weapon fired once: one pull of the trigger, however many bullets it shoots.</summary>
    public virtual void OnShot(ItemData_Equipment _weapon) { }

    /// <summary>
    /// A player bullet is about to start flying: the weapon's own, a spread pellet, or an extra one
    /// from a mod. Change how it behaves here (homing, piercing...).
    /// </summary>
    public virtual void OnAmmoFired(ItemData_Equipment _equipment, AmmoBehaviour _ammo) { }

    /// <summary>A player bullet hit _target for _damage.</summary>
    public virtual void OnHit(ItemData_Equipment _equipment, EntityStats _target, int _damage) { }

    /// <summary>A player bullet is being destroyed, and why: timed out, hit the ground, hit something...</summary>
    public virtual void OnAmmoEnded(ItemData_Equipment _equipment, AmmoBehaviour _ammo, AmmoEndReason _reason) { }

    /// <summary>Every frame (LateUpdate, after the weapon has turned to the aim) while _equipment is worn.</summary>
    public virtual void OnWornUpdate(ItemData_Equipment _equipment) { }

    /// <summary>_equipment was taken off or this mod uninstalled: clean up whatever OnWornUpdate made.</summary>
    public virtual void OnWornStopped(ItemData_Equipment _equipment) { }

    /// <summary>The weapon the player has on, or null.</summary>
    protected static ItemData_Equipment WornWeapon =>
        Inventory.Instance != null && Inventory.Instance.equipmentDictionary != null
            ? Inventory.Instance.GetEquipment(EquipmentType.Weapon)
            : null;

    /// <summary>The worn weapon's Damage, its installed mods included. 0 without a weapon.</summary>
    protected static float WeaponDamage()
    {
        ItemData_Equipment weapon = WornWeapon;
        return weapon != null && weapon.TryGetTooltipNumber(TooltipField.Damage, out float damage) ? damage : 0f;
    }

    /// <summary>The player's stats, which mod damage is dealt from.</summary>
    protected static PlayerStats OwnerStats => PlayerManager.Instance.player.GetComponent<PlayerStats>();

    /// <summary>The enemy behind _collider, or null when it isn't one.</summary>
    protected static EnemyStats EnemyOf(Collider2D _collider) =>
        _collider.GetComponent<Enemy>() != null ? _collider.GetComponent<EnemyStats>() : null;

    /// <summary>_layers, or the Enemy layer when nothing is picked.</summary>
    protected static LayerMask OrEnemyLayer(LayerMask _layers) =>
        _layers.value != 0 ? _layers : (LayerMask)LayerMask.GetMask("Enemy");

    [NonSerialized] private Dictionary<ItemData_Equipment, object> states;

    /// <summary>
    /// Memory this effect keeps per equipment, such as a shot counter. The Mod asset is shared, so the
    /// same mod on two weapons would otherwise count together.
    /// </summary>
    protected T StateFor<T>(ItemData_Equipment _equipment) where T : class, new()
    {
        states ??= new Dictionary<ItemData_Equipment, object>();

        if (!states.TryGetValue(_equipment, out object state) || state is not T typed)
        {
            typed = new T();
            states[_equipment] = typed;
        }

        return typed;
    }
}

/// <summary>
/// Calls the installed mods' effects at the right moments: shots from PlayerReadyToAttack.FireWeapon,
/// bullets and hits from AmmoBehaviour, every frame from ModEffectRunner. Only the worn weapon and armor count.
/// </summary>
public static class ModEffects
{
    // Effects that got OnWornUpdate last frame, so the ones that stop can be told.
    private static HashSet<(ModEffect, ItemData_Equipment)> running = new HashSet<(ModEffect, ItemData_Equipment)>();
    private static HashSet<(ModEffect, ItemData_Equipment)> seen = new HashSet<(ModEffect, ItemData_Equipment)>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Hook()
    {
        // Removed first, in case statics survive between plays (Enter Play Mode Options).
        AmmoBehaviour.PlayerAmmoFired -= AmmoFired;
        AmmoBehaviour.PlayerAmmoFired += AmmoFired;
        AmmoBehaviour.PlayerAmmoHit -= AmmoHit;
        AmmoBehaviour.PlayerAmmoHit += AmmoHit;
        AmmoBehaviour.PlayerAmmoEnded -= AmmoEnded;
        AmmoBehaviour.PlayerAmmoEnded += AmmoEnded;

        running.Clear();
        seen.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRunner()
    {
        GameObject runner = new GameObject("Mod Effects");
        runner.AddComponent<ModEffectRunner>();
        UnityEngine.Object.DontDestroyOnLoad(runner);
    }

    /// <summary>Where effects put objects that last while worn (the laser's line), so they survive scene changes.</summary>
    public static Transform Root { get; private set; }

    internal static void SetRoot(Transform _root) => Root = _root;

    /// <summary>OnWornUpdate for everything worn, then OnWornStopped for whatever stopped being worn.</summary>
    internal static void LateTick()
    {
        seen.Clear();

        foreach (ItemData_Equipment equipment in Worn())
            Call(equipment, effect =>
            {
                seen.Add((effect, equipment));
                effect.OnWornUpdate(equipment);
            });

        foreach ((ModEffect effect, ItemData_Equipment equipment) in running)
            if (!seen.Contains((effect, equipment)))
                Run(effect, null, e => e.OnWornStopped(equipment));

        (running, seen) = (seen, running);
    }

    /// <summary>Everything still running is told to stop, e.g. when the runner goes away.</summary>
    internal static void StopAll()
    {
        foreach ((ModEffect effect, ItemData_Equipment equipment) in running)
            Run(effect, null, e => e.OnWornStopped(equipment));

        running.Clear();
    }

    private static void AmmoEnded(AmmoBehaviour _ammo, AmmoEndReason _reason)
    {
        foreach (ItemData_Equipment equipment in Worn())
            Call(equipment, effect => effect.OnAmmoEnded(equipment, _ammo, _reason));
    }

    public static void Shot(ItemData_Equipment _weapon) => Call(_weapon, effect => effect.OnShot(_weapon));

    private static void AmmoFired(AmmoBehaviour _ammo)
    {
        foreach (ItemData_Equipment equipment in Worn())
            Call(equipment, effect => effect.OnAmmoFired(equipment, _ammo));
    }

    private static void AmmoHit(AmmoBehaviour _ammo, EntityStats _target, int _damage)
    {
        foreach (ItemData_Equipment equipment in Worn())
            Call(equipment, effect => effect.OnHit(equipment, _target, _damage));
    }

    private static IEnumerable<ItemData_Equipment> Worn()
    {
        Inventory inventory = Inventory.Instance;
        if (inventory == null || inventory.equipmentDictionary == null) yield break;

        ItemData_Equipment weapon = inventory.GetEquipment(EquipmentType.Weapon);
        if (weapon != null) yield return weapon;

        ItemData_Equipment armor = inventory.GetEquipment(EquipmentType.Armor);
        if (armor != null) yield return armor;
    }

    /// <summary>Runs _call on every effect of every mod on _equipment. One broken effect doesn't stop the rest.</summary>
    private static void Call(ItemData_Equipment _equipment, Action<ModEffect> _call)
    {
        if (_equipment == null || _equipment.Mods == null) return;

        foreach (Mod mod in _equipment.Mods)
        {
            if (mod == null || mod.effects == null) continue;

            foreach (ModEffect effect in mod.effects)
                if (effect != null)
                    Run(effect, mod, _call);
        }
    }

    private static void Run(ModEffect _effect, Mod _mod, Action<ModEffect> _call)
    {
        try
        {
            _call(_effect);
        }
        catch (Exception e)
        {
            Debug.LogException(e, _mod);
        }
    }
}
