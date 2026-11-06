using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A [LiveText] string as a text area with, once it has {tokens}, a preview of the text with the values
/// filled in, and what's wrong with any token that can't be. Insert Value lists every value the object has
/// and puts its token in at the cursor. Tools > Live Text > Check Item Descriptions checks every item.
/// </summary>
[CustomPropertyDrawer(typeof(LiveTextAttribute))]
public class LiveTextDrawer : PropertyDrawer
{
    private const string PreviewWeaponKey = "LiveText.PreviewWeapon";
    private const float ButtonWidth = 96f;

    private const string SyntaxHelp =
        "Puts a value from this asset into the text, filled in again whenever the text is shown.\n\n" +
        "{damageRate}  a field of the asset\n" +
        "{EnhancedFirePowder.damageRate}  a field of one of its effects\n" +
        "{Weapon.Damage}  the worn weapon's value\n" +
        "{Player.Damage}  the player's stat (in game only)\n" +
        "{damageRate * Weapon.Damage}  maths: + - * / ( ) round min max...\n" +
        "{damageRate:%}  {radius:0.0}  a number format after the colon\n" +
        "[[ ... ]]  left out when a value inside can't be shown\n" +
        "{{ }}  a plain brace";

    // Where the cursor was in each text area, so Insert Value can put the token there.
    private static readonly Dictionary<string, int> cursors = new Dictionary<string, int>();
    // The width each one was last drawn at, for GetPropertyHeight, which isn't told.
    private static readonly Dictionary<string, float> widths = new Dictionary<string, float>();

    // EditorGUI's text areas all edit through this one TextEditor; Unity doesn't make it public.
    private static readonly FieldInfo recycledEditor =
        typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo activeEditor =
        typeof(EditorGUI).GetField("activeEditor", BindingFlags.NonPublic | BindingFlags.Static);

    private static bool previewWeaponLoaded;
    private static GUIStyle areaStyle;
    private static GUIStyle previewStyle;

    private static GUIStyle AreaStyle => areaStyle ??= new GUIStyle(EditorStyles.textArea) { wordWrap = true };
    private static GUIStyle PreviewStyle => previewStyle ??= new GUIStyle(EditorStyles.helpBox)
    {
        richText = true,
        wordWrap = true,
        fontSize = EditorStyles.label.fontSize,
        padding = new RectOffset(6, 6, 4, 4)
    };

    private static float Line => EditorGUIUtility.singleLineHeight;
    private static float Gap => EditorGUIUtility.standardVerticalSpacing;

    private class Parts
    {
        public string text;
        public string preview;
        public string errors;
        public string notes;
        public bool weaponRow;

        public float area;
        public float previewHeight;
        public float errorsHeight;
        public float notesHeight;
    }

    // ---------------------------------------------------------------- layout

    private Parts Measure(SerializedProperty _property, float _width)
    {
        LoadPreviewWeapon();

        Parts parts = new Parts { text = _property.stringValue ?? "" };
        int minLines = attribute is LiveTextAttribute live ? live.minLines : 3;

        float textHeight = AreaStyle.CalcHeight(new GUIContent(parts.text), _width);
        parts.area = Mathf.Max(textHeight, minLines * 14f + 4f);

        // A preview only means something for text with tokens, and only of one object.
        if (_property.hasMultipleDifferentValues || !HasTokens(parts.text))
            return parts;

        var problems = new List<LiveText.Problem>();
        parts.preview = LiveText.Preview(parts.text, _property.serializedObject.targetObject, LiveTextStyle.Default, problems);
        parts.previewHeight = PreviewStyle.CalcHeight(new GUIContent(parts.preview), _width);
        parts.weaponRow = parts.text.IndexOf("weapon.", StringComparison.OrdinalIgnoreCase) >= 0;

        var errors = new List<string>();
        var notes = new List<string>();
        foreach (LiveText.Problem problem in problems)
        {
            List<string> list = problem.state == LiveText.TokenState.Broken ? errors : notes;
            string line = $"{problem.token}  {problem.message}";
            if (!list.Contains(line)) list.Add(line);
        }

        if (errors.Count > 0)
        {
            parts.errors = string.Join("\n", errors);
            parts.errorsHeight = HelpBoxHeight(parts.errors, _width);
        }
        if (notes.Count > 0)
        {
            parts.notes = string.Join("\n", notes);
            parts.notesHeight = HelpBoxHeight(parts.notes, _width);
        }

        return parts;
    }

    private static bool HasTokens(string _text) =>
        _text.IndexOf('{') >= 0 || _text.IndexOf('}') >= 0 || _text.IndexOf("[[", StringComparison.Ordinal) >= 0;

    // The help box icon takes about 40 pixels of the width.
    private static float HelpBoxHeight(string _message, float _width) =>
        Mathf.Max(Line * 2f, EditorStyles.helpBox.CalcHeight(new GUIContent(_message), _width - 40f) + 4f);

