#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Inspector for NPCController: draws the routine's action list as a reorderable
/// list with a one-line summary per action, a type-picker "+" menu, and a live
/// "currently running" readout in Play mode.
///
/// Editor-only. Place in an "Editor" folder (or anywhere; it is #if-guarded).
/// Elements are still drawn by SerializeReferencePickerDrawer, so expanding a
/// row lets you change its type and edit its fields. Nested GroupAction lists
/// use the default list + picker drawer.
/// </summary>
[CustomEditor(typeof(NPCController))]
public class NPCControllerEditor : Editor
{
    private SerializedProperty routineProp;
    private SerializedProperty actionsProp;
    private SerializedProperty loopProp;
    private SerializedProperty repeatProp;
    private SerializedProperty playOnStartProp;

    private ReorderableList list;

    private void OnEnable()
    {
        routineProp = serializedObject.FindProperty("routine");
        actionsProp = routineProp.FindPropertyRelative("actions");
        loopProp = routineProp.FindPropertyRelative("loop");
        repeatProp = routineProp.FindPropertyRelative("repeatCount");
        playOnStartProp = serializedObject.FindProperty("playOnStart");

        list = new ReorderableList(serializedObject, actionsProp, true, true, true, true)
        {
            drawHeaderCallback = DrawHeader,
            elementHeightCallback = ElementHeight,
            drawElementCallback = DrawElement,
            onAddDropdownCallback = OnAddDropdown
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(playOnStartProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Routine", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(loopProp);
        using (new EditorGUI.DisabledScope(!loopProp.boolValue))
            EditorGUILayout.PropertyField(repeatProp, new GUIContent("Repeat Count (0 = \u221E)"));

        EditorGUILayout.Space();
        list.DoLayoutList();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Now: " + ((NPCController)target).CurrentActionDescription(),
                                    MessageType.None);
            Repaint(); // keep the live readout updating
        }

        serializedObject.ApplyModifiedProperties();
    }

    // --- callbacks ---

    private void DrawHeader(Rect rect)
    {
        string mode = loopProp.boolValue
            ? (repeatProp.intValue > 0 ? $"loop x{repeatProp.intValue}" : "loop \u221E")
            : "chain (runs once)";
        EditorGUI.LabelField(rect, $"Actions \u2014 {mode}");
    }

    private float ElementHeight(int index)
    {
        if (index < 0 || index >= actionsProp.arraySize) return EditorGUIUtility.singleLineHeight;
        return EditorGUI.GetPropertyHeight(actionsProp.GetArrayElementAtIndex(index), true) + 4f;
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = actionsProp.GetArrayElementAtIndex(index);
        rect.y += 2f;
        rect.xMin += 12f; // leave room for the drag handle

        // The element label becomes the summary; the picker drawer renders the
        // foldout + type dropdown + fields underneath it.
        EditorGUI.PropertyField(rect, element, new GUIContent(SummaryFor(element, index)), true);
    }

    private static string SummaryFor(SerializedProperty element, int index)
    {
        NPCAction action = element.managedReferenceValue as NPCAction;
        if (action == null) return $"{index}:  (empty \u2014 pick a type \u2192)";

        string s = action.Summary;
        if (action.startCondition != null) s += $"   [if {action.startCondition.Summary}]";
        return $"{index}:  {s}";
    }

    private void OnAddDropdown(Rect buttonRect, ReorderableList l)
    {
        var menu = new GenericMenu();
        var types = TypeCache.GetTypesDerivedFrom<NPCAction>()
            .Where(t => !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null))
            .OrderBy(t => t.Name);

        foreach (Type type in types)
        {
            Type captured = type;
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
            {
                serializedObject.Update();
                int i = actionsProp.arraySize;
                actionsProp.arraySize++;
                actionsProp.GetArrayElementAtIndex(i).managedReferenceValue =
                    Activator.CreateInstance(captured);
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}
#endif
