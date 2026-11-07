using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One active skill in a list: its activated icon, its trigger key and a cooldown overlay. While its
/// description is showing, pressing a key makes that the trigger; a key that can't be used changes nothing.
/// </summary>
public class UI_ActiveSkillRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string Hint = "Press a key to change the trigger";

    [SerializeField] private Image fill;
    [SerializeField] private Image icon;
    [Tooltip("A Filled image over the icon, drained as the skill comes off cooldown.")]
    [SerializeField] private Image cooldown;
    [SerializeField] private TextMeshProUGUI keyLabel;

    // Everything a key press can be, mouse buttons too. Gamepad buttons are left out.
    private static readonly KeyCode[] Keys = ((KeyCode[])Enum.GetValues(typeof(KeyCode)))
        .Where(key => key != KeyCode.None && key < KeyCode.JoystickButton0)
        .Distinct()
        .ToArray();

    private ActiveSkillEntry entry;
    private bool interactive;
    private bool listening;
    private HoverDelay hover;

    private HoverDelay Hover => hover ??= new HoverDelay(this);

    public void Show(ActiveSkillEntry _entry, bool _interactive)
    {
        entry = _entry;
        interactive = _interactive;

        SkillInfo info = entry.info;
        SkillJointLook look = SkillTree.Instance != null ? SkillTree.Instance.LookFor(entry.kind) : null;

        if (fill != null && look != null)
            fill.color = info.ActiveIcon != null ? Color.clear : look.activeFill;

        if (icon != null)
        {
            icon.sprite = info.ActiveIcon != null ? info.ActiveIcon : info.Icon;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
        }

        if (keyLabel != null)
            keyLabel.text = Skill.KeyName(entry.key);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactive || entry == null) return;

        Hover.Start(() =>
        {
            listening = true;
            ShowDescription(Hint);
        });
    }

    public void OnPointerExit(PointerEventData eventData) => StopHovering();

    private void OnDisable() => StopHovering();

    private void StopHovering()
    {
        if (!interactive) return;

        Hover.Stop();
        listening = false;

        if (SkillTree.Instance != null)
            SkillTree.Instance.Description.Hide();
    }

    private void Update()
    {
        if (entry == null) return;

        if (cooldown != null)
            cooldown.fillAmount = entry.CooldownFraction;

        if (listening && Input.anyKeyDown)
            ListenForKey();
    }

    private void ListenForKey()
    {
        foreach (KeyCode key in Keys)
        {
            if (!Input.GetKeyDown(key)) continue;

            bool bound = Skill.Instance.TryBindKey(entry, key, out string problem);
            ShowDescription(bound ? Hint : problem);
            return;
        }
    }

    private void ShowDescription(string _hint)
    {
        if (SkillTree.Instance != null)
            SkillTree.Instance.Description.Show(entry, _hint);
    }
}