    private static string Key(SerializedProperty _property) =>
        _property.serializedObject.targetObject.GetInstanceID() + "/" + _property.propertyPath;

    private static float Total(Parts _parts)
    {
        float height = Line + Gap + _parts.area;
        if (_parts.preview != null) height += Gap + _parts.previewHeight;
        if (_parts.weaponRow) height += Gap + Line;
        if (_parts.errors != null) height += Gap + _parts.errorsHeight;
        if (_parts.notes != null) height += Gap + _parts.notesHeight;
        return height;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
            return EditorGUI.GetPropertyHeight(property, label, true);

        if (!widths.TryGetValue(Key(property), out float width))
            width = EditorGUIUtility.currentViewWidth - 40f;

        return Total(Measure(property, width));
    }

    // ---------------------------------------------------------------- drawing

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        string key = Key(property);
        Rect indented = EditorGUI.IndentedRect(position);
        widths[key] = indented.width;

        Parts parts = Measure(property, indented.width);

        label = EditorGUI.BeginProperty(position, label, property);

        // Label, and Insert Value on the right.
        Rect row = new Rect(position.x, position.y, position.width, Line);
        Rect button = new Rect(row.xMax - ButtonWidth, row.y, ButtonWidth, Line);
        EditorGUI.LabelField(new Rect(row.x, row.y, row.width - ButtonWidth - 4f, Line), label);

        if (EditorGUI.DropdownButton(button, new GUIContent("Insert Value", SyntaxHelp), FocusType.Passive))
        {
            // Let go of the text area first, or it would write its own copy of the text back over the insert.
            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
            InsertMenu(property, key).DropDown(button);
        }

        // The text.
        float y = row.yMax + Gap;
        Rect area = new Rect(indented.x, y, indented.width, parts.area);
        string controlName = "LiveText " + key;

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        GUI.SetNextControlName(controlName);
        string text = EditorGUI.TextArea(area, parts.text, AreaStyle);

        EditorGUI.indentLevel = indent;
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
            property.stringValue = text;

        RememberCursor(controlName, key);
        y = area.yMax;

        if (parts.preview != null)
        {
            Rect preview = new Rect(indented.x, y + Gap, indented.width, parts.previewHeight);
            GUI.Label(preview, parts.preview, PreviewStyle);
            y = preview.yMax;
        }

        if (parts.weaponRow)
        {
            Rect weaponRect = new Rect(indented.x, y + Gap, indented.width, Line);
            DrawPreviewWeapon(weaponRect);
            y = weaponRect.yMax;
        }

        if (parts.errors != null)
        {
            Rect box = new Rect(indented.x, y + Gap, indented.width, parts.errorsHeight);
            EditorGUI.HelpBox(box, parts.errors, MessageType.Error);
            y = box.yMax;
        }

        if (parts.notes != null)
        {
            Rect box = new Rect(indented.x, y + Gap, indented.width, parts.notesHeight);
            EditorGUI.HelpBox(box, parts.notes, MessageType.Info);
        }

