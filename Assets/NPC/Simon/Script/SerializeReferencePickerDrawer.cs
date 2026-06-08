#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a type-picker dropdown to [SerializeReference] fields of NPCAction and
/// NPCCondition (including elements inside their lists). Without this, Unity's
/// default Inspector adds null elements you can't assign a concrete type to.
///
/// Place anywhere in the project (an "Editor" folder is tidiest). Editor-only.
/// </summary>
[CustomPropertyDrawer(typeof(NPCAction), true)]
[CustomPropertyDrawer(typeof(NPCCondition), true)]
public class SerializeReferencePickerDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        bool hasValue = property.managedReferenceValue != null;

        // Label / foldout on the left.
        Rect labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
        if (hasValue)
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
        else
            EditorGUI.LabelField(labelRect, label);

        // Type dropdown on the right.
        Rect popupRect = new Rect(line.x + EditorGUIUtility.labelWidth, line.y,
                                  line.width - EditorGUIUtility.labelWidth, line.height);

        Type baseType = GetManagedReferenceFieldType(property);
        List<Type> options = GetCandidateTypes(baseType);

        string[] names = new string[options.Count + 1];
        names[0] = "(None)";
        int current = 0;
        Type currentType = property.managedReferenceValue?.GetType();
        for (int i = 0; i < options.Count; i++)
        {
            names[i + 1] = Nicify(options[i].Name);
            if (options[i] == currentType) current = i + 1;
        }

        int selected = EditorGUI.Popup(popupRect, current, names);
        if (selected != current)
        {
            if (selected == 0)
                property.managedReferenceValue = null;
            else
            {
                property.managedReferenceValue = Activator.CreateInstance(options[selected - 1]);
                property.isExpanded = true;
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
            return; // redraw fresh next frame
        }

        // Draw the chosen type's own fields.
        if (property.managedReferenceValue != null && property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = line.y + line.height + Spacing;

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
        if (property.managedReferenceValue != null && property.isExpanded)
        {
            SerializedProperty iter = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enter = true;
            while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
            {
                h += EditorGUI.GetPropertyHeight(iter, true) + Spacing;
                enter = false;
            }
        }
        return h;
    }

    // --- helpers ---

    private static List<Type> GetCandidateTypes(Type baseType)
    {
        if (baseType == null) return new List<Type>();
        return TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToList();
    }

    private static Type GetManagedReferenceFieldType(SerializedProperty property)
    {
        // managedReferenceFieldTypename looks like: "Assembly-CSharp NPCAction"
        string typename = property.managedReferenceFieldTypename;
        if (string.IsNullOrEmpty(typename)) return null;

        string[] parts = typename.Split(' ');
        if (parts.Length != 2) return null;

        string asmName = parts[0];
        string typeName = parts[1];

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name != asmName) continue;
            var t = asm.GetType(typeName);
            if (t != null) return t;
        }
        return Type.GetType(typeName);
    }

    private static string Nicify(string name) => ObjectNames.NicifyVariableName(name);
}
#endif
