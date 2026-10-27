using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Inspector for equipment. Weapon settings only show on weapons, and the stats worked out
/// from Level in OnValidate are greyed out, since anything typed there is overwritten.
/// Every other field draws as usual, so new fields need no change here.
/// </summary>
[CustomEditor(typeof(ItemData_Equipment))]
[CanEditMultipleObjects]
public class ItemData_EquipmentEditor : Editor
{
    // Only a weapon is ever fired, so these mean nothing on armor, amulets and the rest.
    private static readonly HashSet<string> weaponOnly = new HashSet<string>
    {
        "mechanism", "ammoPrefab", "ammoOffset", "xVelocity", "firingRate", "effectiveTime", "recoil", "fireEffects",
    };

    // Recomputed from the s-prefixed base stats and Level on every change.
    private static readonly HashSet<string> derived = new HashSet<string>
    {
        "FirePower", "FireDamage", "FrostDamage", "PoisonDamage", "LightningDamage",
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty type = serializedObject.FindProperty("equipmentType");
        bool isWeapon = Is(type, EquipmentType.Weapon);
        bool isArmor = Is(type, EquipmentType.Armor);

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (!isWeapon && weaponOnly.Contains(property.name))
                continue;

            // Armor is only derived on armor; anything else keeps whatever was typed in.
            bool readOnly = property.name == "m_Script"
                         || derived.Contains(property.name)
                         || (isArmor && property.name == "Armor");

            using (new EditorGUI.DisabledScope(readOnly))
                EditorGUILayout.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>True when the type is _value, or when a multi selection disagrees and everything should stay visible.</summary>
    private static bool Is(SerializedProperty _type, EquipmentType _value) =>
        _type.hasMultipleDifferentValues || _type.enumValueIndex == (int)_value;
}
