using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Name", menuName = "Data/Item Effect")]
public class ItemEffect : ScriptableObject
{
    public virtual void ExecuteEffect(Transform _position)
    {

        Debug.Log("Effect executed!");
    }
}
