using System.Collections.Generic;
using UnityEngine;

/// <summary>One piece of information a skill's description can show. Saved as numbers, so add new ones at the end.</summary>
public enum SkillTooltipField
{
    CustomText,
    Name,
    Kind,
    State,
    Description,
    Cost,
    Stats,
    Cooldown,
    TriggerKey,
    InstalledCyberware,
    KeyHint
}

/// <summary>Which description a layout is for: one per kind of joint, plus the active skill list.</summary>
public enum SkillTooltipContext
{
    BasicJoint,
    SpecialJoint,
    CyberwareJoint,
    CyberwareSkillJoint,
    ActiveSkillList
}

[System.Serializable]
public class SkillTooltipLine
{
    // First, so the Inspector names each element after what the line says.
    [Tooltip("What the line says. {v} is replaced by the value; TextMeshPro rich text works. For Custom Text, the text itself.")]
    public string format = "{v}";
    public SkillTooltipField field;
    [Tooltip("Which of InvDescription's Line Styles this line uses.")]
    [TooltipStyleName] public string styleName = "Body";
    public bool customColor;
    public Color color = Color.white;

    public SkillTooltipLine() { }

    public SkillTooltipLine(SkillTooltipField _field, string _styleName, string _format = "{v}")
    {
        field = _field;
        styleName = _styleName;
        format = _format;
    }
}

[System.Serializable]
public class SkillTooltipLayout
{
    public SkillTooltipContext context;
    public List<SkillTooltipLine> lines = new List<SkillTooltipLine>();
}

/// <summary>
/// Picks what a skill's description says. Add, remove or reorder lines in Layouts to choose the info;
/// a line with nothing to say is left out. Drawn by the same tooltip box as items (InvDescription).
/// </summary>
public class SkillDescription : MonoBehaviour
{
    [SerializeField] private List<SkillTooltipLayout> layouts = DefaultLayouts();

    public void Show(SkillJoint _joint)
    {
        // With an item installed, its stats, cooldown and key are what count. Name and description are read from the item below.
        SkillInfo info = _joint.Installed != null ? _joint.Installed.cyberwareGives : _joint.Content;
        if (info == null) return;

        SkillTooltipContext context;

        switch (_joint.kind)
        {
            case SkillJointKind.Special: context = SkillTooltipContext.SpecialJoint; break;
            case SkillJointKind.Cyberware: context = SkillTooltipContext.CyberwareJoint; break;
            case SkillJointKind.CyberwareSkill: context = SkillTooltipContext.CyberwareSkillJoint; break;
            default: context = SkillTooltipContext.BasicJoint; break;
        }

        ShowLines(context, field => JointValue(field, _joint, info));
    }

    /// <summary>The description of a skill in the active list. _hint is the Key Hint line's text.</summary>
    public void Show(ActiveSkillEntry _entry, string _hint)
    {
        ShowLines(SkillTooltipContext.ActiveSkillList, field => EntryValue(field, _entry, _hint));
    }

    public void Hide()
    {
        if (InvDescription.Main != null)
            InvDescription.Main.Hide();
    }

    private void ShowLines(SkillTooltipContext _context, System.Func<SkillTooltipField, string> _valueOf)
    {
        if (InvDescription.Main == null) return;

        var entries = new List<TooltipEntry>();

        foreach (SkillTooltipLayout layout in layouts)
        {
            if (layout.context != _context) continue;

            foreach (SkillTooltipLine line in layout.lines)
            {
                string value = line.field == SkillTooltipField.CustomText ? "" : _valueOf(line.field);
                if (line.field != SkillTooltipField.CustomText && string.IsNullOrEmpty(value)) continue;

                entries.Add(new TooltipEntry(TooltipField.CustomText, line.styleName)
                {
                    format = line.format.Replace("{v}", value),
                    customColor = line.customColor,
                    color = line.color
                });
            }

            break;
        }

        InvDescription.Main.ShowLines(entries);
    }

    private static string JointValue(SkillTooltipField _field, SkillJoint _joint, SkillInfo _info)
    {
        switch (_field)
        {
            case SkillTooltipField.Name: return _joint.Installed != null ? _joint.Installed.itemName : _info.Name;
            case SkillTooltipField.Kind: return KindName(_joint.kind);
            case SkillTooltipField.Description: return _joint.Installed != null ? _joint.Installed.GetTooltipText(TooltipField.Description) : _info.Description;

            case SkillTooltipField.State:
                if (_joint.kind == SkillJointKind.Cyberware && _joint.State == SkillJointState.Ready)
                    return "Empty: left-click to install";
                return _joint.State.ToString();

            case SkillTooltipField.Cost:
                bool paysPoints = _joint.kind != SkillJointKind.Cyberware && !_joint.IsActive && _info.cost > 0 && !_info.activatedAtStart;
                return paysPoints ? _info.cost.ToString() : null;

            case SkillTooltipField.InstalledCyberware:
                return _joint.Installed != null ? _joint.Installed.itemName : null;

            case SkillTooltipField.TriggerKey:
                return _info.activeSkill != ActiveSkillId.None ? Skill.KeyName(KeyOf(_info)) : null;

            case SkillTooltipField.KeyHint: return null;

            default: return InfoValue(_field, _info);
        }
    }

