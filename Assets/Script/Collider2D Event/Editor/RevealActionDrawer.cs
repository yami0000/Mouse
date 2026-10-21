using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a RevealAction without the settings that can't apply: a permanent reveal never hides,
/// so it has no show time or fade out, and an unlimited one has no show time.
/// </summary>
[CustomPropertyDrawer(typeof(RevealAction))]
public class RevealActionDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        SerializedProperty permanent = _property.FindPropertyRelative("permanent");
        SerializedProperty unlimited = _property.FindPropertyRelative("unlimited");

        bool mayHide = permanent.hasMultipleDifferentValues || !permanent.boolValue;
        bool timed = unlimited.hasMultipleDifferentValues || !unlimited.boolValue;

        SerializedProperty follow = _property.FindPropertyRelative("followPlayer");

        switch (_child.name)
        {
            case "headOffset":
                return follow.hasMultipleDifferentValues || follow.boolValue;

            case "unlimited":
            case "hideWhenPlayerLeaves":
            case "fadeOutTime":
            case "showAgainOnReenter":
            case "waitUntilHidden":
                return mayHide;

            case "showTime":
                return mayHide && timed;

            default:
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);

        string warning = Warning(property);
        if (warning == null) return;

        Rect box = new Rect(position.x, position.yMax - HelpHeight, position.width, HelpHeight - 2f);
        EditorGUI.HelpBox(EditorGUI.IndentedRect(box), warning, MessageType.Warning);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + (Warning(property) != null ? HelpHeight : 0f);
    }

    private const float HelpHeight = 40f;

    private static string Warning(SerializedProperty _property)
    {
        if (_property.FindPropertyRelative("permanent").boolValue)
            return null;

        bool unlimited = _property.FindPropertyRelative("unlimited").boolValue;
        bool onLeave = _property.FindPropertyRelative("hideWhenPlayerLeaves").boolValue;
        bool wait = _property.FindPropertyRelative("waitUntilHidden").boolValue;

        if (unlimited && !onLeave)
            return wait
                ? "Unlimited with no Hide When Player Leaves never hides, so Wait Until Hidden stalls the event forever."
                : "Unlimited with no Hide When Player Leaves never hides (until the scene reloads). Tick Permanent if that's the idea.";

        return null;
    }
}
