using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolDownBar : MonoBehaviour
{
    public Image Fill;
    public Image Mask;
    
     
    void Start()
    {
        
        Fill.enabled = false;
        Mask.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerManager.Instance.player.CanUseSkill())
        {
            Fill.enabled = true;
            Mask.enabled = true;

            Fill.fillAmount = 1 - (PlayerManager.Instance.player.SkillTimer / SK.Instance.Skill.CurrentSkillCoolDown);

        }
        else
        {
            Fill.enabled = false;
            Mask.enabled = false;
        }

    }
}
