#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Type-picker for [SerializeReference] fields: shows a dropdown to choose the
/// concrete type, then draws that type's fields. Works at any nesting depth
/// (the dropdown is anchored to the right edge so it is never clipped by
/// indentation). Two concrete subclasses register it for NPCAction and
/// NPCCondition; the logic lives here. Editor-only.
/// </summary>
public abstract class SerializeReferencePickerDrawer : PropertyDrawer
{
    private const float Spacing = 2f;
    private static readonly Dictionary<System.Type, System.Type[]> Candidates =
        new Dictionary<System.Type, System.Type[]>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (property.serializedObject.isEditingMultipleObjects)
        {
            EditorGUI.LabelField(line, label, new GUIContent("(multi-edit not supported)"));
            EditorGUI.EndProperty();
            return;
        }

        object value = property.managedReferenceValue;
        System.Type currentType = value?.GetType();

        GUIContent header = new GUIContent(
            label.text,
            currentType != null ? ObjectNames.NicifyVariableName(currentType.Name) : "Empty");

        // Anchor the dropdown to the RIGHT edge with a fixed width so indentation
        // can never squeeze it out of view (this was the "just text" bug).
        float popupW = Mathf.Min(150f, Mathf.Max(60f, line.width * 0.5f));
        Rect popupRect = new Rect(line.xMax - popupW, line.y, popupW, line.height);
        Rect headRect = new Rect(line.x, line.y, Mathf.Max(0f, line.width - popupW - 4f), line.height);

        if (value != null)
            property.isExpanded = EditorGUI.Foldout(headRect, property.isExpanded, header, true);
        else
            EditorGUI.LabelField(headRect, header);

        System.Type baseType = GetManagedReferenceFieldType(property);
        System.Type[] options = GetCandidateTypes(baseType);

        string[] names = new string[options.Length + 1];
        names[0] = "(None)";
        int current = 0;
        for (int i = 0; i < options.Length; i++)
        {
            names[i + 1] = ObjectNames.NicifyVariableName(options[i].Name);
            if (options[i] == currentType) current = i + 1;
        }

        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(popupRect, current, names);
        if (EditorGUI.EndChangeCheck())
        {
            property.managedReferenceValue =
                (selected == 0) ? null : Activator.CreateInstance(options[selected - 1]);
            property.isExpanded = selected != 0;
            EditorGUI.EndProperty();
            return;
        }

        if (value != null && property.isExpanded)
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
        if (property.propertyType != SerializedPropertyType.ManagedReference)
            return EditorGUI.GetPropertyHeight(property, label, true);

        if (property.serializedObject.isEditingMultipleObjects)
            return EditorGUIUtility.singleLineHeight;

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

    private static System.Type[] GetCandidateTypes(System.Type baseType)
    {
        if (baseType == null) return Array.Empty<System.Type>();
        if (Candidates.TryGetValue(baseType, out var cached)) return cached;

        System.Type[] types = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && (t.IsValueType || t.GetConstructor(System.Type.EmptyTypes) != null))
            .OrderBy(t => t.Name)
            .ToArray();

        Candidates[baseType] = types;
        return types;
    }

    private static System.Type GetManagedReferenceFieldType(SerializedProperty property)
    {
        string typename = property.managedReferenceFieldTypename;
        if (string.IsNullOrEmpty(typename)) return null;

        int sep = typename.IndexOf(' ');
        if (sep < 0) return null;

        string asmName = typename.Substring(0, sep);
        string typeName = typename.Substring(sep + 1);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name != asmName) continue;
            var t = asm.GetType(typeName);
            if (t != null) return t;
        }
        return System.Type.GetType(typeName);
    }
}

// Concrete registrations (split so both types definitely get the drawer).
[CustomPropertyDrawer(typeof(NPCAction), true)]
public class NPCActionPickerDrawer : SerializeReferencePickerDrawer { }

[CustomPropertyDrawer(typeof(NPCCondition), true)]
public class NPCConditionPickerDrawer : SerializeReferencePickerDrawer { }
#endif
