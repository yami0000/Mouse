using UnityEditor;

/// <summary>
/// Draws a tooltip layout (InvDescription > Layouts) folded to its name and the items it is for.
/// All Equipment Types only shows when Item Type is Equipment, and Equipment Type only when that is off.
/// </summary>
[CustomPropertyDrawer(typeof(TooltipLayout))]
public class TooltipLayoutDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        bool isEquipment = Is(_property.FindPropertyRelative("itemType"), (int)ItemType.Equipment);
        SerializedProperty allTypes = _property.FindPropertyRelative("allEquipmentTypes");

        switch (_child.name)
        {
            case "allEquipmentTypes":
                return isEquipment;
            case "equipmentType":
                return isEquipment && (allTypes.hasMultipleDifferentValues || !allTypes.boolValue);
            default:
                return true;
        }
    }

    protected override string Summary(SerializedProperty _property)
    {
        SerializedProperty itemType = _property.FindPropertyRelative("itemType");
        if (itemType.enumValueIndex < 0) return null;
        string target = itemType.enumDisplayNames[itemType.enumValueIndex];

        SerializedProperty equipmentType = _property.FindPropertyRelative("equipmentType");
        if (itemType.enumValueIndex == (int)ItemType.Equipment && !_property.FindPropertyRelative("allEquipmentTypes").boolValue
            && equipmentType.enumValueIndex >= 0)
            target += " > " + equipmentType.enumDisplayNames[equipmentType.enumValueIndex];

        string name = _property.FindPropertyRelative("name").stringValue;
        return string.IsNullOrEmpty(name) ? target : $"{name}   ({target})";
    }

    /// <summary>True when the enum is _value, or when a multi selection disagrees and everything should stay visible.</summary>
    private static bool Is(SerializedProperty _enum, int _value) =>
        _enum.hasMultipleDifferentValues || _enum.enumValueIndex == _value;
}
