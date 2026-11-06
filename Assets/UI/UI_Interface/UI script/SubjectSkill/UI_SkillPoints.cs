using TMPro;
using UnityEngine;

/// <summary>Shows how many skill points are left.</summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_SkillPoints : MonoBehaviour
{
    [Tooltip("{v} is replaced by the points left.")]
    [SerializeField] private string format = "SP  {v}";

    private TextMeshProUGUI text;
    private int shown = -1;

    private void Awake() => text = GetComponent<TextMeshProUGUI>();

    private void Update()
    {
        if (Skill.Instance == null || Skill.Instance.SP == shown) return;

        shown = Skill.Instance.SP;
        text.text = format.Replace("{v}", shown.ToString());
    }
}
