using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillCoolDown : MonoBehaviour
{
    public static SkillCoolDown Instance;

   private Image slotImage;
    private GameObject GameObject;

    private Sprite inActive;
    private Sprite Active;
   // [SerializeField] private Sprite emptySprite;

    private Icon assignedSkill;

    void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (PlayerManager.Instance.player.CanUseSkill())
            slotImage.sprite = Active;
        else
            slotImage.sprite = inActive;
    }
    private void Start()
    {
        slotImage= GetComponent<Image>();
        GameObject = GetComponent<GameObject>();
        gameObject.SetActive(false);
    }
    public void AssignSkill(Icon skill)
    {
        gameObject.SetActive(true);
        assignedSkill = skill;
        slotImage.sprite = skill.activeSprite;
        
        inActive = skill.normalSprite;
        Active = skill.activeSprite;
    }

     

    public void UnassignSkill()
    {
        assignedSkill = null;
        gameObject.SetActive(false);

    }

   

}