    private static string EntryValue(SkillTooltipField _field, ActiveSkillEntry _entry, string _hint)
    {
        switch (_field)
        {
            case SkillTooltipField.Name: return _entry.info.Name;
            case SkillTooltipField.Kind: return "Active skill";
            case SkillTooltipField.Description: return _entry.info.Description;
            case SkillTooltipField.TriggerKey: return Skill.KeyName(_entry.key);
            case SkillTooltipField.KeyHint: return _hint;
            default: return InfoValue(_field, _entry.info);
        }
    }

    private static string InfoValue(SkillTooltipField _field, SkillInfo _info)
    {
        switch (_field)
        {
            case SkillTooltipField.Stats:
                if (_info.stats == null || _info.stats.Count == 0) return null;
                return string.Join("\n", _info.stats);

            case SkillTooltipField.Cooldown:
                return _info.activeSkill != ActiveSkillId.None && _info.Cooldown > 0f ? _info.Cooldown.ToString("0.##") : null;

            default: return null;
        }
    }

    // The key the player has given it, or the one it will start with.
    private static KeyCode KeyOf(SkillInfo _info)
    {
        if (Skill.Instance != null)
            foreach (ActiveSkillEntry entry in Skill.Instance.ActiveSkills)
                if (entry.id == _info.activeSkill)
                    return entry.key;

        return _info.DefaultKey;
    }

    private static string KindName(SkillJointKind _kind)
    {
        switch (_kind)
        {
            case SkillJointKind.Special: return "Special skill";
            case SkillJointKind.Cyberware: return "Cyberware";
            case SkillJointKind.CyberwareSkill: return "Cyberware skill";
            default: return "Basic skill";
        }
    }

    private static List<SkillTooltipLayout> DefaultLayouts()
    {
        SkillTooltipLine Line(SkillTooltipField _field, string _style, string _format = "{v}") =>
            new SkillTooltipLine(_field, _style, _format);

        SkillTooltipLayout Layout(SkillTooltipContext _context, params SkillTooltipLine[] _lines) =>
            new SkillTooltipLayout { context = _context, lines = new List<SkillTooltipLine>(_lines) };

        SkillTooltipLine[] JointLines() => new[]
        {
            Line(SkillTooltipField.Name, "Title"),
            Line(SkillTooltipField.Kind, "Subtitle"),
            Line(SkillTooltipField.State, "Subtitle"),
            Line(SkillTooltipField.Cost, "Stat", "Cost  {v} SP"),
            Line(SkillTooltipField.Stats, "Stat"),
            Line(SkillTooltipField.Cooldown, "Stat", "Cooldown  {v}s"),
            Line(SkillTooltipField.TriggerKey, "Stat", "Trigger  {v}"),
            Line(SkillTooltipField.Description, "Body")
        };

        return new List<SkillTooltipLayout>
        {
            Layout(SkillTooltipContext.BasicJoint, JointLines()),
            Layout(SkillTooltipContext.SpecialJoint, JointLines()),
            Layout(SkillTooltipContext.CyberwareSkillJoint, JointLines()),
            Layout(SkillTooltipContext.CyberwareJoint,
                Line(SkillTooltipField.Name, "Title"),
                Line(SkillTooltipField.Kind, "Subtitle"),
                Line(SkillTooltipField.State, "Subtitle"),
                Line(SkillTooltipField.Stats, "Stat"),
                Line(SkillTooltipField.Cooldown, "Stat", "Cooldown  {v}s"),
                Line(SkillTooltipField.TriggerKey, "Stat", "Trigger  {v}"),
                Line(SkillTooltipField.Description, "Body")),
            Layout(SkillTooltipContext.ActiveSkillList,
                Line(SkillTooltipField.Name, "Title"),
                Line(SkillTooltipField.TriggerKey, "Stat", "Trigger  {v}"),
                Line(SkillTooltipField.Cooldown, "Stat", "Cooldown  {v}s"),
                Line(SkillTooltipField.Stats, "Stat"),
                Line(SkillTooltipField.Description, "Body"),
                Line(SkillTooltipField.KeyHint, "Subtitle"))
        };
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Layouts")]
    private void ResetLayouts()
    {
        UnityEditor.Undo.RecordObject(this, "Reset Skill Description Layouts");
        layouts = DefaultLayouts();
    }
#endif
}
