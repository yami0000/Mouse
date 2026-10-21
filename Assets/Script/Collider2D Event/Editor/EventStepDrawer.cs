using UnityEditor;

/// <summary>Draws a CameraEvent step with only the fields its type uses.</summary>
[CustomPropertyDrawer(typeof(EventStep))]
public class EventStepDrawer : FilteredDrawer
{
    protected override string[] Lead => new[] { "type", "delay", "runWithNext" };

    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        SerializedProperty type = _property.FindPropertyRelative("type");
        if (type.hasMultipleDifferentValues)
            return true;

        switch (_child.name)
        {
            case "dialogueName": return type.enumValueIndex == (int)CameraEventType.Dialogue;
            case "waves":        return type.enumValueIndex == (int)CameraEventType.EnemyWaves;
            case "move":         return type.enumValueIndex == (int)CameraEventType.Move;
            case "reveal":       return type.enumValueIndex == (int)CameraEventType.Reveal;
            case "killTargets":  return type.enumValueIndex == (int)CameraEventType.KillEnemies;
            case "forceState":   return type.enumValueIndex == (int)CameraEventType.ForceState;
            default:             return true;
        }
    }

    protected override string Summary(SerializedProperty _property)
    {
        string what;

        switch ((CameraEventType)_property.FindPropertyRelative("type").enumValueIndex)
        {
            case CameraEventType.Dialogue:
                string node = _property.FindPropertyRelative("dialogueName").stringValue;
                what = "Dialogue: " + (string.IsNullOrEmpty(node) ? "(no node, skipped)" : node);
                break;

            case CameraEventType.EnemyWaves:
                int count = _property.FindPropertyRelative("waves").arraySize;
                what = $"Enemy Waves: {count} wave{(count == 1 ? "" : "s")}";
                break;

            case CameraEventType.Reveal:
                SerializedProperty targets = _property.FindPropertyRelative("reveal.targets");
                what = "Reveal: " + (targets.arraySize == 0
                    ? "(no objects)"
                    : targets.arraySize == 1
                        ? NameOf(targets.GetArrayElementAtIndex(0), "(empty)")
                        : $"{targets.arraySize} objects");

                if (_property.FindPropertyRelative("reveal.followPlayer").boolValue)
                    what += ", over player";

                if (_property.FindPropertyRelative("reveal.permanent").boolValue)
                    what += ", permanent";
                break;

            case CameraEventType.KillEnemies:
                SerializedProperty kills = _property.FindPropertyRelative("killTargets");
                what = "Kill: " + (kills.arraySize == 0
                    ? "(no enemies, skipped)"
                    : kills.arraySize == 1
                        ? NameOf(kills.GetArrayElementAtIndex(0), "(empty)")
                        : $"{kills.arraySize} enemies");
                break;

            case CameraEventType.ForceState:
                string state = _property.FindPropertyRelative("forceState.stateName").stringValue;
                SerializedProperty forced = _property.FindPropertyRelative("forceState.targets");
                string who = forced.arraySize == 0
                    ? "(no targets)"
                    : forced.arraySize == 1
                        ? NameOf(forced.GetArrayElementAtIndex(0), "(empty)")
                        : $"{forced.arraySize} targets";
                what = $"Force State: {who} → " + (string.IsNullOrEmpty(state) ? "(no state)" : ObjectNames.NicifyVariableName(state));
                break;

            default:
                what = "Move: " + NameOf(_property.FindPropertyRelative("move.target"), "(no target)");
                break;
        }

        return what + Timing(_property);
    }
}
