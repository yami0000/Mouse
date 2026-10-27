using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One Create menu entry for every item, item effect and passive, instead of a CreateAssetMenu
/// attribute on each class. Assets > Create > Data Asset... opens a picker listing every concrete
/// type of each family, so a new item, effect or passive class shows up here on its own.
/// </summary>
public class DataAssetMenu : EditorWindow
{
    private struct Family
    {
        public string label;
        public Type[] types;
    }

    private static readonly (string label, Type baseType, bool includeBase)[] Families =
    {
        ("Item", typeof(ItemData), true),               // plain ItemData is a material, currency or consumable
        ("Item Effect", typeof(ItemEffect), false),     // the base does nothing on its own
        ("Passive", typeof(PassiveEffectSO), false),
    };

    private Family[] families;
    private string folder = "Assets";
    private string search = "";
    private Vector2 scroll;

    [MenuItem("Assets/Create/Data Asset...", false, 0)]
    private static void Open()
    {
        DataAssetMenu window = CreateInstance<DataAssetMenu>();
        window.titleContent = new GUIContent("Create Data Asset");
        window.folder = SelectedFolder();
        window.minSize = new Vector2(260, 320);
        window.ShowUtility();
    }

    private void OnEnable()
    {
        families = Families.Select(f => new Family
        {
            label = f.label,
            types = TypeCache.GetTypesDerivedFrom(f.baseType)
                .Append(f.baseType)
                .Where(t => (t != f.baseType || f.includeBase) && !t.IsAbstract && !t.IsGenericType)
                .OrderBy(t => t.Name)
                .ToArray(),
        }).ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Into " + folder, EditorStyles.miniLabel);
        search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (Family family in families)
        {
            IEnumerable<Type> shown = family.types.Where(t => Matches(t, search));
            if (!shown.Any()) continue;

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(family.label, EditorStyles.boldLabel);

            foreach (Type type in shown)
            {
                if (!GUILayout.Button(DisplayName(type), EditorStyles.miniButton)) continue;

                Create(type);
                Close();
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Create(Type _type)
    {
        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/New {DisplayName(_type)}.asset");

        ScriptableObject asset = CreateInstance(_type);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    // ItemData_Equipment -> "Item Data Equipment", Effect_Grenade -> "Effect Grenade".
    private static string DisplayName(Type _type) => ObjectNames.NicifyVariableName(_type.Name.Replace('_', ' '));

    private static bool Matches(Type _type, string _search) =>
        string.IsNullOrEmpty(_search) || DisplayName(_type).IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>The folder right-clicked in the Project window, or the folder of the selected asset.</summary>
    private static string SelectedFolder()
    {
        foreach (UnityEngine.Object selected in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(selected);

            if (AssetDatabase.IsValidFolder(path))
                return path;

            if (!string.IsNullOrEmpty(path))
                return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        return "Assets";
    }
}
