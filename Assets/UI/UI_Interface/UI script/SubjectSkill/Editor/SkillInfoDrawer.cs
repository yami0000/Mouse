using UnityEditor;

/// <summary>
/// The Mechanism and Active Skill dropdowns come first. Picking a skill from Skill List hides the fields
/// that now come from there; otherwise cooldown and the default key only show on an active skill.
/// In a list, folds to the skill's name.
/// </summary>
[CustomPropertyDrawer(typeof(SkillInfo))]
public class SkillInfoDrawer : FilteredDrawer
{
    protected override string[] Lead => new[] { "mechanism", "activeSkill" };

    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        switch (_child.name)
        {
            case "skillName":
            case "description":
            case "icon":
            case "activeIcon":
                return Listed(_property) == null;

            case "cooldown":
            case "defaultKey":
                SerializedProperty activeSkill = _property.FindPropertyRelative("activeSkill");
                return Listed(_property) == null
                    && (activeSkill.hasMultipleDifferentValues || activeSkill.enumValueIndex != (int)ActiveSkillId.None);

            default:
                return true;
        }
    }

    protected override string Summary(SerializedProperty _property)
    {
        ListedSkill listed = Listed(_property);
        string name = listed != null ? listed.skillName : _property.FindPropertyRelative("skillName").stringValue;
        return string.IsNullOrEmpty(name) ? "(unnamed skill)" : name;
    }

    private static ListedSkill Listed(SerializedProperty _property)
    {
        SerializedProperty mechanism = _property.FindPropertyRelative("mechanism");
        SerializedProperty activeSkill = _property.FindPropertyRelative("activeSkill");

        if (mechanism.hasMultipleDifferentValues || activeSkill.hasMultipleDifferentValues)
            return null;

        return SkillList.Find((SkillMechanism)mechanism.enumValueIndex, (ActiveSkillId)activeSkill.enumValueIndex);
    }
}
