using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PassiveEffectSO : ScriptableObject
{
    [Header("Condition")]
    public PassiveTrigger trigger;
    [Range(0f, 1f)] public float threshold; 
 
    public abstract void OnApply(GameObject owner);
    public abstract void OnRemove(GameObject owner);
    public abstract bool EvaluateCondition(PassiveContext ctx);
}
public enum PassiveTrigger
{
    OnHealthBelow,
    OnAlways,   
}
public struct PassiveContext
{
    public float currentHP;
   public float maxHP;  
   
}