        EditorGUI.EndProperty();
    }

    private static void RememberCursor(string _controlName, string _key)
    {
        if (GUI.GetNameOfFocusedControl() != _controlName || !EditorGUIUtility.editingTextField)
            return;

        TextEditor editor = (recycledEditor?.GetValue(null) ?? activeEditor?.GetValue(null)) as TextEditor;
        if (editor != null)
            cursors[_key] = editor.cursorIndex;
    }

    private static void DrawPreviewWeapon(Rect _rect)
    {
        EditorGUI.BeginChangeCheck();
        var weapon = (ItemData_Equipment)EditorGUI.ObjectField(_rect,
            new GUIContent("Preview Weapon", "Stands in for the worn weapon in this preview. Only for the Inspector; the game uses the worn one."),
            LiveText.previewWeapon, typeof(ItemData_Equipment), false);

        if (EditorGUI.EndChangeCheck())
        {
            LiveText.previewWeapon = weapon;
            string guid = weapon != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(weapon)) : "";
            EditorPrefs.SetString(PreviewWeaponKey, guid);
        }
    }

    private static void LoadPreviewWeapon()
    {
        if (previewWeaponLoaded) return;
        previewWeaponLoaded = true;

        string guid = EditorPrefs.GetString(PreviewWeaponKey, "");
        if (!string.IsNullOrEmpty(guid))
            LiveText.previewWeapon = AssetDatabase.LoadAssetAtPath<ItemData_Equipment>(AssetDatabase.GUIDToAssetPath(guid));
    }

    // ---------------------------------------------------------------- Insert Value

    private static GenericMenu InsertMenu(SerializedProperty _property, string _key)
    {
        SerializedObject serializedObject = _property.serializedObject;
        string path = _property.propertyPath;
        object root = serializedObject.targetObject;

        var menu = new GenericMenu();

        void Add(string _menuPath, string _token)
        {
            string value = LiveText.Format("{" + _token + "}", root, LiveTextStyle.Plain);
            menu.AddItem(new GUIContent($"{_menuPath}   = {Shorten(value)}"), false,
                () => Insert(serializedObject, path, _key, "{" + _token + "}"));
        }

        // The asset's own values.
        string own = ObjectNames.NicifyVariableName(root.GetType().Name);
        foreach (FieldInfo field in DataFields(root.GetType()))
            if (field.Name != path)
                Add($"{own}/{field.Name}", field.Name);

        // Its effects', by short name, numbered when there are several of one type.
        List<object> listed = LiveText.ListedObjects(root);
        var counts = new Dictionary<string, int>();
        foreach (object effect in listed)
        {
            string name = LiveText.ShortName(effect.GetType());
            counts[name] = counts.TryGetValue(name, out int count) ? count + 1 : 1;
        }

        var seen = new Dictionary<string, int>();
        foreach (object effect in listed)
        {
            string name = LiveText.ShortName(effect.GetType());
            int index = seen.TryGetValue(name, out int i) ? i : 0;
            seen[name] = index + 1;

            string prefix = counts[name] > 1 ? $"{name}[{index}]" : name;
            foreach (FieldInfo field in DataFields(effect.GetType()))
                Add($"Effects/{prefix.Replace("[", " [")}/{field.Name}", $"{prefix}.{field.Name}");
        }

        menu.AddSeparator("");

        // The worn weapon's tooltip numbers, mods included.
        foreach (TooltipField field in Enum.GetValues(typeof(TooltipField)))
            if (IsWeaponField(field))
                Add($"Weapon (worn)/{field}", $"Weapon.{field}");

        // The player's stats.
        foreach (FieldInfo field in typeof(PlayerStats).GetFields(BindingFlags.Instance | BindingFlags.Public))
            if (field.FieldType == typeof(Stats))
                menu.AddItem(new GUIContent($"Player (in game)/{field.Name}"), false,
                    () => Insert(serializedObject, path, _key, $"{{Player.{field.Name}}}"));

        return menu;
    }

    private static bool IsWeaponField(TooltipField _field)
    {
        if (TooltipFields.IsText(_field)) return false;

        switch (_field)
        {
            case TooltipField.CustomText:
            case TooltipField.StackSize:
            case TooltipField.BulletSpeedMultiplier:
            case TooltipField.ShotDelayMultiplier:
            case TooltipField.BulletLifetimeBonus:
                return false;
            default:
                return true;
        }
    }

    // What the Inspector would show of _type: saved, visible fields holding a number, a switch, a stat or a name.
    private static IEnumerable<FieldInfo> DataFields(Type _type)
    {
        var types = new List<Type>();
        for (Type type = _type; type != null && type != typeof(ScriptableObject) && type != typeof(MonoBehaviour) && type != typeof(object); type = type.BaseType)
            types.Insert(0, type); // base class fields first, as the Inspector orders them

        foreach (Type type in types)
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                bool saved = (field.IsPublic && !field.IsNotSerialized) || field.IsDefined(typeof(SerializeField), true);
                if (!saved || field.IsDefined(typeof(HideInInspector), true)) continue;

                Type t = field.FieldType;
                if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Stats))
                    yield return field;
            }
    }

    private static string Shorten(string _value)
    {
        _value = (_value ?? "").Replace("\n", " ").Replace("/", "∕");
        return _value.Length > 30 ? _value.Substring(0, 30) + "…" : _value;
    }

    private static void Insert(SerializedObject _serializedObject, string _path, string _key, string _token)
    {
        if (_serializedObject == null || _serializedObject.targetObject == null) return;

        _serializedObject.Update();
        SerializedProperty property = _serializedObject.FindProperty(_path);
        if (property == null) return;

        string text = property.stringValue ?? "";
        int at = cursors.TryGetValue(_key, out int cursor) ? Mathf.Clamp(cursor, 0, text.Length) : text.Length;

        property.stringValue = text.Insert(at, _token);
        _serializedObject.ApplyModifiedProperties();
        cursors[_key] = at + _token.Length;
    }

    // ---------------------------------------------------------------- checking every item

    [MenuItem("Tools/Live Text/Check Item Descriptions")]
    private static void CheckItemDescriptions()
    {
        int items = 0;
        int broken = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:ItemData"))
        {
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || string.IsNullOrEmpty(item.Description)) continue;

            items++;
            var problems = new List<LiveText.Problem>();
            LiveText.Preview(item.Description, item, LiveTextStyle.Plain, problems);

            foreach (LiveText.Problem problem in problems)
                if (problem.state == LiveText.TokenState.Broken)
                {
                    broken++;
                    Debug.LogError($"{item.name}: {problem.token}  {problem.message}", item);
                }
        }

        if (broken == 0)
            Debug.Log($"Live Text: the descriptions of all {items} items are fine.");
        else
            Debug.LogWarning($"Live Text: {broken} broken token(s) in item descriptions. Click an error to find its item.");
    }
}
