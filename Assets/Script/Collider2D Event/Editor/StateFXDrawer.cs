using UnityEditor;

/// <summary>Draws a StateFX folded to a one line summary, with Seconds only when the lifetime uses it.</summary>
[CustomPropertyDrawer(typeof(StateFX))]
public class StateFXDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        if (_child.name != "seconds")
            return true;

        // Seconds is also One Loop's fallback for an effect with no animation or particles.
        SerializedProperty lifetime = _property.FindPropertyRelative("lifetime");
        return lifetime.hasMultipleDifferentValues || lifetime.enumValueIndex != (int)StateFX.Lifetime.UntilStateEnds;
    }

    protected override string Summary(SerializedProperty _property)
    {
        string state = _property.FindPropertyRelative("state").stringValue;
        string fx = NameOf(_property.FindPropertyRelative("fxPrefab"), "(no FX)");
        string at = NameOf(_property.FindPropertyRelative("spawnPoint"), "self");
        float delay = _property.FindPropertyRelative("delay").floatValue;

        string end;
        switch ((StateFX.Lifetime)_property.FindPropertyRelative("lifetime").enumValueIndex)
        {
            case StateFX.Lifetime.OneLoop:        end = "one loop"; break;
            case StateFX.Lifetime.UntilStateEnds: end = "until state ends"; break;
            default:                              end = $"{_property.FindPropertyRelative("seconds").floatValue:0.##}s"; break;
        }

        string when = string.IsNullOrEmpty(state) ? "(no state)" : ObjectNames.NicifyVariableName(state);
        if (delay > 0f) when += $" +{delay:0.##}s";

        return $"{when} → {fx} at {at}, {end}";
    }
}
