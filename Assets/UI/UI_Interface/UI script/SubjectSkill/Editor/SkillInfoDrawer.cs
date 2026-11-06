using UnityEditor;

/// <summary>Cooldown and the default key only mean something on an active skill. In a list, folds to the skill's name.</summary>
[CustomPropertyDrawer(typeof(SkillInfo))]
public class SkillInfoDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        if (_child.name != "cooldown" && _child.name != "defaultKey")
            return true;

        SerializedProperty activeSkill = _property.FindPropertyRelative("activeSkill");
        return activeSkill.hasMultipleDifferentValues || activeSkill.enumValueIndex != (int)ActiveSkillId.None;
    }

    protected override string Summary(SerializedProperty _property)
    {
        string name = _property.FindPropertyRelative("skillName").stringValue;
        return string.IsNullOrEmpty(name) ? "(unnamed skill)" : name;
    }
}
