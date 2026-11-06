using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One row per active skill the player has, stacked by the Vertical Layout Group on this object.
/// Used twice: on the skill page, where hovering a row lets you change its key, and on the HUD for the cooldowns.
/// </summary>
public class UI_ActiveSkillList : MonoBehaviour
{
    [Tooltip("Copied once per skill. Stays switched off itself; restyle it to restyle them all.")]
    [SerializeField] private UI_ActiveSkillRow rowTemplate;
    [Tooltip("Hovering a row shows its description and listens for a new trigger key.")]
    [SerializeField] private bool interactive = true;

    private readonly List<UI_ActiveSkillRow> rows = new List<UI_ActiveSkillRow>();
    private Skill subscribed;

    private void OnEnable() => Subscribe();

    // Skill's Awake may not have run by this object's first OnEnable.
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed != null)
            subscribed.OnActiveSkillsChanged -= Rebuild;

        subscribed = null;
    }

    private void Subscribe()
    {
        if (subscribed != null || Skill.Instance == null) return;

        subscribed = Skill.Instance;
        subscribed.OnActiveSkillsChanged += Rebuild;
        Rebuild();
    }

    private void Rebuild()
    {
        IReadOnlyList<ActiveSkillEntry> skills = subscribed.ActiveSkills;

        while (rows.Count < skills.Count)
            rows.Add(Instantiate(rowTemplate, transform));

        for (int i = 0; i < rows.Count; i++)
        {
            bool used = i < skills.Count;
            rows[i].gameObject.SetActive(used);

            if (used)
                rows[i].Show(skills[i], interactive);
        }
    }
}
