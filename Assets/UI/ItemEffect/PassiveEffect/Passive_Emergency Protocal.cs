using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Passives/EmergencyProtocal")]
public class Passive_EmergencyProtocal : PassiveEffectSO
{
    public float Multiplier;
    private int bonus;
    public override bool EvaluateCondition(PassiveContext ctx) 
    {
     return ctx.currentHP/ctx.maxHP < threshold;
    }

    public override void OnApply(GameObject owner)
    {
        bonus = (int)(owner.GetComponent<PlayerStats>().Armor.GetValue()*Multiplier);
        owner.GetComponent<PlayerStats>().Armor.AddModifier(bonus);
        Debug.Log("EmergencyProtocal activated!");
    }

    public override void OnRemove(GameObject owner)
    {
        owner.GetComponent<PlayerStats>().Armor.RemoveModifier(bonus);
        Debug.Log("EmergencyProtocal deactivated.");
    }
}
