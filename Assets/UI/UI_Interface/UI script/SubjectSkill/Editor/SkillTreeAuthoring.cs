using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>What the skill tree's inspectors and Scene view tools do: creating, connecting and checking joints. All undoable.</summary>
public static class SkillTreeAuthoring
{
    public const float DefaultJointSize = 120f;
    public const float ChildDistance = 260f;

    public static string Label(SkillJointKind _kind) => ObjectNames.NicifyVariableName(_kind.ToString());

    /// <summary>A new joint at _position on the map, after _previous when there is one. Selected, ready to fill in.</summary>
    public static SkillJoint CreateJoint(SkillTree _tree, SkillJointKind _kind, Vector2 _position, SkillJoint _previous = null)
    {
        GameObject go = new GameObject($"{Label(_kind)} Joint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SkillJoint));
        Undo.RegisterCreatedObjectUndo(go, "Create Skill Joint");

        go.layer = _tree.JointRoot.gameObject.layer;
        go.transform.SetParent(_tree.JointRoot, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.sizeDelta = Vector2.one * DefaultJointSize;
        rect.anchoredPosition = _position;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.layer = go.layer;
        iconObject.transform.SetParent(go.transform, false);

        RectTransform iconRect = (RectTransform)iconObject.transform;
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;

        Image icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.preserveAspect = true;

        SkillJoint joint = go.GetComponent<SkillJoint>();
        joint.kind = _kind;
        joint.fill = go.GetComponent<Image>();
        joint.icon = icon;

        if (_kind == SkillJointKind.Cyberware)
            joint.info.cost = 0;

        if (_previous != null)
            joint.previous.Add(_previous);

        if (_kind == SkillJointKind.CyberwareSkill)
            joint.cyberwareSlot = NextSlot(_tree, joint);

        RefreshPreview(_tree);
        Selection.activeGameObject = go;
        return joint;
    }

    /// <summary>A spot beside _joint for a new one, moved along until nothing is in the way.</summary>
    public static Vector2 PositionAfter(SkillTree _tree, SkillJoint _joint)
    {
        RectTransform rect = (RectTransform)_joint.transform;
        Vector2 spot = rect.anchoredPosition + Vector2.right * ChildDistance;

        var taken = _tree.JointRoot.GetComponentsInChildren<SkillJoint>(true)
            .Select(joint => ((RectTransform)joint.transform).anchoredPosition).ToList();

        while (taken.Any(position => Vector2.Distance(position, spot) < DefaultJointSize))
            spot += Vector2.down * (DefaultJointSize * 1.3f);

        return spot;
    }

    // The first slot free among the joints extending from the same Cyberware joint.
    private static int NextSlot(SkillTree _tree, SkillJoint _joint)
    {
        SkillJoint owner = _joint.FindOwner();

        IEnumerable<int> used = _tree.JointRoot.GetComponentsInChildren<SkillJoint>(true)
            .Where(joint => joint != _joint && joint.kind == SkillJointKind.CyberwareSkill && joint.FindOwner() == owner)
            .Select(joint => joint.cyberwareSlot);

        int slot = 0;
        while (used.Contains(slot))
            slot++;

        return slot;
    }

    /// <summary>Connects _from to _to, or disconnects them if they already are. Null = fine; otherwise why it wasn't done.</summary>
    public static string ToggleLink(SkillJoint _from, SkillJoint _to)
    {
        if (_from == _to) return null;

        if (_to.previous.Contains(_from))
        {
            RemoveLink(_from, _to);
            return null;
        }

        if (_from.DependsOn(_to))
            return $"{_from.name} already comes after {_to.name}: that would be a loop.";

        Undo.RecordObject(_to, "Connect Skill Joints");
        _to.previous.Add(_from);
        EditorUtility.SetDirty(_to);

        // Took this joint out of one cyberware's tree and into another's, or into its first.
        SkillTree tree = _to.GetComponentInParent<SkillTree>(true);
        if (_to.kind == SkillJointKind.CyberwareSkill && tree != null)
            _to.cyberwareSlot = NextSlot(tree, _to);

        RefreshLines(_to);
        return null;
    }

    public static void RemoveLink(SkillJoint _from, SkillJoint _to)
    {
        Undo.RecordObject(_to, "Disconnect Skill Joints");
        _to.previous.Remove(_from);
        EditorUtility.SetDirty(_to);
        RefreshLines(_to);
    }

    public static void RefreshLines(SkillJoint _joint)
    {
        SkillTree tree = _joint.GetComponentInParent<SkillTree>(true);
        if (tree != null && tree.Links != null)
            tree.Links.SetVerticesDirty();
    }

    /// <summary>Shows every joint as activated in the editor, in its kind's look, so it can be seen while placing it.</summary>
    public static void RefreshPreview(SkillTree _tree)
    {
        if (_tree == null || _tree.JointRoot == null || Application.isPlaying) return;

        foreach (SkillJoint joint in _tree.JointRoot.GetComponentsInChildren<SkillJoint>(true))
        {
            // No cyberware to read an icon from yet, so a Cyberware Skill joint is just its shape.
            SkillInfo content = joint.kind == SkillJointKind.CyberwareSkill ? null : joint.info;
            joint.ShowLook(_tree.LookFor(joint.kind), SkillJointState.Activated, content, null);

            if (joint.fill != null) EditorUtility.SetDirty(joint.fill);
            if (joint.icon != null) EditorUtility.SetDirty(joint.icon);
        }

        if (_tree.Links != null)
            _tree.Links.SetVerticesDirty();
    }

    /// <summary>Everything that would stop the tree working as designed. Empty = all good.</summary>
    public static List<string> FindProblems(SkillTree _tree)
    {
        var problems = new List<string>();
        SkillJoint[] joints = _tree.JointRoot.GetComponentsInChildren<SkillJoint>(true);

        SkillTree.Sort(joints, out List<SkillJoint> stuck);
        foreach (SkillJoint joint in stuck)
            problems.Add($"{joint.name}: part of a loop of previous joints, so it can never activate.");

        int groups = CountConnectedGroups(joints);
        if (groups > 1)
            problems.Add($"The tree is in {groups} separate pieces: every joint has to be connected to the rest.");

        foreach (SkillJoint joint in joints)
        {
            if (joint.previous.Any(earlier => earlier == null))
                problems.Add($"{joint.name}: has an empty entry in Previous.");

            if (joint.kind == SkillJointKind.CyberwareSkill && joint.FindOwner() == null)
                problems.Add($"{joint.name}: a Cyberware Skill joint has to come after a Cyberware joint, or it can never show.");
        }

        foreach (var sameSlot in joints.Where(joint => joint.kind == SkillJointKind.CyberwareSkill && joint.FindOwner() != null)
                                       .GroupBy(joint => (joint.FindOwner(), joint.cyberwareSlot)))
            if (sameSlot.Count() > 1)
                problems.Add($"{string.Join(" and ", sameSlot.Select(joint => joint.name))}: same Slot under {sameSlot.Key.Item1.name}, so they'd show the same skill.");

        foreach (var sameSkill in joints.Where(joint => joint.kind != SkillJointKind.CyberwareSkill
                                                     && joint.kind != SkillJointKind.Cyberware
                                                     && joint.info.activeSkill != ActiveSkillId.None)
                                        .GroupBy(joint => joint.info.activeSkill))
            if (sameSkill.Count() > 1)
                problems.Add($"{string.Join(" and ", sameSkill.Select(joint => joint.name))}: all give {sameSkill.Key}.");

        return problems;
    }

    private static int CountConnectedGroups(SkillJoint[] _joints)
    {
        var neighbours = _joints.ToDictionary(joint => joint, joint => new List<SkillJoint>());

        foreach (SkillJoint joint in _joints)
            foreach (SkillJoint earlier in joint.previous)
                if (earlier != null && neighbours.ContainsKey(earlier))
                {
                    neighbours[joint].Add(earlier);
                    neighbours[earlier].Add(joint);
                }

        var seen = new HashSet<SkillJoint>();
        int groups = 0;

        foreach (SkillJoint start in _joints)
        {
            if (!seen.Add(start)) continue;
            groups++;

            var stack = new Stack<SkillJoint>();
            stack.Push(start);

            while (stack.Count > 0)
                foreach (SkillJoint next in neighbours[stack.Pop()])
                    if (seen.Add(next))
                        stack.Push(next);
        }

        return groups;
    }

    /// <summary>Logs the tree's problems, each as its own warning, or that there are none.</summary>
    public static void LogProblems(SkillTree _tree)
    {
        List<string> problems = FindProblems(_tree);

        if (problems.Count == 0)
            Debug.Log("[SkillTree] No problems found.", _tree);

        foreach (string problem in problems)
            Debug.LogWarning($"[SkillTree] {problem}", _tree);
    }
}
