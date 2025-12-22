using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Skill : MonoBehaviour
{
    public static Skill Instance;

    [HideInInspector] public string EquippedSkill = null; 

     public int SP = 5;

    [Header("Skill")]
    [HideInInspector] public bool Metabolism = false;
    [HideInInspector] public bool OverCharging = false;
    [HideInInspector] public float CurrentSkillCoolDown;
    [HideInInspector] public bool DoubleJump = false;

    [Header("CoolDown")]
    public float ProtectiveShield;
    public float Reaper;

    void Awake()
    {
        Instance = this;
    }

    public bool HasEnoughPoints(int cost)
    {
        return SP >= cost;
    }

    public void SpendPoints(int cost)
    {
        SP -= cost;
        Debug.Log($"Spent {cost} points. Remaining: {SP}");
    }
    public void ActivatePassiveSkill(string skillName)
    {
        if (skillName == "Metabolism")
            Metabolism = true;
        if(skillName == "OverCharging")
            OverCharging = true;
        if (skillName == "FirePower Training")
            PlayerManager.Instance.player.stats.FirePower.AddModifier(5);
        if (skillName == "Defense Training")
            PlayerManager.Instance.player.stats.Armor.AddModifier(10);
        if(skillName == "Double Jump")
            DoubleJump = true;


    }
 
}
