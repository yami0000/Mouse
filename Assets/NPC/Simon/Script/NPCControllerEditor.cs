#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for NPCController. The routine (and nested groups) are drawn by
/// ActionRunnerDrawer, so this editor only adds a live "currently running"
/// readout in Play mode. Editor-only.
/// </summary>
[CustomEditor(typeof(NPCController))]
public class NPCControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // routine -> ActionRunnerDrawer, plus playOnStart

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Now: " + ((NPCController)target).CurrentActionDescription(),
                                    MessageType.None);
            Repaint(); // keep the readout updating each frame
        }
    }
}
#endif
