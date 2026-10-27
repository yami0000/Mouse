using UnityEngine;


public enum ItemType
{
    Currency,
    Material,
    UsableObject,
    Mods,
    Equipment

}


// Created from Assets > Create > Data > Item (see Editor/DataAssetMenu.cs).
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public ItemType ItemType;
    public Sprite icon;
    public int price;

    [TextArea] public string Description;

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
