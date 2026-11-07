using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A joint's inspector shows only what its kind uses. In the Scene view, a selected joint gets a dot to drag
/// from: drop it on a joint to connect them (or disconnect, if they already are), or on empty space to make
/// a new joint after this one. Red squares on its lines disconnect them. Moving joints is just the usual
/// Move/Rect tool, and SkillLinks redraws the lines.
/// </summary>
[CustomEditor(typeof(SkillJoint))]
[CanEditMultipleObjects]
public class SkillJointEditor : Editor
{
    private static int draggingFrom;
    private static Vector3 dragEnd;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        SerializedProperty kind = serializedObject.FindProperty("kind");
        EditorGUILayout.PropertyField(kind);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("previous"), true);
        EditorGUILayout.Space();

        if (!kind.hasMultipleDifferentValues)
            DrawKindFields((SkillJointKind)kind.enumValueIndex);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fill"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));

        bool changed = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        SkillJoint joint = (SkillJoint)target;
        SkillTree tree = joint.GetComponentInParent<SkillTree>(true);

        if (changed && tree != null)
        {
            SyncNames();
            SkillTreeAuthoring.RefreshLines(tree);
        }

        DrawCyberwareImageButton();

        if (targets.Length == 1 && tree != null)
            DrawAddButtons(tree, joint);
    }

    // For joints made before Cyberware Image existed, or changed to Cyberware afterwards.
    private void DrawCyberwareImageButton()
    {
        SkillJoint[] missing = targets.Cast<SkillJoint>()
            .Where(joint => joint.kind == SkillJointKind.Cyberware && joint.cyberwareImage == null)
            .ToArray();

        if (missing.Length == 0 || !GUILayout.Button("Create Cyberware Image")) return;

        foreach (SkillJoint joint in missing)
            SkillTreeAuthoring.CreateCyberwareImage(joint);
    }

    private void DrawKindFields(SkillJointKind _kind)
    {
        SerializedProperty info = serializedObject.FindProperty("info");

        switch (_kind)
        {
            case SkillJointKind.Cyberware:
                EditorGUILayout.HelpBox("Costs no skill points: activated by installing a cyberware. Left-click opens the list of carried cyberware once it's ready.", MessageType.None);
                EditorGUILayout.PropertyField(info.FindPropertyRelative("skillName"));
                EditorGUILayout.PropertyField(info.FindPropertyRelative("description"));
                EditorGUILayout.PropertyField(info.FindPropertyRelative("icon"));
                EditorGUILayout.PropertyField(info.FindPropertyRelative("activeIcon"), new GUIContent("Active Icon", "Replaces Icon once a cyberware is installed, with the item's image on top of it."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cyberwareImage"));

                SerializedProperty any = serializedObject.FindProperty("acceptsAnyType");
                EditorGUILayout.PropertyField(any);
                if (any.hasMultipleDifferentValues || !any.boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("acceptedType"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("preinstalled"), new GUIContent("Pre-installed"));
                break;

            case SkillJointKind.CyberwareSkill:
                EditorGUILayout.HelpBox("Hidden until the Cyberware joint before it has a cyberware. Name, icon, cost and effects come from that item's Cyberware Skills, entry number Slot.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cyberwareSlot"), new GUIContent("Slot"));
                break;

            default:
                EditorGUILayout.PropertyField(info, true);

                if (((SkillJoint)target).info.IsListed && GUILayout.Button("Edit in Skill List"))
                    SkillTreeAuthoring.OpenSkillList();
                break;
        }
    }

    // Keeps the Hierarchy readable: the object goes by its skill's name.
    private void SyncNames()
    {
        foreach (Object selected in targets)
        {
            SkillJoint joint = (SkillJoint)selected;

            string wanted = joint.kind == SkillJointKind.CyberwareSkill
                ? $"{SkillTreeAuthoring.Label(joint.kind)} {joint.cyberwareSlot}"
                : string.IsNullOrEmpty(joint.info.Name) ? null : joint.info.Name;

            if (wanted != null && joint.name != wanted)
            {
                Undo.RecordObject(joint.gameObject, "Rename Skill Joint");
                joint.name = wanted;
            }
        }
    }

    private static void DrawAddButtons(SkillTree _tree, SkillJoint _joint)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add a joint after this one", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (SkillJointKind kind in System.Enum.GetValues(typeof(SkillJointKind)))
            {
                using (new EditorGUI.DisabledScope(kind == SkillJointKind.CyberwareSkill && !CanOwnCyberwareSkill(_joint)))
                    if (GUILayout.Button(SkillTreeAuthoring.Label(kind)))
                        SkillTreeAuthoring.CreateJoint(_tree, kind, SkillTreeAuthoring.PositionAfter(_tree, _joint), _joint);
            }
        }
    }

    private static bool CanOwnCyberwareSkill(SkillJoint _joint) =>
        _joint.kind == SkillJointKind.Cyberware || _joint.FindOwner() != null;

    private void OnSceneGUI()
    {
        SkillJoint joint = (SkillJoint)target;
        SkillTree tree = joint.GetComponentInParent<SkillTree>(true);
        if (tree == null || tree.JointRoot == null) return;

        DrawDisconnectHandles(tree, joint);
        HandleConnectDot(tree, joint);
    }

    private static void DrawDisconnectHandles(SkillTree _tree, SkillJoint _joint)
    {
        Handles.color = new Color(1f, 0.3f, 0.3f);

        foreach (SkillJoint earlier in _joint.previous.ToArray())
            if (earlier != null && DisconnectButton(earlier, _joint))
                SkillTreeAuthoring.RemoveLink(earlier, _joint);

        foreach (SkillJoint later in _tree.JointRoot.GetComponentsInChildren<SkillJoint>(true))
            if (later.previous.Contains(_joint) && DisconnectButton(_joint, later))
                SkillTreeAuthoring.RemoveLink(_joint, later);
    }

    private static bool DisconnectButton(SkillJoint _from, SkillJoint _to)
    {
        Vector3 middle = (_from.transform.position + _to.transform.position) * 0.5f;
        float size = HandleUtility.GetHandleSize(middle) * 0.05f;
        return Handles.Button(middle, Quaternion.identity, size, size * 1.5f, Handles.RectangleHandleCap);
    }

    private static void HandleConnectDot(SkillTree _tree, SkillJoint _joint)
    {
        RectTransform rect = (RectTransform)_joint.transform;
        Vector3 dot = rect.TransformPoint(new Vector3(rect.rect.xMax * 1.35f, rect.rect.center.y, 0f));
        float size = HandleUtility.GetHandleSize(dot) * 0.08f;

        int id = GUIUtility.GetControlID(FocusType.Passive);
        Event current = Event.current;

        switch (current.GetTypeForControl(id))
        {
            case EventType.Layout:
                HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(dot, size));
                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == id && current.button == 0)
                {
                    GUIUtility.hotControl = id;
                    draggingFrom = id;
                    dragEnd = dot;
                    current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    TryPointOnMap(_tree, current.mousePosition, out dragEnd);
                    current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    draggingFrom = 0;
                    current.Use();

                    if (TryPointOnMap(_tree, current.mousePosition, out Vector3 end))
                        FinishDrag(_tree, _joint, end);
                }
                break;

            case EventType.Repaint:
                Handles.color = new Color(0.45f, 0.85f, 1f);
                Handles.DotHandleCap(id, dot, Quaternion.identity, size, EventType.Repaint);

                if (draggingFrom == id)
                    Handles.DrawLine(dot, dragEnd);
                break;
        }
    }

    private static bool TryPointOnMap(SkillTree _tree, Vector2 _guiPoint, out Vector3 _point)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(_guiPoint);
        Plane map = new Plane(_tree.JointRoot.forward, _tree.JointRoot.position);

        if (map.Raycast(ray, out float distance))
        {
            _point = ray.GetPoint(distance);
            return true;
        }

        _point = Vector3.zero;
        return false;
    }

    private static void FinishDrag(SkillTree _tree, SkillJoint _from, Vector3 _end)
    {
        foreach (SkillJoint other in _tree.JointRoot.GetComponentsInChildren<SkillJoint>(false))
        {
            RectTransform rect = (RectTransform)other.transform;

            if (other == _from || !rect.rect.Contains(rect.InverseTransformPoint(_end)))
                continue;

            string problem = SkillTreeAuthoring.ToggleLink(_from, other);
            if (problem != null && SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent(problem));

            return;
        }

        ShowNewJointMenu(_tree, _from, _end);
    }

    private static void ShowNewJointMenu(SkillTree _tree, SkillJoint _from, Vector3 _end)
    {
        Vector2 position = _tree.JointRoot.InverseTransformPoint(_end);
        GenericMenu menu = new GenericMenu();

        foreach (SkillJointKind kind in System.Enum.GetValues(typeof(SkillJointKind)))
        {
            GUIContent label = new GUIContent($"New {SkillTreeAuthoring.Label(kind)} Joint");

            if (kind == SkillJointKind.CyberwareSkill && !CanOwnCyberwareSkill(_from))
                menu.AddDisabledItem(label);
            else
                menu.AddItem(label, false, () => SkillTreeAuthoring.CreateJoint(_tree, kind, position, _from));
        }

        menu.ShowAsContext();
    }
}
