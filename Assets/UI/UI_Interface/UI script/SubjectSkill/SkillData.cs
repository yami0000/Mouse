using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Saved as numbers in the scene, so add new ones at the end.</summary>
public enum SkillJointKind
{
    Basic,          // stats, or just a fact about the protagonist
    Special,        // unlocks a mechanism or an active skill
    Cyberware,      // takes a cyberware item; costs no skill points
    CyberwareSkill  // filled from the installed cyberware; hidden without one
}

public enum SkillJointState
{
    Locked,     // a previous joint isn't activated
    Ready,      // can be activated
    Activated
}

/// <summary>Things a joint can switch on. To add one: add it here, read it with Skill.Instance.Has(...), then give it an entry in Skill List.</summary>
public enum SkillMechanism
{
    None,
    DoubleJump,
    Metabolism,
    OverCharging,
    Hellfire,
    ShieldOfTheInferno
}

/// <summary>Skills with a trigger key. To add one: add it here, give it a case in Player.UseActiveSkill, then give it an entry in Skill List.</summary>
public enum ActiveSkillId
{
    None,
    ProtectiveShield,
    Reaper,
    HellEngine
}

/// <summary>A stat a joint can raise. Same numbers as TooltipField, so it reads like an item's stat line.</summary>
public enum SkillStat
{
    Damage = (int)TooltipField.Damage,
    CriticalChance = (int)TooltipField.CriticalChance,
    CriticalDamage = (int)TooltipField.CriticalDamage,
    MaxHP = (int)TooltipField.MaxHP,
    Armor = (int)TooltipField.Armor,
    FireResistance = (int)TooltipField.FireResistance,
    FrostResistance = (int)TooltipField.FrostResistance,
    LightningResistance = (int)TooltipField.LightningResistance,
    PoisonResistance = (int)TooltipField.PoisonResistance,
    FireDamage = (int)TooltipField.FireDamage,
    FrostDamage = (int)TooltipField.FrostDamage,
    PoisonDamage = (int)TooltipField.PoisonDamage,
    LightningDamage = (int)TooltipField.LightningDamage
}

[Serializable]
public class SkillStatBonus
{
    public SkillStat stat;
    public int value;

    public Stats Find(EntityStats _stats)
    {
        switch (stat)
        {
            case SkillStat.Damage: return _stats.Damage;
            case SkillStat.CriticalChance: return _stats.CriticalChance;
            case SkillStat.CriticalDamage: return _stats.CriticalDamage;
            case SkillStat.MaxHP: return _stats.MaxHP;
            case SkillStat.Armor: return _stats.Armor;
            case SkillStat.FireResistance: return _stats.FireResistance;
            case SkillStat.FrostResistance: return _stats.FrostResistance;
            case SkillStat.LightningResistance: return _stats.LightingResistance;
            case SkillStat.PoisonResistance: return _stats.PoisonResistance;
            case SkillStat.FireDamage: return _stats.FireDamage;
            case SkillStat.FrostDamage: return _stats.FrostDamage;
            case SkillStat.PoisonDamage: return _stats.PoisonDamage;
            case SkillStat.LightningDamage: return _stats.LightningDamage;
            default: return null;
        }
    }

    /// <summary>The same words an item's tooltip uses, e.g. "Damage  +5".</summary>
    public override string ToString()
    {
        TooltipField field = (TooltipField)(int)stat;
        return TooltipFields.DefaultFormat(field).Replace("{v}", TooltipFields.FormatNumber(field, value));
    }
}

/// <summary>
/// What a skill is and gives. Basic and Special joints hold their own; a Cyberware Skill joint shows one
/// from the installed item's Cyberware Skills, so the same joint means something else per cyberware.
/// </summary>
[Serializable]
public class SkillInfo
{
    public string skillName;
    [TextArea(2, 6)] public string description;

