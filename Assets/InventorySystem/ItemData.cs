using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Currency,
    Material,
    UsableObject,
    Other,
    Equipment

}


[CreateAssetMenu(fileName ="New Item Name",menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public ItemType ItemType;
    public string itemName;
    public Sprite icon;
    public int price;

    [Range(0,100)]
    public float dropChance;

    public ItemEffect[] itemEffects;

    public void ExecuteItemEffect(Transform _position)
    {
        foreach (var effect in itemEffects)
        {
            effect.ExecuteEffect(_position);
        }
    }

}
