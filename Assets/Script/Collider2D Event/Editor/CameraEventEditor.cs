using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for CameraEvent. The enemy wave settings only show while some step is an
/// Enemy Waves step. Steps and consequences hide their own unused fields through
/// EventStepDrawer and EventActionDrawer.
///
/// Anything added to CameraEvent later still shows up on its own, under Other.
/// </summary>
[CustomEditor(typeof(CameraEvent), true)]
[CanEditMultipleObjects]
public class CameraEventEditor : Editor
{
    private readonly HashSet<string> drawn = new HashSet<string>();

    private static readonly string[] WaveFields = { "spawnMargin", "walkInDistance", "walkInSpeed", "groundLayer" };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        drawn.Clear();

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        drawn.Add("m_Script");

        Section("Camera & Area");
        SerializedProperty cameraMode = Get("cameraMode");
        Draw(cameraMode);

        if (cameraMode.hasMultipleDifferentValues || cameraMode.enumValueIndex == (int)EventCameraMode.ConfineToArea)
            Draw("confineBlend");
        else
            Skip(new[] { "confineBlend" });

        Draw("restrictPlayer");

        if (Get("restrictPlayer").boolValue && ((CameraEvent)target).GetComponent<Collider2D>() == null)
            EditorGUILayout.HelpBox("Restrict Player keeps the player inside this object's Collider2D, but there is none.", MessageType.Warning);

        Section("Event Steps");
        EditorGUILayout.HelpBox("Steps run top to bottom. Delay = seconds before a step starts. " +
                                "Run With Next = the next step starts at the same time instead of waiting for this one.", MessageType.None);
        Draw("steps");

        if (Get("restrictPlayer").boolValue && WaitsForPlayerToLeave())
            EditorGUILayout.HelpBox("A Reveal step waits until hidden, but only hides when the player walks out, and Restrict Player " +
                                    "keeps them in until the event completes. The event can never finish.", MessageType.Warning);

        if (Get("steps").arraySize == 0 && !Get("steps").hasMultipleDifferentValues)
            EditorGUILayout.HelpBox("No steps, so the event completes as soon as the player walks in.", MessageType.Info);

        if (targets.Length > 1 || ((CameraEvent)target).HasStep(CameraEventType.EnemyWaves))
        {
            Section("Enemy Wave Settings");
            foreach (string field in WaveFields)
                Draw(field);
        }
        else
        {
            Skip(WaveFields);
        }

        Section("Consequences");
        Draw("consequences");

        DrawEverythingElse();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>Draws any property this inspector does not know about, so subclasses and new fields still appear.</summary>
    private void DrawEverythingElse()
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        bool headerDrawn = false;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (drawn.Contains(property.propertyPath))
                continue;

            if (!headerDrawn)
            {
                Section("Other");
                headerDrawn = true;
            }

            EditorGUILayout.PropertyField(property, true);
        }
    }

    /// <summary>A Reveal step the event waits on that only ever hides when the player leaves.</summary>
    private bool WaitsForPlayerToLeave()
    {
        SerializedProperty steps = Get("steps");

        for (int i = 0; i < steps.arraySize; i++)
        {
            SerializedProperty step = steps.GetArrayElementAtIndex(i);
            if (step.FindPropertyRelative("type").enumValueIndex != (int)CameraEventType.Reveal) continue;

            SerializedProperty reveal = step.FindPropertyRelative("reveal");
            if (!reveal.FindPropertyRelative("permanent").boolValue &&
                reveal.FindPropertyRelative("unlimited").boolValue &&
                reveal.FindPropertyRelative("waitUntilHidden").boolValue)
                return true;
        }

        return false;
    }

    private SerializedProperty Get(string _name) => serializedObject.FindProperty(_name);

    private void Draw(string _name) => Draw(Get(_name));

    private void Draw(SerializedProperty _property)
    {
        if (_property == null)
            return;

        EditorGUILayout.PropertyField(_property, true);
        drawn.Add(_property.propertyPath);
    }

    private void Skip(string[] _names)
    {
        foreach (string name in _names)
            drawn.Add(name);
    }

    private static void Section(string _title)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
    }
}
