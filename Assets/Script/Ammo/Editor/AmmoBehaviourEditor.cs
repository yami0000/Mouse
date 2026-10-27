using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for AmmoBehaviour. Only shows the settings the chosen modes actually read,
/// so a straight bullet is not buried under homing and split fields it never uses.
///
/// Anything added to AmmoBehaviour later still shows up on its own, under Other, so this
/// file never has to be edited just to keep a new field visible.
/// </summary>
[CustomEditor(typeof(AmmoBehaviour), true)]
[CanEditMultipleObjects]
public class AmmoBehaviourEditor : Editor
{
    private readonly HashSet<string> drawn = new HashSet<string>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        drawn.Clear();

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        drawn.Add("m_Script");

        // Serialized but HideInInspector, so they must never surface under Other.
        drawn.Add("finalDirection");
        drawn.Add("lastFramePosition");
        drawn.Add("sp");

        SerializedProperty side = Get("side");
        SerializedProperty impactMode = Get("impactMode");
        SerializedProperty damageMode = Get("damageMode");
        SerializedProperty homingMode = Get("homingMode");

        bool isPlayerAmmo = Matches(side, (int)AmmoSide.Player);
        bool dealsDamage = Matches(impactMode, (int)AmmoImpactMode.Damage);

        Section("Owner");
        Draw(side);

        Section("Impact");
        Draw(impactMode);
        Draw("hitOnce");
        Draw("destroyOnHit");
        Draw("impactPrefab");

        // The lifetime only means anything once there is something to clean up.
        if (Get("impactPrefab").objectReferenceValue != null || Get("impactPrefab").hasMultipleDifferentValues)
            Draw("impactPrefabLifeTime");
        else
            drawn.Add("impactPrefabLifeTime");

        if (Matches(impactMode, (int)AmmoImpactMode.SpawnOnly) && Get("impactPrefab").objectReferenceValue == null)
            EditorGUILayout.HelpBox("SpawnOnly deals no damage of its own, so with no Impact Prefab this ammo does nothing on hit.", MessageType.Warning);

        if (dealsDamage)
        {
            Section("Damage");
            Draw(damageMode);

            if (Matches(damageMode, (int)AmmoDamageMode.RandomRange))
            {
                Draw("damageMultiplierMin");
                Draw("damageMultiplierMax");
            }
            else
            {
                drawn.Add("damageMultiplierMin");
                drawn.Add("damageMultiplierMax");
            }

            if (Matches(damageMode, (int)AmmoDamageMode.ChargeCurve))
                Draw("damageCurve");
            else
                drawn.Add("damageCurve");

            Section("Impact FX");
            Draw("splashEffectPrefab");

            if (Get("splashEffectPrefab").objectReferenceValue != null || Get("splashEffectPrefab").hasMultipleDifferentValues)
                Draw("damageToParticleCount");
            else
                drawn.Add("damageToParticleCount");
        }
        else
        {
            // Damage and its FX are dead weight on a healing or spawn-only projectile.
            drawn.Add("damageMode");
            drawn.Add("damageMultiplierMin");
            drawn.Add("damageMultiplierMax");
            drawn.Add("damageCurve");
            drawn.Add("splashEffectPrefab");
            drawn.Add("damageToParticleCount");
        }

        Section("Movement");
        Draw(homingMode);

        if (Matches(homingMode, (int)AmmoHomingMode.Homing))
        {
            Draw("slowDownRate");
            Draw("homingDelay");
            Draw("homingAcceleration");
            Draw("maxSpeed");

            // Enemy ammo homes on the player, so it has nothing to search for.
            if (isPlayerAmmo)
            {
                Draw("whatIsEnemy");
                Draw("enemyCheckDistance");
            }
            else
            {
                drawn.Add("whatIsEnemy");
                drawn.Add("enemyCheckDistance");
            }
        }
        else
        {
            drawn.Add("slowDownRate");
            drawn.Add("homingDelay");
            drawn.Add("homingAcceleration");
            drawn.Add("maxSpeed");
            drawn.Add("whatIsEnemy");
            drawn.Add("enemyCheckDistance");
        }

