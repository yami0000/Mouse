using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a [StateName] string as a dropdown of the states of the object being edited, read from its
/// script (see StateLookup). A state picked earlier that no longer exists stays shown, marked missing.
/// </summary>
[CustomPropertyDrawer(typeof(StateNameAttribute))]
public class StateNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "[StateName] only works on strings.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        Object owner = property.serializedObject.targetObject;
        List<string> states = owner != null ? StateLookup.StateNames(owner.GetType()) : new List<string>();
        string current = property.stringValue;

        var options = new List<GUIContent>();
        int index = -1;

        bool missing = !string.IsNullOrEmpty(current) && !states.Contains(current);
        if (missing)
        {
            options.Add(new GUIContent($"(missing) {ObjectNames.NicifyVariableName(current)}"));
            index = 0;
        }

        foreach (string state in states)
        {
            if (state == current) index = options.Count;
            options.Add(new GUIContent(ObjectNames.NicifyVariableName(state)));
        }

        if (options.Count == 0)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.Popup(position, label, 0, new[] { new GUIContent("(no state machine found)") });
        }
        else
        {
            using (new EditorGUI.MixedValueScope(property.hasMultipleDifferentValues))
            {
                EditorGUI.BeginChangeCheck();
                int picked = EditorGUI.Popup(position, label, index, options.ToArray());

                if (EditorGUI.EndChangeCheck() && !(missing && picked == 0))
                    property.stringValue = states[picked - (missing ? 1 : 0)];
            }
        }

        EditorGUI.EndProperty();
    }
}
