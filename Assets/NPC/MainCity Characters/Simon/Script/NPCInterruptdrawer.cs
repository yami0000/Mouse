#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws each NPCInterrupt list element with a readable header
/// ("id  [trigger]  -> N action(s)") instead of "Element N", then defers to the
/// child drawers (the NPCCondition picker for 'trigger', the ActionRunner drawer
/// for 'body'). Editor-only. Reads the header straight from serialized properties
/// so there is no reflection. Drop in NPC/Simon/Script (or any Editor folder).
/// </summary>
[CustomPropertyDrawer(typeof(NPCInterrupt))]
public class NPCInterruptDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, BuildHeader(property), true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = line.yMax + Spacing;

            SerializedProperty iter = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enter = true;
            while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
            {
                float h = EditorGUI.GetPropertyHeight(iter, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), iter, true);
                y += h + Spacing;
                enter = false;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded) return h;

        SerializedProperty iter = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enter = true;
        while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
        {
            h += EditorGUI.GetPropertyHeight(iter, true) + Spacing;
            enter = false;
        }
        return h;
    }

    private static GUIContent BuildHeader(SerializedProperty property)
    {
        SerializedProperty idProp = property.FindPropertyRelative("id");
        SerializedProperty trigProp = property.FindPropertyRelative("trigger");
        SerializedProperty bodyProp = property.FindPropertyRelative("body");

        string id = idProp != null ? idProp.stringValue : "?";

        string trig;
        if (trigProp != null && trigProp.managedReferenceValue != null)
            trig = ObjectNames.NicifyVariableName(trigProp.managedReferenceValue.GetType().Name);
        else
            trig = $"Raise(\"{id}\")";

        int n = 0;
        if (bodyProp != null)
        {
            SerializedProperty actions = bodyProp.FindPropertyRelative("actions");
            if (actions != null) n = actions.arraySize;
        }

        return new GUIContent($"\u201c{id}\u201d   [{trig}]   \u2192 {n} action(s)");
    }
}
#endif
