using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Currency,
    Material,
    UsableObject,
    Mods,
    Equipment,
    Throwable

}


[CreateAssetMenu(fileName ="New Item Name",menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public ItemType ItemType;
    public string itemName;
    public Sprite icon;
    public int price;

    [TextArea] public string Description;

    //[Range(0,100)]
    //public float dropChance;

    public ItemEffect[] itemEffects;
    public PassiveEffectSO[] passiveEffects;


    public void ExecuteItemEffect(Transform _position)
    {
        foreach (var effect in itemEffects)
        {
            effect.ExecuteEffect(_position);
        }
    }


}