    [Header("Icon")]
    [Tooltip("The line art, see-through inside. The joint's background Image colours in behind it when activated (Skill Tree > Joint Looks), so this one sprite is enough.")]
    public Sprite icon;
    [Tooltip("Optional. Only for an icon whose activated form can't be made by colouring the joint's background behind it.")]
    public Sprite activeIcon;

    [Header("Unlocking")]
    [Tooltip("Skill points to activate. Cyberware joints ignore this.")]
    [Min(0)] public int cost = 1;
    [Tooltip("Free, and activated as soon as every previous joint allows it. A previous joint that isn't activated still holds it back, unless it's a Cyberware joint ready for installing.")]
    public bool activatedAtStart;

    [Header("What it gives")]
    public List<SkillStatBonus> stats = new List<SkillStatBonus>();
    [Tooltip("Pick a skill from Skill List. Its name, description and icons then come from there.")]
    public SkillMechanism mechanism;
    [Tooltip("Pick a skill from Skill List: a skill with a trigger key, listed on the left of the skill page. Its name, description, icons, cooldown and key then come from there.")]
    public ActiveSkillId activeSkill;
    [Min(0)] public float cooldown;
    [Tooltip("The trigger key until the player changes it.")]
    public KeyCode defaultKey;

    private ListedSkill Listed => SkillList.Find(mechanism, activeSkill);

    /// <summary>Whether Skill List supplies what's below, instead of this skill's own fields.</summary>
    public bool IsListed => Listed != null;
    public string Name => Listed != null ? Listed.skillName : skillName;
    public string Description => Listed != null ? Listed.description : description;
    public Sprite Icon => Listed != null ? Listed.icon : icon;
    public Sprite ActiveIcon => Listed != null ? Listed.activeIcon : activeIcon;
    public float Cooldown => Listed is ListedActiveSkill active ? active.cooldown : cooldown;
    public KeyCode DefaultKey => Listed is ListedActiveSkill active ? active.defaultKey : defaultKey;

    public void Apply(PlayerStats _stats, Skill _skill, SkillJointKind _kind)
    {
        foreach (SkillStatBonus bonus in stats)
            bonus.Find(_stats)?.AddModifier(bonus.value);

        _skill.AddMechanism(mechanism);
        _skill.GrantActiveSkill(this, _kind);
    }

    public void Remove(PlayerStats _stats, Skill _skill)
    {
        foreach (SkillStatBonus bonus in stats)
            bonus.Find(_stats)?.RemoveModifier(bonus.value);

        _skill.RemoveMechanism(mechanism);
        _skill.RevokeActiveSkill(this);
    }
}

/// <summary>An active skill the player has or had. Kept after a cyberware comes out, so its key and cooldown survive a reinstall.</summary>
public class ActiveSkillEntry
{
    public ActiveSkillId id;
    public SkillInfo info;
    public SkillJointKind kind;
    public KeyCode key;
    public float cooldownLeft;
    public int grants; // how many activated joints give it

    public bool IsReady => cooldownLeft <= 0f;
    public float CooldownFraction => info.Cooldown > 0f ? Mathf.Clamp01(cooldownLeft / info.Cooldown) : 0f;
}

/// <summary>The colours of one kind of joint in each state. The sprites stay on the joint's own Images.</summary>
[Serializable]
public class SkillJointLook
{
    public SkillJointKind kind;
    public Color lockedFill = Color.clear;
    public Color readyFill = Color.clear;
    public Color activeFill = new Color(0.2f, 0.65f, 0.85f);
    public Color lockedIcon = new Color(1f, 1f, 1f, 0.35f);
    public Color readyIcon = Color.white;
    public Color activeIcon = Color.white;

    public Color Fill(SkillJointState _state) =>
        _state == SkillJointState.Activated ? activeFill : _state == SkillJointState.Ready ? readyFill : lockedFill;

    public Color Icon(SkillJointState _state) =>
        _state == SkillJointState.Activated ? activeIcon : _state == SkillJointState.Ready ? readyIcon : lockedIcon;
}
