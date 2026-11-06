using System.Collections.Generic;
using UnityEngine;

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

/// <summary>
/// How hard a weapon shoves the player back when it fires. The push runs against the shot,
/// speeds up over Acceleration Time, slows back to a stop over Deceleration Time, and never
/// turns the player around.
/// </summary>
[System.Serializable]
public class WeaponRecoil
{
    [Tooltip("Top speed the player is pushed back at. 0 = no recoil.")]
    [Min(0)] public float force;
    [Tooltip("Seconds to build up to Force after the shot. 0 = full force at once.")]
    [Min(0)] public float accelerationTime = 0.05f;
    [Tooltip("Seconds to slow from Force back to a stop.")]
    [Min(0)] public float decelerationTime = 0.2f;
    [Tooltip("Push sideways only. Off = straight back along the shot, so firing downwards lifts the player.")]
    public bool horizontalOnly = true;

    public float Duration => accelerationTime + decelerationTime;
    public bool Enabled => force > 0f && Duration > 0f;

    /// <summary>Push speed _time seconds after the shot: ramps up to Force, then back down to 0.</summary>
    public float SpeedAt(float _time)
    {
        if (_time < accelerationTime)
            return force * (_time / accelerationTime);

        if (decelerationTime <= 0f)
            return 0f;

        return force * Mathf.Clamp01(1f - (_time - accelerationTime) / decelerationTime);
    }
}

/// <summary>
/// An effect spawned on the held weapon each time it fires, e.g. a muzzle flash.
/// It turns exactly as the weapon does, clamp and left/right flip included. Offset is laid out
/// on the weapon before it is turned, so the FX stays on the barrel at any angle.
/// </summary>
[System.Serializable]
public class WeaponFireFX
{
    [Tooltip("The effect prefab: particles, an animated sprite, anything. It should point right (+X) by default, " +
             "e.g. through the particles' Start Rotation. Empty = no FX.")]
    public GameObject prefab;
    // Ticked off with the box in the entry's header. Stored as "off" so existing entries stay on.
    [HideInInspector] public bool disabled;
    [Tooltip("Where it appears on the weapon before the weapon's rotation is applied: X along the barrel, Y above it. In the weapon's local units.")]
    public Vector2 offset;
    [Tooltip("Degrees added on top of the weapon's rotation. Use it when the prefab fires along another axis, e.g. -90 for one made to fire upwards (+Y).")]
    public float rotation;
    [Tooltip("Multiplies the prefab's own scale.")]
    public Vector3 scale = Vector3.one;
    [Tooltip("Moves and turns with the weapon. Off = stays where it appeared.")]
    public bool followWeapon;
    [Tooltip("Seconds until it is destroyed. 0 = once its animation / particles have played through.")]
    [Min(0)] public float lifetime;
    [Tooltip("Turn camera-facing particles (Render Alignment View / World / Facing) with the weapon. " +
             "Off = they keep facing the screen as the prefab sets them, ignoring the aim.")]
    public bool turnParticles = true;

    public void Spawn(Transform _weapon)
    {
        if (disabled || prefab == null || _weapon == null)
            return;

        // The weapon's own rotation: PlayerWeaponHolder has already clamped and flipped it.
        Quaternion weaponRotation = _weapon.rotation;

        // Offset first, in the weapon's unturned frame, then turned with the weapon around its pivot.
        Vector3 localOffset = Vector3.Scale(offset, _weapon.lossyScale);
        Vector3 position = _weapon.position + weaponRotation * localOffset;
        Quaternion facing = weaponRotation * Quaternion.Euler(0f, 0f, rotation);

        GameObject fx = Object.Instantiate(prefab, position, facing);
        fx.transform.localScale = Vector3.Scale(fx.transform.localScale, scale);

        if (turnParticles)
            AlignParticlesToTransform(fx);

        // Parent only after placing it, keeping its world size: the weapon is scaled to 0.1,
        // which would otherwise shrink a following FX to a tenth.
        if (followWeapon)
            fx.transform.SetParent(_weapon, true);

        Object.Destroy(fx, lifetime > 0f ? lifetime : StateFX.OneLoopLength(fx, 1f));
    }

    /// <summary>
    /// Camera-facing particles ignore their transform's rotation, so a muzzle flash would point the
    /// same way whatever the aim. Lined up with the FX's transform instead, they turn and flip with
    /// the weapon, and the prefab's Start Rotation becomes the FX's default direction.
    /// Only this spawned copy changes; the prefab is left alone.
    /// </summary>
    private static void AlignParticlesToTransform(GameObject _fx)
    {
        foreach (ParticleSystemRenderer particles in _fx.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            // Stretched and horizontal/vertical billboards are oriented by velocity or the ground, not rotation.
            bool usesRotation = particles.renderMode == ParticleSystemRenderMode.Billboard
                             || particles.renderMode == ParticleSystemRenderMode.Mesh;

            bool facesCamera = particles.alignment == ParticleSystemRenderSpace.View
                            || particles.alignment == ParticleSystemRenderSpace.World
                            || particles.alignment == ParticleSystemRenderSpace.Facing;

            if (usesRotation && facesCamera)
                particles.alignment = ParticleSystemRenderSpace.Local;
        }
    }
}

