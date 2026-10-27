using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Base for drawers that show only the fields that apply (e.g. only the Move settings of a Move step).
/// As a list element it folds, with a one line summary as the header, so a long list stays readable.
/// Fields a subclass does not know about are always shown, so new ones never disappear by accident.
/// </summary>
public abstract class FilteredDrawer : PropertyDrawer
{
    private const float Spacing = 2f;
    private static readonly Regex ElementIndex = new Regex(@"\.Array\.data\[(\d+)\]$");

    /// <summary>Fields drawn first, in this order, whatever their place in the class.</summary>
    protected virtual string[] Lead => System.Array.Empty<string>();

    /// <summary>Whether a child of this property is shown. Lead fields are always shown.</summary>
    protected abstract bool Show(SerializedProperty _property, SerializedProperty _child);

    /// <summary>Header text for list elements, without the number. Null = Unity's "Element N".</summary>
    protected virtual string Summary(SerializedProperty _property) => null;

    /// <summary>
    /// A bool field that switches the entry off. When set, the header gets an on/off box left of the
    /// title, like a component's enable box, and the field itself is not drawn again below. Null = no box.
    /// </summary>
    protected virtual string DisabledField => null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        SerializedProperty disabled = DisabledField != null ? property.FindPropertyRelative(DisabledField) : null;

        bool isElement = IsListElement(property, out int index);
        string summary = isElement ? Summary(property) : null;
        string header = summary != null ? $"{index + 1}. {summary}" : label.text;

        if (disabled != null)
            DrawToggleHeader(line, property, disabled, header, isElement);
        else if (isElement)
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, header, true);
        else
            EditorGUI.LabelField(line, label);

        if (IsOpen(property))
        {
            line.y += line.height + Spacing;

            EditorGUI.indentLevel++;
            foreach (SerializedProperty child in VisibleChildren(property))
            {
                line.height = EditorGUI.GetPropertyHeight(child, true);
                EditorGUI.PropertyField(line, child, true);
                line.y += line.height + Spacing;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (IsOpen(property))
        {
            height += Spacing;
            foreach (SerializedProperty child in VisibleChildren(property))
                height += EditorGUI.GetPropertyHeight(child, true) + Spacing;
        }

        return height;
    }

    /// <summary>Fold arrow, on/off box, then the title, in the same order as a component header.</summary>
    private static void DrawToggleHeader(Rect _line, SerializedProperty _property, SerializedProperty _disabled, string _title, bool _foldable)
    {
        const float ArrowWidth = 14f;
        const float BoxWidth = 16f;

        Rect row = EditorGUI.IndentedRect(_line);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float x = row.x;
        if (_foldable)
        {
            _property.isExpanded = EditorGUI.Foldout(new Rect(x, row.y, ArrowWidth, row.height), _property.isExpanded, GUIContent.none, true);
            x += ArrowWidth;
        }

        Rect box = new Rect(x, row.y, BoxWidth, row.height);
        Rect title = new Rect(box.xMax + 2f, row.y, row.xMax - box.xMax - 2f, row.height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = _disabled.hasMultipleDifferentValues;
        bool on = EditorGUI.Toggle(box, !_disabled.boolValue);
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
            _disabled.boolValue = !on;

        // A switched off entry reads greyed out, like a disabled component's name.
        using (new EditorGUI.DisabledScope(_disabled.boolValue && !_disabled.hasMultipleDifferentValues))
            EditorGUI.LabelField(title, _title);

        // Clicking the title still folds and unfolds the entry, as it did before the box was added.
        Event e = Event.current;
        if (_foldable && e.type == EventType.MouseDown && e.button == 0 && title.Contains(e.mousePosition))
        {
            _property.isExpanded = !_property.isExpanded;
            e.Use();
        }

        EditorGUI.indentLevel = indent;
    }

    /// <summary>"After 2s, with next" style suffix for anything with delay / runWithNext fields.</summary>
    protected static string Timing(SerializedProperty _property)
    {
        var parts = new List<string>();

        SerializedProperty delay = _property.FindPropertyRelative("delay");
        if (delay != null && delay.floatValue > 0f)
            parts.Add($"after {delay.floatValue:0.##}s");

        SerializedProperty withNext = _property.FindPropertyRelative("runWithNext");
        if (withNext != null && withNext.boolValue)
            parts.Add("with next");

        return parts.Count > 0 ? "   (" + string.Join(", ", parts) + ")" : "";
    }

    protected static string NameOf(SerializedProperty _objectProperty, string _whenEmpty)
    {
        Object value = _objectProperty != null ? _objectProperty.objectReferenceValue : null;
        return value != null ? value.name : _whenEmpty;
    }

    private bool IsOpen(SerializedProperty _property) => !IsListElement(_property, out _) || _property.isExpanded;

    private static bool IsListElement(SerializedProperty _property, out int _index)
    {
        Match match = ElementIndex.Match(_property.propertyPath);
        _index = match.Success ? int.Parse(match.Groups[1].Value) : -1;
        return match.Success;
    }

    private IEnumerable<SerializedProperty> VisibleChildren(SerializedProperty _property)
    {
        string[] lead = Lead;

        foreach (string name in lead)
        {
            SerializedProperty first = _property.FindPropertyRelative(name);
            if (first != null)
                yield return first;
        }

        SerializedProperty child = _property.Copy();
        SerializedProperty end = _property.GetEndProperty();

        if (!child.NextVisible(true))
            yield break;

        while (!SerializedProperty.EqualContents(child, end))
        {
            if (System.Array.IndexOf(lead, child.name) < 0 && Show(_property, child))
                yield return child.Copy();

            if (!child.NextVisible(false))
                break;
        }
    }
}
