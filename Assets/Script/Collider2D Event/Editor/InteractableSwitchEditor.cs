using UnityEditor;
using UnityEngine;

/// <summary>Inspector for InteractableSwitch: shows the Door or the Elevator settings, whichever Mode is picked.</summary>
[CustomEditor(typeof(InteractableSwitch), true)]
[CanEditMultipleObjects]
public class InteractableSwitchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

        SerializedProperty mode = serializedObject.FindProperty("mode");
        EditorGUILayout.PropertyField(mode);

        bool mixed = mode.hasMultipleDifferentValues;

        if (mixed || mode.enumValueIndex == (int)InteractableSwitch.Mode.Door)
        {
            SerializedProperty door = serializedObject.FindProperty("door");
            EditorGUILayout.PropertyField(door, true);

            if (!door.hasMultipleDifferentValues && door.FindPropertyRelative("target").objectReferenceValue == null)
                EditorGUILayout.HelpBox("No door Target, so pressing E does nothing.", MessageType.Warning);

            SerializedProperty remember = serializedObject.FindProperty("rememberState");
            EditorGUILayout.PropertyField(remember);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("openOnlyOnce"));

            if (remember.hasMultipleDifferentValues || remember.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("saveId"));
        }

        if (mixed || mode.enumValueIndex == (int)InteractableSwitch.Mode.CallElevator)
        {
            SerializedProperty elevator = serializedObject.FindProperty("elevator");
            EditorGUILayout.PropertyField(elevator);

            if (!elevator.hasMultipleDifferentValues && elevator.objectReferenceValue == null)
                EditorGUILayout.HelpBox("No Elevator, so pressing E does nothing.", MessageType.Warning);
        }

        if (((Component)target).GetComponent<Collider2D>() == null)
            EditorGUILayout.HelpBox("Needs a trigger Collider2D on this object for the player to stand in.", MessageType.Warning);

        serializedObject.ApplyModifiedProperties();
    }
}