// Created from Assets > Create > Data Asset... (see Editor/DataAssetMenu.cs).
public class ItemData_Equipment : ItemData

{
    public EquipmentType equipmentType;

    public Mechanism mechanism;

    public int Level;

    public ItemEffect[] equipmentEffects;

    public Mod[] Mods;

    [Range(0, 6)]
    public int modSlotCount ;


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
    [Tooltip("Fired straight along the aim on every shot, at X Velocity, gone after Effective Time. " +
             "Leave empty when an Equipment Effect does the firing instead (Shotgun spread, Grenade arc).")]
    public GameObject ammoPrefab;
    [Tooltip("The muzzle: where ammo leaves the weapon, on the weapon before its rotation is applied. " +
             "X along the barrel, Y above it, in the weapon's local units (same as Fire Effects offsets). 0 = the weapon's pivot.")]
    public Vector2 ammoOffset;
    public float xVelocity;
    public float firingRate;
    public float effectiveTime;
    public WeaponRecoil recoil = new WeaponRecoil();
    [Tooltip("Spawned on the held weapon every time it fires (muzzle flash, smoke, sparks...). Add as many as needed.")]
    public List<WeaponFireFX> fireEffects = new List<WeaponFireFX>();

    [Header("Stats can be influenced by level")]
    public int Armor;
    public int FireDamage;
    public int FrostDamage;
    public int PoisonDamage;
    public int LightningDamage;

    /// <summary>The asset this copy was made from. Null on the asset itself.</summary>
    [System.NonSerialized] public ItemData_Equipment source;
    public bool IsRuntimeCopy => source != null;

    /// <summary>
    /// A play-session copy of this asset. Installing mods and the weapon tweaks they bring change
    /// the copy only, so in the Editor nothing carries over once play mode stops.
    /// </summary>
    public ItemData_Equipment CreateRuntimeCopy()
    {
        ItemData_Equipment copy = Instantiate(this);
        copy.name = name;
        copy.source = this;
        return copy;
    }

    private bool IsEquipped => Inventory.Instance != null
                            && Inventory.Instance.equipmentDictionary != null
                            && Inventory.Instance.equipmentDictionary.ContainsKey(this);

