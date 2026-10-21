using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a ForceStateAction: the targets, then a dropdown of the states they have, read from their
/// scripts (see StateLookup). With several targets only the states all of them share are offered.
/// </summary>
[CustomPropertyDrawer(typeof(ForceStateAction))]
public class ForceStateActionDrawer : PropertyDrawer
{
    private const float Spacing = 2f;
    private const float HelpHeight = 32f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty targets = property.FindPropertyRelative("targets");
        SerializedProperty stateName = property.FindPropertyRelative("stateName");
        SerializedProperty wait = property.FindPropertyRelative("waitUntilStateEnds");

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(line, label);
        line.y += line.height + Spacing;

        EditorGUI.indentLevel++;

        line.height = EditorGUI.GetPropertyHeight(targets, true);
        EditorGUI.PropertyField(line, targets, true);
        line.y += line.height + Spacing;

        line.height = EditorGUIUtility.singleLineHeight;
        DrawStatePopup(line, targets, stateName);
        line.y += line.height + Spacing;

        EditorGUI.PropertyField(line, wait);
        line.y += line.height + Spacing;

        string warning = Warning(targets, stateName);
        if (warning != null)
        {
            line.height = HelpHeight;
            EditorGUI.HelpBox(EditorGUI.IndentedRect(line), warning, MessageType.Warning);
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty targets = property.FindPropertyRelative("targets");
        float line = EditorGUIUtility.singleLineHeight + Spacing;

        float height = line                                         // label
                     + EditorGUI.GetPropertyHeight(targets, true) + Spacing
                     + line                                         // state
                     + line;                                        // wait

        if (Warning(targets, property.FindPropertyRelative("stateName")) != null)
            height += HelpHeight + Spacing;

        return height;
    }

    private static void DrawStatePopup(Rect _rect, SerializedProperty _targets, SerializedProperty _stateName)
    {
        List<string> states = CommonStates(_targets);
        string current = _stateName.stringValue;

        var options = new List<string>();
        int index = -1;

        // A state picked earlier that the targets no longer share stays visible, marked, until changed.
        bool missing = !string.IsNullOrEmpty(current) && !states.Contains(current);
        if (missing)
        {
            options.Add($"(missing) {ObjectNames.NicifyVariableName(current)}");
            index = 0;
        }

        foreach (string state in states)
        {
            if (state == current) index = options.Count;
            options.Add(ObjectNames.NicifyVariableName(state));
        }

        GUIContent label = new GUIContent("State", "The state to enter. Only states every target has are listed.");

        if (options.Count == 0)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.Popup(_rect, label.text, 0, new[] { "(add targets first)" });
            return;
        }

        EditorGUI.BeginChangeCheck();
        int picked = EditorGUI.Popup(_rect, label, index, options.Select(o => new GUIContent(o)).ToArray());

        if (EditorGUI.EndChangeCheck() && !(missing && picked == 0))
            _stateName.stringValue = states[picked - (missing ? 1 : 0)];
    }

    /// <summary>States every assigned target has, in the first target's order.</summary>
    private static List<string> CommonStates(SerializedProperty _targets)
    {
        List<string> common = null;

        for (int i = 0; i < _targets.arraySize; i++)
        {
            Object target = _targets.GetArrayElementAtIndex(i).objectReferenceValue;
            if (target == null) continue;

            List<string> names = StateLookup.StateNames(target.GetType());
            common = common == null ? new List<string>(names) : common.Where(names.Contains).ToList();
        }

        return common ?? new List<string>();
    }

    private static string Warning(SerializedProperty _targets, SerializedProperty _stateName)
    {
        bool anyTarget = false;
        for (int i = 0; i < _targets.arraySize; i++)
            anyTarget |= _targets.GetArrayElementAtIndex(i).objectReferenceValue != null;

        if (!anyTarget)
            return "No targets, so this step does nothing.";

        if (CommonStates(_targets).Count == 0)
            return "These targets share no state (or have no state machine), so there is nothing to pick.";

        if (string.IsNullOrEmpty(_stateName.stringValue))
            return "No state picked, so this step does nothing.";

        if (!CommonStates(_targets).Contains(_stateName.stringValue))
            return "Not every target has this state any more. Pick another.";

        return null;
    }
}
