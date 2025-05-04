using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
  Material,
  Equipment

}


[CreateAssetMenu(fileName ="New Item Name",menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public ItemType ItemType;
    public string itemName;
    public Sprite icon;

    [Range(0,100)]
    public float dropChance;
     
}
