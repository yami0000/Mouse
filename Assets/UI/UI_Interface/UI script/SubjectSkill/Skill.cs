using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// What the player has from the skill tree: skill points, switched-on mechanisms and active skills.
/// Gameplay code only talks to this; which joint gave what is SkillTree's business.
/// </summary>
public class Skill : MonoBehaviour
{
    public static Skill Instance;

    public int SP = 5;

    [Header("Trigger Keys")]
    [Tooltip("Keys a skill can never use. The trigger keys of the menu pages (UI_Active) are refused as well, without being listed here.")]
    [SerializeField] private List<KeyCode> reservedKeys = new List<KeyCode>
    {
        KeyCode.Mouse0, KeyCode.E, KeyCode.LeftShift, KeyCode.RightShift, KeyCode.Space,
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, // the movement axes use these too
        KeyCode.Q, KeyCode.Escape // throwing, closing menus
    };

    // Counted, so one of two joints giving the same thing coming off doesn't take it away.
    private readonly Dictionary<SkillMechanism, int> mechanisms = new Dictionary<SkillMechanism, int>();
    private readonly Dictionary<ActiveSkillId, ActiveSkillEntry> entries = new Dictionary<ActiveSkillId, ActiveSkillEntry>();
    private readonly List<ActiveSkillEntry> activeSkills = new List<ActiveSkillEntry>();

    /// <summary>The active skills the player can use right now, in the order they were gained.</summary>
    public IReadOnlyList<ActiveSkillEntry> ActiveSkills => activeSkills;

    /// <summary>The list of active skills or one of their keys changed.</summary>
    public event Action OnActiveSkillsChanged;

    public bool Metabolism => Has(SkillMechanism.Metabolism);
    public bool OverCharging => Has(SkillMechanism.OverCharging);
    public bool DoubleJump => Has(SkillMechanism.DoubleJump);

    void Awake()
    {
        Instance = this;
    }

    public bool HasEnoughPoints(int cost)
    {
        return SP >= cost;
    }

    public void SpendPoints(int cost)
    {
        SP -= cost;
    }

    public bool Has(SkillMechanism _mechanism) =>
        mechanisms.TryGetValue(_mechanism, out int count) && count > 0;

    public void AddMechanism(SkillMechanism _mechanism)
    {
        if (_mechanism == SkillMechanism.None) return;

        mechanisms.TryGetValue(_mechanism, out int count);
        mechanisms[_mechanism] = count + 1;
    }

    public void RemoveMechanism(SkillMechanism _mechanism)
    {
        if (_mechanism == SkillMechanism.None) return;

        mechanisms.TryGetValue(_mechanism, out int count);
        mechanisms[_mechanism] = Mathf.Max(0, count - 1);
    }

    #region Active skills

    public void GrantActiveSkill(SkillInfo _info, SkillJointKind _kind)
    {
        if (_info.activeSkill == ActiveSkillId.None) return;

        if (!entries.TryGetValue(_info.activeSkill, out ActiveSkillEntry entry))
        {
            entry = new ActiveSkillEntry { id = _info.activeSkill, key = FreeKey(_info.defaultKey) };
            entries[_info.activeSkill] = entry;
        }

        entry.info = _info;
        entry.kind = _kind;
        entry.grants++;

        if (entry.grants == 1)
            activeSkills.Add(entry);

        OnActiveSkillsChanged?.Invoke();
    }

    public void RevokeActiveSkill(SkillInfo _info)
    {
        if (_info.activeSkill == ActiveSkillId.None) return;
        if (!entries.TryGetValue(_info.activeSkill, out ActiveSkillEntry entry) || entry.grants == 0) return;

        entry.grants--;

        if (entry.grants == 0)
            activeSkills.Remove(entry);

        OnActiveSkillsChanged?.Invoke();
    }

    public void TickCooldowns(float _deltaTime)
    {
        foreach (ActiveSkillEntry entry in entries.Values)
            if (entry.cooldownLeft > 0f)
                entry.cooldownLeft -= _deltaTime;
    }

    public void StartCooldown(ActiveSkillId _id)
    {
        if (entries.TryGetValue(_id, out ActiveSkillEntry entry))
            entry.cooldownLeft = entry.info.cooldown;
    }

    /// <summary>
    /// Gives _entry the key, or leaves it alone and says why not: the key is reserved, or another skill
    /// has it (a skill of an uninstalled cyberware included, as it gets its key back when reinstalled).
    /// </summary>
    public bool TryBindKey(ActiveSkillEntry _entry, KeyCode _key, out string _problem)
    {
        _problem = null;

        if (_key == _entry.key) return true;

        if (IsReservedKey(_key))
        {
            _problem = $"{KeyName(_key)} can't be used";
            return false;
        }

        foreach (ActiveSkillEntry other in entries.Values)
        {
            if (other == _entry || other.key != _key) continue;

            _problem = $"{KeyName(_key)} is used by {other.info.skillName}";
            return false;
        }

        _entry.key = _key;
        OnActiveSkillsChanged?.Invoke();
        return true;
    }

    public bool IsReservedKey(KeyCode _key)
    {
        if (reservedKeys.Contains(_key)) return true;

        // Including the pages switched off for now, so a skill never opens a menu as well.
        return FindObjectsByType<UI_Active>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Any(page => page.TriggerKey == _key);
    }

    // A default that can't be used starts unbound instead of firing alongside something else.
    private KeyCode FreeKey(KeyCode _key)
    {
        if (_key == KeyCode.None || IsReservedKey(_key)) return KeyCode.None;
        return entries.Values.Any(entry => entry.key == _key) ? KeyCode.None : _key;
    }

    public static string KeyName(KeyCode _key)
    {
        switch (_key)
        {
            case KeyCode.None: return "-";
            case KeyCode.Mouse0: return "LMB";
            case KeyCode.Mouse1: return "RMB";
            case KeyCode.Mouse2: return "MMB";
        }

        if (_key >= KeyCode.Alpha0 && _key <= KeyCode.Alpha9)
            return ((int)_key - (int)KeyCode.Alpha0).ToString();

        return _key.ToString();
    }

    #endregion
}
