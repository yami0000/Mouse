using UnityEditor;

/// <summary>
/// Draws a weapon's Fire Effects entry folded to a one line summary, with an on/off box left of the
/// title (see FilteredDrawer).
/// </summary>
[CustomPropertyDrawer(typeof(WeaponFireFX))]
public class WeaponFireFXDrawer : FilteredDrawer
{
    protected override string DisabledField => "disabled";

    protected override bool Show(SerializedProperty _property, SerializedProperty _child) => true;

    protected override string Summary(SerializedProperty _property)
    {
        string fx = NameOf(_property.FindPropertyRelative("prefab"), "(no FX)");
        UnityEngine.Vector2 offset = _property.FindPropertyRelative("offset").vector2Value;
        float rotation = _property.FindPropertyRelative("rotation").floatValue;

        string summary = $"{fx} at ({offset.x:0.##}, {offset.y:0.##})";
        if (rotation != 0f) summary += $", {rotation:0.##}°";
        if (_property.FindPropertyRelative("followWeapon").boolValue) summary += ", follows weapon";

        return summary;
    }
}
