using UnityEditor;

/// <summary>
/// Inspector for mods: everything as usual except what Mod.OnValidate sets itself (Item Type, and Type
/// from whether it has Effects) and Item Effects, which mods don't use: their mechanisms go in Effects.
/// </summary>
[CustomEditor(typeof(Mod))]
[CanEditMultipleObjects]
public class ModEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "ItemType", "type", "itemEffects", "cyberwareType", "cyberwareGives", "cyberwareSkills");
        serializedObject.ApplyModifiedProperties();
    }
}
