using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A joint of the old skill tree, data only. Tools > Skill Tree > Set Up Scene reads these into
/// SkillJoints and removes them. Delete this file after that; nothing else uses it.
/// </summary>
public class Icon : MonoBehaviour
{
    [SerializeField] private string skillName;
    [TextArea][SerializeField] private string skillDescription;
    public Sprite normalSprite;
    public Sprite activeSprite;
    [SerializeField] private int cost = 1;
    [SerializeField] private bool ActiveSkill;
    [SerializeField] private Icon[] RequiredSkill;
    [SerializeField] private RawImage line;
}
