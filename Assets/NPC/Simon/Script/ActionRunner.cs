#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Draws an ActionRunner (the routine, and every nested GroupAction.sequence)
/// as a reorderable list with a one-line summary per action and a type-picker
/// "+" menu, plus the loop / repeatCount fields. Because GroupAction holds an
/// ActionRunner, nested groups get the exact same UI as the top-level routine.
/// Editor-only.
/// </summary>
[CustomPropertyDrawer(typeof(ActionRunner))]
public class ActionRunnerDrawer : PropertyDrawer
{
    private const float Gap = 4f;
    private static readonly Dictionary<string, ReorderableList> Lists =
        new Dictionary<string, ReorderableList>();

    private ReorderableList GetList(SerializedProperty property)
    {
        SerializedProperty actions = property.FindPropertyRelative("actions");
        string key = property.serializedObject.targetObject.GetInstanceID() + ":" + property.propertyPath;

        if (Lists.TryGetValue(key, out ReorderableList existing))
        {
            existing.serializedProperty = actions; // rebind to this frame's property
            return existing;
        }

        ReorderableList rl = null;
        rl = new ReorderableList(property.serializedObject, actions, true, true, true, true);

        rl.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Actions");

        rl.elementHeightCallback = index =>
        {
            SerializedProperty a = rl.serializedProperty;
            if (index < 0 || index >= a.arraySize) return EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(a.GetArrayElementAtIndex(index), true) + Gap;
        };

        rl.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty a = rl.serializedProperty;
            if (index < 0 || index >= a.arraySize) return;
            SerializedProperty element = a.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.xMin += 12f; // room for the drag handle
            EditorGUI.PropertyField(rect, element, new GUIContent(Summary(element, index)), true);
        };

        rl.onAddDropdownCallback = (buttonRect, list) =>
        {
            SerializedProperty a = list.serializedProperty;
            GenericMenu menu = new GenericMenu();
            foreach (System.Type type in ConcreteTypes(typeof(NPCAction)))
            {
                System.Type captured = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                {
                    a.serializedObject.Update();
                    int i = a.arraySize;
                    a.arraySize++;
                    a.GetArrayElementAtIndex(i).managedReferenceValue = Activator.CreateInstance(captured);
                    a.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        };

        Lists[key] = rl;
        return rl;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float sl = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        Rect foldoutRect = new Rect(position.x, y, position.width, sl);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        y += sl + 2f;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty loop = property.FindPropertyRelative("loop");
            SerializedProperty repeat = property.FindPropertyRelative("repeatCount");

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, sl), loop);
            y += sl + 2f;

            if (loop.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, sl), repeat,
                                        new GUIContent("Repeat Count (0 = \u221E)"));
                y += sl + 2f;
            }

            y += Gap;
            ReorderableList rl = GetList(property);
            Rect listRect = EditorGUI.IndentedRect(new Rect(position.x, y, position.width, rl.GetHeight()));
            rl.DoList(listRect);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float sl = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded) return sl;

        float h = sl + 2f;                                            // foldout
        h += sl + 2f;                                                 // loop
        if (property.FindPropertyRelative("loop").boolValue) h += sl + 2f; // repeat
        h += Gap;
        h += GetList(property).GetHeight();                           // the list
        return h;
    }

    // --- helpers ---

    private static string Summary(SerializedProperty element, int index)
    {
        NPCAction a = element.managedReferenceValue as NPCAction;
        if (a == null) return $"{index}:  (empty \u2014 pick a type \u2192)";
        string s = a.Summary;
        if (a.startCondition != null) s += $"   [if {a.startCondition.Summary}]";
        return $"{index}:  {s}";
    }

    private static IEnumerable<System.Type> ConcreteTypes(System.Type baseType)
    {
        return TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && (t.IsValueType || t.GetConstructor(System.Type.EmptyTypes) != null))
            .OrderBy(t => t.Name);
    }
}
#endif
