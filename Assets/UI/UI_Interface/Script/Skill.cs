using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Skill : MonoBehaviour
{
    public static Skill Instance;

    [HideInInspector] public string EquippedSkill = null; 

    [SerializeField] private int totalSkillPoints = 5;

    void Awake()
    {
        Instance = this;
    }

    public bool HasEnoughPoints(int cost)
    {
        return totalSkillPoints >= cost;
    }

    public void SpendPoints(int cost)
    {
        totalSkillPoints -= cost;
        Debug.Log($"Spent {cost} points. Remaining: {totalSkillPoints}");
    }
    public void ActivatePassiveSkill(string skillName)
    {
        if(skillName == "Metabolism") 
        {
        
        }


    }
 
}
