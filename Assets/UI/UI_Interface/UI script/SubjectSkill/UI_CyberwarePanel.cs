using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Opens from a Cyberware joint: what's installed in it, and every cyberware it accepts that the player
/// is carrying. Right-clicking a carried one installs it; right-clicking the installed one takes it out.
/// </summary>
public class UI_CyberwarePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private UI_CyberwareSlot installedSlot;
    [Tooltip("Copied once per carried cyberware. Stays switched off itself; restyle it to restyle them all.")]
    [SerializeField] private UI_CyberwareSlot slotTemplate;
    [Tooltip("Shown while the player carries nothing this joint accepts.")]
    [SerializeField] private GameObject emptyMessage;

    private readonly List<UI_CyberwareSlot> slots = new List<UI_CyberwareSlot>();
    private SkillJoint joint;

    public void Open(SkillJoint _joint)
    {
        joint = _joint;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        joint = null;
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        title.text = joint.info.skillName;

        installedSlot.Show(this, joint.Installed != null ? new InventoryItem(joint.Installed) : null);

        List<InventoryItem> carried = Carried();

        while (slots.Count < carried.Count)
            slots.Add(Instantiate(slotTemplate, slotTemplate.transform.parent));

        for (int i = 0; i < slots.Count; i++)
        {
            bool used = i < carried.Count;
            slots[i].gameObject.SetActive(used);

            if (used)
                slots[i].Show(this, carried[i]);
        }

        if (emptyMessage != null)
            emptyMessage.SetActive(carried.Count == 0);
    }

    private List<InventoryItem> Carried()
    {
        var carried = new List<InventoryItem>();
        Inventory bag = Inventory.Instance;

        foreach (List<InventoryItem> list in new[] { bag.inventory, bag.stash })
            foreach (InventoryItem item in list)
                if (joint.Accepts(item.data))
                    carried.Add(item);

        return carried;
    }

    public void SlotRightClicked(UI_CyberwareSlot _slot)
    {
        if (joint == null || _slot.Item == null) return;

        if (_slot == installedSlot)
            SkillTree.Instance.Uninstall(joint);
        else
            SkillTree.Instance.Install(joint, _slot.Item.data);

        Refresh();
    }
}
