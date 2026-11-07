using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Starting a tree: buttons that add joints, and the check for joints left unconnected.</summary>
[CustomEditor(typeof(SkillTree))]
public class SkillTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillTree tree = (SkillTree)target;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
            SkillTreeAuthoring.RefreshLines(tree);

        if (tree.JointRoot == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Switch UI Parent on to see the map in the Scene view; it switches itself off when the game starts.\n" +
            "Select a joint, then drag from the dot beside it: onto a joint to connect them, onto empty space to make a new joint.",
            MessageType.Info);

        EditorGUILayout.LabelField("Add a joint in the middle of the map", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (SkillJointKind kind in new[] { SkillJointKind.Basic, SkillJointKind.Special, SkillJointKind.Cyberware })
                if (GUILayout.Button(SkillTreeAuthoring.Label(kind)))
                    SkillTreeAuthoring.CreateJoint(tree, kind, Vector2.zero);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Check the Tree for Problems"))
            SkillTreeAuthoring.LogProblems(tree);

        List<string> problems = SkillTreeAuthoring.FindProblems(tree);
        foreach (string problem in problems)
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
    }
}
