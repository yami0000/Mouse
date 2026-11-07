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
    [Tooltip("Every one of these has to be activated before this joint can be, except that a Cyberware joint ready for installing already counts. The whole tree has to be connected through them.")]
    public List<SkillJoint> previous = new List<SkillJoint>();
    [Tooltip("Cyberware Skill joints show what the installed cyberware has instead of this.")]
    public SkillInfo info = new SkillInfo();

    [Tooltip("Cyberware Skill only: which entry of the installed item's Cyberware Skills this joint shows. No such entry = hidden.")]
    [Min(0)] public int cyberwareSlot;

    [Tooltip("Cyberware only: off = only items of Accepted Type show in this joint's list.")]
    public bool acceptsAnyType = true;
    public CyberwareType acceptedType;
    [Tooltip("Cyberware only: already in this joint when the game starts, without coming from the bag. A previous joint that isn't activated still holds it back, and taking it out puts it in the bag.")]
    public ItemData preinstalled;

    [Header("Parts")]
    [Tooltip("The background behind the icon. Set its Source Image on the Image itself: this joint only changes its colour, and only in Play mode.")]
    public Image fill;
    public Image icon;
    [Tooltip("Cyberware only: a child Image showing the installed item, on top of the icon. Set its size and rotation on its own Rect Transform.")]
    public Image cyberwareImage;

    public SkillJointState State { get; private set; }
    public bool IsActive => State == SkillJointState.Activated;
    /// <summary>Lets the joints after this one go ahead: it's activated, or it's a Cyberware joint ready for installing.</summary>
    public bool OpensNext => IsActive || (kind == SkillJointKind.Cyberware && State == SkillJointState.Ready);
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

        if (kind != SkillJointKind.Cyberware || preinstalled == null) return;

        if (Accepts(preinstalled))
            Installed = preinstalled;
        else
            Debug.LogWarning($"[SkillTree] {name} can't take its Pre-installed {preinstalled.name}: it isn't a cyberware this joint accepts.", this);
    }

    /// <summary>Works out this joint from its previous joints, which SkillTree has already done, and applies what changed.</summary>
    public void Evaluate(PlayerStats _stats, Skill _skill)
    {
        bool previousOpen = true;
        bool previousVisible = true;

        foreach (SkillJoint joint in previous)
        {
            if (joint == null) continue;
            previousOpen &= joint.OpensNext;
            previousVisible &= joint.IsVisible;
        }

        SkillInfo content = Content;
        bool activated;

        switch (kind)
        {
            case SkillJointKind.Cyberware:
                IsVisible = previousVisible;
                activated = previousOpen && Installed != null;
                break;

            case SkillJointKind.CyberwareSkill:
                IsVisible = previousVisible && content != null;
                activated = IsVisible && previousOpen
                         && (content.activatedAtStart || tree.IsUnlocked(Owner.Installed, cyberwareSlot));
                break;

            default:
                IsVisible = previousVisible;
                activated = previousOpen && (Purchased || content.activatedAtStart);
                break;
        }

        State = activated ? SkillJointState.Activated
              : previousOpen ? SkillJointState.Ready
              : SkillJointState.Locked;

        // A Cyberware joint gives what its item gives, which also comes off when the item does.
        SkillInfo shouldGive = !activated ? null : kind == SkillJointKind.Cyberware ? Installed.cyberwareGives : content;

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

    /// <summary>
    /// Coloured background behind, line art in front, and on a Cyberware joint the installed item on top of that.
    /// Only runs in Play mode. The background's Source Image is yours to set on the Image: only its colour is changed here.
    /// </summary>
    private void ShowLook(SkillJointLook _look, SkillJointState _state, SkillInfo _content, ItemData _installed)
    {
        Sprite line = _content?.Icon;
        Sprite activeLine = _content?.ActiveIcon;
        bool ownActiveSprite = _state == SkillJointState.Activated && activeLine != null;

        // An icon with its own activated sprite already has its background.
        if (fill != null)
            fill.color = activeLine != null ? Color.clear : _look.Fill(_state);

        if (icon != null)
        {
            icon.sprite = ownActiveSprite ? activeLine : line;
            icon.enabled = icon.sprite != null;
            icon.color = _look.Icon(_state);
        }

        if (cyberwareImage != null)
        {
            cyberwareImage.sprite = _installed != null ? _installed.icon : null;
            cyberwareImage.enabled = cyberwareImage.sprite != null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => UpdateHover(eventData);

    public void OnPointerExit(PointerEventData eventData) => UpdateHover(eventData);

    // A joint placed inside another in the Hierarchy sends these to both. So each one only reports
    // what's really under the pointer, and the tree shows that one's description.
    private void UpdateHover(PointerEventData _eventData)
    {
        if (tree == null) return;

        GameObject under = _eventData.dragging ? null : _eventData.pointerCurrentRaycast.gameObject;
        tree.SetHovered(under != null ? under.GetComponentInParent<SkillJoint>() : null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || tree == null) return;

        tree.JointClicked(this);
    }

    private void OnDisable()
    {
        if (tree != null && tree.Hovered == this)
            tree.SetHovered(null);
    }
}
