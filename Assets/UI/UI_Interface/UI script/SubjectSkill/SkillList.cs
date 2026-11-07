using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>What a Mechanism or Active skill is called and looks like, wherever it's placed in the tree.</summary>
[Serializable]
public class ListedSkill
{
    // First, so the Inspector names each element after its skill.
    public string skillName;
    [TextArea(2, 6)] public string description;
    public Sprite icon;
    public Sprite activeIcon;
}

[Serializable]
public class ListedMechanism : ListedSkill
{
    public SkillMechanism id;

    // Each only shows on its own skill's entry (see SkillListDrawers).
    [Tooltip("How much of its cooldown time, in percent, a use with no damage taken takes off the cooldown.")]
    [Min(0)] public float hellfireCooldownReduction = 50f;
    [Tooltip("How much of Max Health, in percent, each use in a row with no damage taken adds to the next one.")]
    [Min(0)] public float hellfireBonusHealth = 10f;
    [Tooltip("How much, in percent, damage going past temporary health is reduced before it reaches health. 50 = halved.")]
    [Range(0, 100)] public float infernoOverflowReduction = 50f;
}

[Serializable]
public class ListedActiveSkill : ListedSkill
{
    public ActiveSkillId id;
    [Min(0)] public float cooldown;
    [Tooltip("The trigger key until the player changes it.")]
    public KeyCode defaultKey;

    // Only shows on its own skill's entry (see SkillListDrawers).
    [Tooltip("Temporary health given, in percent of Max Health.")]
    [Min(0)] public float hellEngineHealth = 30f;
    [Tooltip("Seconds that temporary health takes to arrive.")]
    [Min(0)] public float hellEngineSeconds = 2f;
}

/// <summary>
/// Every Mechanism and Active skill in one place. A joint or a cyberware item only picks one from its
/// dropdowns, and its name, description, icons, cooldown and key come from here. Cost, Activated At Start
/// and stats stay on the joint, since those depend on where it sits in the tree.
/// Lives in a Resources folder so it needs no reference, keep the name.
/// </summary>
public class SkillList : ScriptableObject
{
    private static readonly ListedMechanism DefaultMechanism = new ListedMechanism();
    private static readonly ListedActiveSkill DefaultActive = new ListedActiveSkill();

    private static SkillList instance;

    [SerializeField] private List<ListedMechanism> mechanisms = new List<ListedMechanism>();
    [SerializeField] private List<ListedActiveSkill> activeSkills = new List<ListedActiveSkill>();

    public static SkillList Instance => instance != null ? instance : instance = Resources.Load<SkillList>("SkillList");

    /// <summary>The entry for what a skill picked, the active skill first. Null = not listed, so it uses its own fields.</summary>
    public static ListedSkill Find(SkillMechanism _mechanism, ActiveSkillId _activeSkill)
    {
        SkillList list = Instance;
        if (list == null) return null;

        if (_activeSkill != ActiveSkillId.None)
        {
            ListedActiveSkill active = list.activeSkills.Find(skill => skill.id == _activeSkill);
            if (active != null) return active;
        }

        return _mechanism != SkillMechanism.None ? list.mechanisms.Find(skill => skill.id == _mechanism) : null;
    }

    /// <summary>For reading a skill's values. If it isn't listed, the values it starts with in code.</summary>
    public static ListedMechanism FindMechanism(SkillMechanism _id) =>
        Find(_id, ActiveSkillId.None) as ListedMechanism ?? DefaultMechanism;

    /// <summary>For reading a skill's values. If it isn't listed, the values it starts with in code.</summary>
    public static ListedActiveSkill FindActive(ActiveSkillId _id) =>
        Find(SkillMechanism.None, _id) as ListedActiveSkill ?? DefaultActive;
}