    /// <summary>
    /// Puts the mod in slot _index, or the first free slot when _index is -1. False when it isn't a mod,
    /// the slot doesn't exist or is taken, or (with -1) every slot is taken.
    /// </summary>
    public bool TryAddMod(Mod modItem, int _index = -1)
    {
        if (modItem == null || modItem.ItemType != ItemType.Mods || Mods == null) return false;

        if (_index >= 0)
        {
            if (_index >= Mods.Length || Mods[_index] != null) return false;

            Mods[_index] = modItem;
            return true;
        }

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

    /// <summary>
    /// Slots the mod in (slot _index, or the first free one for -1) and applies it: its weapon handling
    /// straight away, its stats only while this equipment is worn (equipping applies them otherwise).
    /// False when nothing changed.
    /// </summary>
    public bool InstallMod(Mod _mod, int _index = -1)
    {
        if (!TryAddMod(_mod, _index)) return false;

        if (IsEquipped)
            _mod.AddModifiers(GetPlayerStats());

        _mod.ApplyWeaponModifiers(this);
        return true;
    }

    /// <summary>Takes the mod out of slot _index and undoes everything InstallMod applied.</summary>
    public bool UninstallModAt(int _index, out Mod _removed)
    {
        if (!TryRemoveModAt(_index, out _removed)) return false;

        if (IsEquipped)
            _removed.RemoveModifiers(GetPlayerStats());

        _removed.RemoveWeaponModifiers(this);
        return true;
    }

    /// <summary>
    /// Swaps the mods in slots _from and _to (either may be empty). Nothing is applied or undone:
    /// the same mods stay installed. False when a slot doesn't exist or nothing would move.
    /// </summary>
    public bool MoveMod(int _from, int _to)
    {
        if (Mods == null || _from == _to) return false;
        if (_from < 0 || _from >= Mods.Length || _to < 0 || _to >= Mods.Length) return false;
        if (Mods[_from] == null && Mods[_to] == null) return false;

        (Mods[_from], Mods[_to]) = (Mods[_to], Mods[_from]);
        return true;
    }

    /// <summary>
    /// Damage per second: Damage, installed mods included, over the delay between shots
    /// (which mods have already changed). 0 when it doesn't fire.
    /// </summary>
    public float Dps => firingRate > 0f
        ? StatWithMods(TooltipField.Damage) / firingRate
        : 0f;

    private float StatWithMods(TooltipField _stat) => TryGetTooltipNumber(_stat, out float value) ? value : 0f;

    public override string GetTooltipText(TooltipField _field)
    {
        switch (_field)
        {
            case TooltipField.Type:
                return equipmentType.ToString();

            case TooltipField.Mods:
                if (Mods == null || Mods.Length == 0) return null;

                // "1/3", then one "- name" line per installed mod.
                int installed = 0;
                string names = "";
                foreach (Mod mod in Mods)
                {
                    if (mod == null) continue;
                    installed++;
                    names += $"\n- {mod.itemName}";
                }
                return $"{installed}/{Mods.Length}{names}";

            default:
                return base.GetTooltipText(_field);
        }
    }

    /// <summary>
    /// Stats come with the installed mods' bonuses added, so the tooltip and the upgrade panel show
    /// what the equipment really gives. Weapon handling (speed, delay, lifetime) already includes mods.
    /// </summary>
    public override bool TryGetTooltipNumber(TooltipField _field, out float _value)
    {
        // Only things that fire have a Firing Rate, so armor and amulets get 0 (hidden by Hide When Zero).
        if (_field == TooltipField.DPS)
        {
            _value = Dps;
            return true;
        }

        if (!TryGetOwnNumber(_field, out _value))
            return base.TryGetTooltipNumber(_field, out _value);

        if (TooltipFields.IsStat(_field) && Mods != null)
            foreach (Mod mod in Mods)
                if (mod != null && mod.TryGetTooltipNumber(_field, out float bonus))
                    _value += bonus;

        return true;
    }

    /// <summary>This equipment's own number for _field, without mods.</summary>
    private bool TryGetOwnNumber(TooltipField _field, out float _value)
    {
        switch (_field)
        {
            case TooltipField.Level: _value = Level; return true;
            case TooltipField.BulletSpeed: _value = xVelocity; return true;
            case TooltipField.ShotDelay: _value = firingRate; return true;
            case TooltipField.BulletLifetime: _value = effectiveTime; return true;

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

            default: _value = 0f; return false;
        }
    }

    /// <summary>Spawns every Fire Effect on _weapon, the transform of the weapon being held.</summary>
    public void PlayFireEffects(Transform _weapon)
    {
        if (fireEffects == null) return;

        foreach (WeaponFireFX fx in fireEffects)
            fx?.Spawn(_weapon);
    }

    private void OnValidate()//excute when data is changing in the inspector.
    {
        // Equipment is always Equipment; the Inspector hides the field.
        ItemType = ItemType.Equipment;

        if (Mods == null || Mods.Length != modSlotCount)
        {
            System.Array.Resize(ref Mods, modSlotCount);
        }
        int maxVal = Mathf.Max(new int[] { sFireDamage, sFrostDamage, sPoisonDamage, sLightningDamage });

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
    /// <summary>
    /// World position of the muzzle on _weapon, the held weapon's transform: Ammo Offset laid out on
    /// the weapon, then turned with it, so it stays at the barrel tip at any angle.
    /// </summary>
    public Vector3 MuzzlePosition(Transform _weapon) =>
        _weapon.position + _weapon.rotation * Vector3.Scale(ammoOffset, _weapon.lossyScale);

    /// <summary>
    /// Fires one _ammoPrefab from the muzzle along the aim, at this weapon's X Velocity, gone after its Effective Time.
    /// Returns the bullet, or null when there is no prefab.
    /// </summary>
    public GameObject FireAmmo(GameObject _ammoPrefab, Transform _weapon)
    {
        if (_ammoPrefab == null) return null;

        // Facing comes from the player, as before, so ammo sprites still flip with them.
        Quaternion facing = PlayerManager.Instance.player.transform.rotation;

        GameObject ammo = Instantiate(_ammoPrefab, MuzzlePosition(_weapon), facing);
        ammo.GetComponent<Rigidbody2D>().velocity = GameManager.Instance.GetMouse(true) * xVelocity;
        AmmoBehaviour.MarkSource(ammo, _ammoPrefab);

        Destroy(ammo, effectiveTime);
        return ammo;
    }

    public void ExecuteEquipmentEffect(Transform _position)
    {
        foreach (var effect in equipmentEffects)
        {
            effect.ExecuteWeaponEffect(_position, this);//实例化可视的子弹，或效果
        }
    }

    public void AddModifiers() => ApplyModifiers(true);
    public void RemoveModifiers() => ApplyModifiers(false);

    // One list for both directions, so adding and removing can never drift apart.
    private void ApplyModifiers(bool _add)
    {
        PlayerStats playerStats = GetPlayerStats();

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

        if (Mods == null) return;

        foreach (var mod in Mods)
        {
            if (mod == null) continue;

            if (_add) mod.AddModifiers(playerStats);
            else mod.RemoveModifiers(playerStats);
        }
    }

    private static PlayerStats GetPlayerStats() => PlayerManager.Instance.player.GetComponent<PlayerStats>();
}
