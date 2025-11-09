using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image slotImage;
    [SerializeField] private Sprite emptySprite;

    private Icon assignedSkill;

    public void AssignSkill(Icon skill,string skillname)
    {
        assignedSkill = skill;
        slotImage.sprite = skill.activeSprite;
        SK.Instance.Skill.EquippedSkill = skillname;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
         
        if (eventData.button == PointerEventData.InputButton.Right && assignedSkill != null)
        {

            UnassignSkill();
        }
    }

    private void UnassignSkill()
    {
        assignedSkill = null;
        slotImage.sprite = emptySprite;
        SK.Instance.Skill.EquippedSkill = null;
    }

    public bool IsEmpty()
    {
        return assignedSkill == null;
    }

 
}