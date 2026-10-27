using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_RocketPack : PassiveEffectSO
{
    public float Multiplier;
    private float M;
    public override bool EvaluateCondition(PassiveContext ctx)
    {
       return true;
    }

    public override void OnApply(GameObject owner)
    {
        M = PlayerManager.Instance.player.movespeed * Multiplier;

        PlayerManager.Instance.player.movespeed += M; 
        
    }

    public override void OnRemove(GameObject owner)
    {
        PlayerManager.Instance.player.movespeed -= M;
    }

    
}
