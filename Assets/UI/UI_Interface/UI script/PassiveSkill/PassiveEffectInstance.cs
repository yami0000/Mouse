using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PassiveEffectInstance
{
    public PassiveEffectSO Definition { get; }
    public GameObject Owner { get; }
    public bool IsActive { get; private set; }

    public PassiveEffectInstance(PassiveEffectSO def, GameObject owner)
    {
        Definition = def;
        Owner = owner;
    }

    public bool Evaluate(PassiveContext ctx)
    {
        bool conditionMet = Definition.EvaluateCondition(ctx);

        if (conditionMet && !IsActive)
        {
            IsActive = true;
            Definition.OnApply(Owner);
            return true;
        }
        else if (!conditionMet && IsActive)
        {
            IsActive = false;
            Definition.OnRemove(Owner);
            return true;
        }
        return false;
    }

    // Force-remove when item is unequipped
    public void ForceRemove()
    {
        if (IsActive)
        {
            IsActive = false;
            Definition.OnRemove(Owner);
        }
    }

}
