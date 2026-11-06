using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A tooltip line's Style Name as a dropdown of the names in InvDescription > Line Styles,
/// so a line can't point at a style by a typo. A name no style has any more shows as "(missing)".
/// </summary>
[CustomPropertyDrawer(typeof(TooltipStyleNameAttribute))]
public class TooltipStyleNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty styles = property.serializedObject.FindProperty("styles");

        // Lines kept outside InvDescription (e.g. SkillDescription) still pick from its styles.
        if (styles == null && InvDescription.Main != null)
            styles = new SerializedObject(InvDescription.Main).FindProperty("styles");

        if (property.propertyType != SerializedPropertyType.String || styles == null || !styles.isArray)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var names = new List<string>();
        for (int i = 0; i < styles.arraySize; i++)
            names.Add(styles.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);

        var options = new List<GUIContent>();
        foreach (string name in names)
            options.Add(new GUIContent(name));

        int index = names.IndexOf(property.stringValue);
        if (index < 0)
        {
            options.Add(new GUIContent($"{property.stringValue} (missing)"));
            index = options.Count - 1;
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

        int picked = EditorGUI.Popup(position, label, index, options.ToArray());

        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck() && picked < names.Count)
            property.stringValue = names[picked];
        EditorGUI.EndProperty();
    }
}
