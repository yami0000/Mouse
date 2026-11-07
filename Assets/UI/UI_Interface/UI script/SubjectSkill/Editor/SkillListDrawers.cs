using UnityEditor;

/// <summary>A mechanism's values only show on its own entry. To add some: add the field, then a case here.</summary>
[CustomPropertyDrawer(typeof(ListedMechanism))]
public class ListedMechanismDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        SkillMechanism id = (SkillMechanism)_property.FindPropertyRelative("id").enumValueIndex;

        switch (_child.name)
        {
            case "hellfireCooldownReduction":
            case "hellfireBonusHealth":
                return id == SkillMechanism.Hellfire;

            case "infernoOverflowReduction":
                return id == SkillMechanism.ShieldOfTheInferno;

            default:
                return true;
        }
    }

    protected override string Summary(SerializedProperty _property) =>
        _property.FindPropertyRelative("skillName").stringValue;
}

/// <summary>An active skill's values only show on its own entry. To add some: add the field, then a case here.</summary>
[CustomPropertyDrawer(typeof(ListedActiveSkill))]
public class ListedActiveSkillDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        ActiveSkillId id = (ActiveSkillId)_property.FindPropertyRelative("id").enumValueIndex;

        switch (_child.name)
        {
            case "hellEngineHealth":
            case "hellEngineSeconds":
                return id == ActiveSkillId.HellEngine;

            default:
                return true;
        }
    }

    protected override string Summary(SerializedProperty _property) =>
        _property.FindPropertyRelative("skillName").stringValue;
}
