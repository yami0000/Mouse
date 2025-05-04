using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Stats 
{
    [SerializeField]private int BaseValue;

    public List<int> modifiers;    

    public int GetValue() 
    {
        int finalValue = BaseValue;

        foreach (int modifier in modifiers)
        {
            finalValue += modifier;
        }


        return finalValue;
    }

    public void SetDefaultValue(int _value)
    {
        BaseValue = _value;

    }
    public void AddModifier(int _modifier) 
    {
        modifiers.Add(_modifier);
    }

    public void RemoveModifier(int _modifier) 
    {
     modifiers.Remove(_modifier); 
    }
}
