using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the tree together: who may activate what, and installing cyberware. Every change ends in
/// Recalculate, which walks the joints in order and works out each one from what's before it. So a
/// cyberware coming out takes its Cyberware Skill joints, and anything hanging off them, with it, and
/// they come back when it goes in again. An empty socket that's ready doesn't hold back other joints.
/// Sits on the always-active UI_Skill, so all of this works while the page itself is closed.
/// </summary>
public class SkillTree : MonoBehaviour
{
    public static SkillTree Instance;

    [Header("Parts")]
    [Tooltip("Every SkillJoint under this belongs to the tree.")]
    [SerializeField] private RectTransform jointRoot;
    [SerializeField] private SkillLinks links;
    [SerializeField] private SkillDescription description;
    [SerializeField] private UI_CyberwarePanel cyberwarePanel;

    [Header("Joint Looks")]
    [Tooltip("One per kind of joint.")]
    [SerializeField] private List<SkillJointLook> looks = new List<SkillJointLook>
    {
        new SkillJointLook { kind = SkillJointKind.Basic, activeFill = new Color(0.25f, 0.62f, 0.85f) },
        new SkillJointLook { kind = SkillJointKind.Special, activeFill = new Color(0.95f, 0.65f, 0.2f) },
        new SkillJointLook { kind = SkillJointKind.Cyberware, activeFill = new Color(0.85f, 0.25f, 0.45f) },
        new SkillJointLook { kind = SkillJointKind.CyberwareSkill, activeFill = new Color(0.55f, 0.35f, 0.9f) }
    };

    private static readonly SkillJointLook FallbackLook = new SkillJointLook();

    private List<SkillJoint> joints = new List<SkillJoint>();
    private PlayerStats playerStats;
    private HoverDelay hoverDelay;

    // Paid-for skills are remembered per cyberware, not per joint, so they survive being uninstalled.
    private readonly Dictionary<ItemData, HashSet<int>> unlockedSkills = new Dictionary<ItemData, HashSet<int>>();

    public RectTransform JointRoot => jointRoot;
    public SkillLinks Links => links;
    public SkillDescription Description => description;
    public IReadOnlyList<SkillJoint> Joints => joints;
    /// <summary>The joint really under the pointer. Null when there isn't one.</summary>
    public SkillJoint Hovered { get; private set; }

    private HoverDelay HoverTimer => hoverDelay ??= new HoverDelay(this);

    private void Awake()
    {
        Instance = this;

        joints = Sort(jointRoot.GetComponentsInChildren<SkillJoint>(true), out List<SkillJoint> stuck);

        foreach (SkillJoint joint in stuck)
            Debug.LogError($"[SkillTree] {joint.name} is part of a loop of previous joints, so it can never activate.", joint);

        foreach (SkillJoint joint in joints)
            joint.Bind(this);
    }

    private void Start()
    {
        playerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();

        foreach (SkillJoint joint in joints)
            if (joint.Installed != null)
                WarnAboutMissingSlots(joint, joint.Installed);

        Recalculate();
    }

    /// <summary>
    /// _joints with each one after all its previous joints, so one pass can work them all out.
    /// Joints in a loop have no such place: they come back in _stuck instead.
    /// </summary>
    public static List<SkillJoint> Sort(IEnumerable<SkillJoint> _joints, out List<SkillJoint> _stuck)
    {
        var sorted = new List<SkillJoint>();
        var waiting = new List<SkillJoint>(_joints);
        var done = new HashSet<SkillJoint>();
        bool progress = true;

        while (progress && waiting.Count > 0)
        {
            progress = false;

            for (int i = 0; i < waiting.Count; i++)
            {
                SkillJoint joint = waiting[i];

                if (!joint.previous.TrueForAll(earlier => earlier == null || done.Contains(earlier)))
                    continue;

                sorted.Add(joint);
                done.Add(joint);
                waiting.RemoveAt(i--);
                progress = true;
            }
        }

        _stuck = waiting;
        return sorted;
    }

