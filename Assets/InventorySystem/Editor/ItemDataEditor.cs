using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for plain items. Type sits under Item Type and, like the cyberware's skills, only shows
/// when Item Type is Cyberware. Every other field draws as usual, so new fields need no change here.
/// Equipment and mods have their own inspectors, which hide these fields too.
/// </summary>
[CustomEditor(typeof(ItemData))]
[CanEditMultipleObjects]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty itemType = serializedObject.FindProperty("ItemType");
        bool isCyberware = itemType.hasMultipleDifferentValues || itemType.enumValueIndex == (int)ItemType.Cyberware;

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "cyberwareType" || property.name == "cyberwareSkills")
                continue;

            using (new EditorGUI.DisabledScope(property.name == "m_Script"))
                EditorGUILayout.PropertyField(property, true);

            if (property.name == "ItemType" && isCyberware)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cyberwareType"), new GUIContent("Type"));
                EditorGUI.indentLevel--;
            }
        }

        if (isCyberware)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cyberwareSkills"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