        Section("Ground");
        Draw("destroyOnGround");
        Draw("ricochetOffGround");
        Draw("splitOnGround");

        if (Get("ricochetOffGround").boolValue && Get("destroyOnGround").boolValue)
            EditorGUILayout.HelpBox("Ricochet and Destroy On Ground fight each other. Turn Destroy On Ground off so the bounce survives.", MessageType.Warning);

        if (Get("splitOnGround").boolValue || Get("splitOnGround").hasMultipleDifferentValues)
        {
            Section("Split On Ground");
            Draw("splitPrefab");
            Draw("splitCount");
            Draw("splitMinSpeed");
            Draw("splitMaxSpeed");
            Draw("splitMinAngle");
            Draw("splitMaxAngle");
            Draw("splitLifeTime");

            if (Get("splitPrefab").objectReferenceValue == null)
                EditorGUILayout.HelpBox("Split On Ground needs a Split Prefab, or the ammo just disappears.", MessageType.Warning);
        }
        else
        {
            drawn.Add("splitPrefab");
            drawn.Add("splitCount");
            drawn.Add("splitMinSpeed");
            drawn.Add("splitMaxSpeed");
            drawn.Add("splitMinAngle");
            drawn.Add("splitMaxAngle");
            drawn.Add("splitLifeTime");
        }

        // Player ammo is live at once and timed by the weapon's Effective Time, so both are enemy-only.
        bool isEnemyAmmo = Matches(side, (int)AmmoSide.Enemy);

        if (isEnemyAmmo)
        {
            Section("Collider Window");
            Draw("useColliderWindow");

            if (Get("useColliderWindow").boolValue || Get("useColliderWindow").hasMultipleDifferentValues)
            {
                Draw("armTime");
                Draw("disarmTime");

                if (Get("disarmTime").floatValue <= Get("armTime").floatValue)
                    EditorGUILayout.HelpBox("Disarm Time is not past Arm Time, so the collider arms and stays armed.", MessageType.Info);
            }
            else
            {
                drawn.Add("armTime");
                drawn.Add("disarmTime");
            }

            Section("Lifetime");
            Draw("lifeTime");
        }
        else
        {
            drawn.Add("useColliderWindow");
            drawn.Add("armTime");
            drawn.Add("disarmTime");
            drawn.Add("lifeTime");

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("Lifetime comes from the weapon's Effective Time (Item Data Equipment).", MessageType.None);
        }

        DrawEverythingElse();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>Draws any property this inspector does not know about, so subclasses and new fields still appear.</summary>
    private void DrawEverythingElse()
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        bool headerDrawn = false;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (drawn.Contains(property.propertyPath))
                continue;

            if (!headerDrawn)
            {
                Section("Other");
                headerDrawn = true;
            }

            EditorGUILayout.PropertyField(property, true);
        }
    }

    private SerializedProperty Get(string _name) => serializedObject.FindProperty(_name);

    private void Draw(string _name)
    {
        SerializedProperty property = Get(_name);

        if (property == null)
            return;

        EditorGUILayout.PropertyField(property, true);
        drawn.Add(property.propertyPath);
    }

    private void Draw(SerializedProperty _property)
    {
        if (_property == null)
            return;

        EditorGUILayout.PropertyField(_property, true);
        drawn.Add(_property.propertyPath);
    }

    private static void Section(string _title)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
    }

    /// <summary>True when the enum sits on this value, or when a multi selection disagrees and everything should stay visible.</summary>
    private static bool Matches(SerializedProperty _enumProperty, int _value)
    {
        if (_enumProperty == null)
            return false;

        return _enumProperty.hasMultipleDifferentValues || _enumProperty.enumValueIndex == _value;
    }
}