    public SkillJointLook LookFor(SkillJointKind _kind)
    {
        foreach (SkillJointLook look in looks)
            if (look.kind == _kind)
                return look;

        return FallbackLook;
    }

    public void Recalculate()
    {
        foreach (SkillJoint joint in joints)
            joint.Evaluate(playerStats, Skill.Instance);

        if (links != null)
            links.SetVerticesDirty();
    }

    public bool IsUnlocked(ItemData _cyberware, int _slot) =>
        _cyberware != null && unlockedSkills.TryGetValue(_cyberware, out HashSet<int> slots) && slots.Contains(_slot);

    /// <summary>Hides the description of the joint the pointer left, and after the usual delay shows _joint's.</summary>
    public void SetHovered(SkillJoint _joint)
    {
        if (Hovered == _joint) return;

        Hovered = _joint;
        HoverTimer.Stop();
        description.Hide();

        if (_joint != null)
            HoverTimer.Start(() => description.Show(_joint));
    }

    public void JointClicked(SkillJoint _joint)
    {
        HoverTimer.Stop();

        if (_joint.kind == SkillJointKind.Cyberware)
        {
            description.Hide();

            if (_joint.State != SkillJointState.Locked || _joint.Installed != null)
                cyberwarePanel.Open(_joint);
            else
                cyberwarePanel.Close();

            return;
        }

        cyberwarePanel.Close();

        if (TryActivate(_joint))
            description.Show(_joint); // the pointer is still on it, and what it says just changed
        else
            description.Hide();
    }

    public void CloseCyberwarePanel() => cyberwarePanel.Close();

    /// <summary>Pays for a Ready joint and activates it. Cyberware joints activate by installing instead.</summary>
    public bool TryActivate(SkillJoint _joint)
    {
        if (_joint.kind == SkillJointKind.Cyberware || _joint.State != SkillJointState.Ready) return false;

        SkillInfo content = _joint.Content;
        if (content == null || !Skill.Instance.HasEnoughPoints(content.cost)) return false;

        Skill.Instance.SpendPoints(content.cost);

        if (_joint.kind == SkillJointKind.CyberwareSkill)
        {
            ItemData item = _joint.Owner.Installed;

            if (!unlockedSkills.TryGetValue(item, out HashSet<int> slots))
                unlockedSkills[item] = slots = new HashSet<int>();

            slots.Add(_joint.cyberwareSlot);
        }
        else
        {
            _joint.Purchased = true;
        }

        Recalculate();
        return true;
    }

    /// <summary>Moves _item from the bag into the joint. Whatever was installed goes back into the bag.</summary>
    public bool Install(SkillJoint _joint, ItemData _item)
    {
        if (!_joint.Accepts(_item)) return false;
        if (_joint.State == SkillJointState.Locked && _joint.Installed == null) return false;
        if (Inventory.Instance.RemoveItem(_item) == 0) return false;

        if (_joint.Installed != null)
            Inventory.Instance.AddItem(_joint.Installed, 1, notify: false);

        _joint.Installed = _item;

        WarnAboutMissingSlots(_joint, _item);
        Recalculate();
        return true;
    }

    /// <summary>Back into the bag. Its joints hide and stop giving anything; skill points stay spent.</summary>
    public void Uninstall(SkillJoint _joint)
    {
        if (_joint.Installed == null) return;

        Inventory.Instance.AddItem(_joint.Installed, 1, notify: false);
        _joint.Installed = null;

        Recalculate();
    }

    private void WarnAboutMissingSlots(SkillJoint _joint, ItemData _item)
    {
        int count = _item.cyberwareSkills != null ? _item.cyberwareSkills.Count : 0;

        for (int slot = 0; slot < count; slot++)
            if (!joints.Exists(joint => joint.Owner == _joint && joint.cyberwareSlot == slot))
                Debug.LogWarning($"[SkillTree] {_item.name} has a skill for slot {slot}, but {_joint.name} has no Cyberware Skill joint with that slot.", _joint);
    }
}
