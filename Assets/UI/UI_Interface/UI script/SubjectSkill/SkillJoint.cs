using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One joint of the skill tree. Create and connect these in the Scene view: select one and drag from the
/// dot beside it (see SkillJointEditor). SkillTree works out every joint's state, so a joint on a closed
/// page is still up to date; lines between joints are drawn by SkillLinks.
/// </summary>
[DisallowMultipleComponent]
public class SkillJoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SkillJointKind kind;
    [Tooltip("Every one of these has to be activated before this joint can be. The whole tree has to be connected through them.")]
    public List<SkillJoint> previous = new List<SkillJoint>();
    [Tooltip("Cyberware Skill joints show what the installed cyberware has instead of this.")]
    public SkillInfo info = new SkillInfo();

    [Tooltip("Cyberware Skill only: which entry of the installed item's Cyberware Skills this joint shows. No such entry = hidden.")]
    [Min(0)] public int cyberwareSlot;

    [Tooltip("Cyberware only: off = only items of Accepted Type show in this joint's list.")]
    public bool acceptsAnyType = true;
    public CyberwareType acceptedType;

    [Header("Parts")]
    public Image fill;
    public Image icon;

    public SkillJointState State { get; private set; }
    public bool IsActive => State == SkillJointState.Activated;
    /// <summary>False while a Cyberware Skill joint has nothing to show, and for anything hanging off a hidden joint.</summary>
    public bool IsVisible { get; private set; } = true;
    /// <summary>Cyberware joints: what's installed.</summary>
    public ItemData Installed { get; set; }
    /// <summary>Basic and Special joints: skill points were paid. Cyberware skills keep this on their item instead.</summary>
    public bool Purchased { get; set; }
    /// <summary>Cyberware Skill joints: the Cyberware joint they extend from.</summary>
    public SkillJoint Owner { get; private set; }

    private SkillTree tree;
    private SkillInfo applied;
    private HoverDelay hover;
    private bool hovered;

    private HoverDelay Hover => hover ??= new HoverDelay(this);

    /// <summary>What this joint shows and gives right now. Null for a Cyberware Skill joint with no matching entry.</summary>
    public SkillInfo Content
    {
        get
        {
            if (kind != SkillJointKind.CyberwareSkill)
                return info;

            ItemData item = Owner != null ? Owner.Installed : null;
            if (item == null || item.cyberwareSkills == null || cyberwareSlot >= item.cyberwareSkills.Count)
                return null;

            return item.cyberwareSkills[cyberwareSlot];
        }
    }

    public bool Accepts(ItemData _item) =>
        kind == SkillJointKind.Cyberware && _item != null && _item.ItemType == ItemType.Cyberware
        && (acceptsAnyType || _item.cyberwareType == acceptedType);

    /// <summary>The nearest Cyberware joint up the previous joints.</summary>
    public SkillJoint FindOwner()
    {
        var seen = new HashSet<SkillJoint>();
        var queue = new Queue<SkillJoint>(previous);

        while (queue.Count > 0)
        {
            SkillJoint joint = queue.Dequeue();
            if (joint == null || !seen.Add(joint)) continue;
            if (joint.kind == SkillJointKind.Cyberware) return joint;

            foreach (SkillJoint earlier in joint.previous)
                queue.Enqueue(earlier);
        }

        return null;
    }

    /// <summary>True when _joint has to be activated, directly or further up, before this one can be.</summary>
    public bool DependsOn(SkillJoint _joint)
    {
        var seen = new HashSet<SkillJoint>();
        var stack = new Stack<SkillJoint>(previous);

        while (stack.Count > 0)
        {
            SkillJoint joint = stack.Pop();
            if (joint == null || !seen.Add(joint)) continue;
            if (joint == _joint) return true;

            foreach (SkillJoint earlier in joint.previous)
                stack.Push(earlier);
        }

        return false;
    }

    public void Bind(SkillTree _tree)
    {
        tree = _tree;
        Owner = kind == SkillJointKind.CyberwareSkill ? FindOwner() : null;
    }

    /// <summary>Works out this joint from its previous joints, which SkillTree has already done, and applies what changed.</summary>
    public void Evaluate(PlayerStats _stats, Skill _skill)
    {
        bool previousActive = true;
        bool previousVisible = true;

        foreach (SkillJoint joint in previous)
        {
            if (joint == null) continue;
            previousActive &= joint.IsActive;
            previousVisible &= joint.IsVisible;
        }

        SkillInfo content = Content;
        bool activated;

        switch (kind)
        {
            case SkillJointKind.Cyberware:
                IsVisible = previousVisible;
                activated = previousActive && Installed != null;
                break;

            case SkillJointKind.CyberwareSkill:
                IsVisible = previousVisible && content != null;
                activated = IsVisible && previousActive
                         && (content.activatedAtStart || tree.IsUnlocked(Owner.Installed, cyberwareSlot));
                break;

            default:
                IsVisible = previousVisible;
                activated = previousActive && (Purchased || content.activatedAtStart);
                break;
        }

        State = activated ? SkillJointState.Activated
              : previousActive ? SkillJointState.Ready
              : SkillJointState.Locked;

        // A Cyberware joint only holds the item; what it gives is on the joints it opens.
        SkillInfo shouldGive = activated && kind != SkillJointKind.Cyberware ? content : null;

        if (applied != shouldGive)
        {
            applied?.Remove(_stats, _skill);
            shouldGive?.Apply(_stats, _skill, kind);
            applied = shouldGive;
        }

        if (gameObject.activeSelf != IsVisible)
            gameObject.SetActive(IsVisible);

        ShowLook(tree.LookFor(kind), State, content, kind == SkillJointKind.Cyberware ? Installed : null);
    }

    /// <summary>Shape behind, line art in front. An installed item's icon replaces a Cyberware joint's own.</summary>
    public void ShowLook(SkillJointLook _look, SkillJointState _state, SkillInfo _content, ItemData _installed)
    {
        Sprite line = _installed != null ? _installed.icon : _content?.icon;
        Sprite activeLine = _installed == null ? _content?.activeIcon : null;
        bool ownActiveSprite = _state == SkillJointState.Activated && activeLine != null;

        if (fill != null)
        {
            fill.sprite = _look.shape;
            // An icon with its own activated sprite already has its background.
            fill.color = activeLine != null ? Color.clear : _look.Fill(_state);
        }

        if (icon != null)
        {
            icon.sprite = ownActiveSprite ? activeLine : line;
            icon.enabled = icon.sprite != null;
            icon.color = _look.Icon(_state);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        Hover.Stop();

        if (eventData.dragging || tree == null) return;

        Hover.Start(() => { if (hovered) tree.Description.Show(this); });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        Hover.Stop();

        if (tree != null)
            tree.Description.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || tree == null) return;

        Hover.Stop();
        tree.JointClicked(this);
    }

    private void OnDisable()
    {
        Hover.Stop();

        if (hovered && tree != null)
            tree.Description.Hide();

        hovered = false;
    }
}
