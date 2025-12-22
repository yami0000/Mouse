using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Icon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Skill Info")]
    [SerializeField]private Description description;
    [SerializeField] private string skillName;
    [TextArea][SerializeField] private string skillDescription;

    [Header("UI")]
    [SerializeField] private Image skillImage;
    public Sprite normalSprite;
    public Sprite activeSprite;

    [Header("Skill Settings")]
    [SerializeField] private int cost = 1;
    [SerializeField] private bool ActiveSkill; 
    private Image dragImage;
    private Canvas canvas;

    [HideInInspector]public bool isActivated = false;
    private bool isHovered = false;

    [Header("Former Skill")]
    [SerializeField] private Icon[] RequiredSkill;
    [SerializeField] private RawImage line;
    [SerializeField] private Color lineColor;

    #region  Drag Skills
    public void OnBeginDrag(PointerEventData eventData)
    {
         if (isActivated && ActiveSkill) 
        dragImage = DragManager.Instance.CreateDragIcon(activeSprite);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragImage != null && isActivated && ActiveSkill)
            dragImage.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(dragImage?.gameObject);

        // Check if we dropped on a SkillSlot
        if (eventData.pointerEnter != null && isActivated && ActiveSkill)
        {

            SkillCoolDown.Instance.AssignSkill(this);


            SkillSlot slot = eventData.pointerEnter.GetComponent<SkillSlot>();
            if (slot != null)
            {
                slot.AssignSkill(this,skillName);
            }
        }
    }

    #endregion


    #region Activate Skills

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StartCoroutine(WaitTillShow());
        
    }

    IEnumerator WaitTillShow() 
    {
    yield return new WaitForSeconds(1f);
        if (isHovered)
    description.Show(skillName, skillDescription);

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        description.Hide();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        isHovered = false;
        if (isActivated)
         return;

        foreach (var req in RequiredSkill)
        {
            if (req == null || !req.isActivated)
            {
                Debug.Log($"{skillName} cannot be activated ¡ª prerequisite not met.");
                return;
            }
        }

        if (SK.Instance.Skill.HasEnoughPoints(cost))
            {
                ActivateSkill();
                SK.Instance.Skill.SpendPoints(cost);

                if (!ActiveSkill)
                    SK.Instance.Skill.ActivatePassiveSkill(skillName);
            }
            else
            {
                Debug.Log("Not enough skill points!");

            }
        
    }

    private void ActivateSkill()
    {

        isActivated = true;
        if (activeSprite != null)
            skillImage.sprite = activeSprite;
        if(line != null)
        line.color = lineColor;
    }

    #endregion
}
