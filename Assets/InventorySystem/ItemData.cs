using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Currency,
    Material,
    UsableObject,
    Mods,
    Equipment,
    Cyberware // saved as a number in every item asset, so new types go at the end

}

/// <summary>What kind of cyberware an item is. A Cyberware joint can be set to accept only one of these.</summary>
public enum CyberwareType
{
    Neural,
    Ocular,
    Arm,
    Leg,
    Spine,
    Organ
}


// Created from Assets > Create > Data > Item (see Editor/DataAssetMenu.cs).
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public ItemType ItemType;
    public Sprite icon;
    public int price;

    [Tooltip("{tokens} show this item's values, e.g. {Damage} or {EnhancedFirePowder.damageRate:%}. " +
             "Insert Value lists them all; hover it for the rest of the syntax.")]
    [LiveText] public string Description;

    public ItemEffect[] itemEffects;
    public PassiveEffectSO[] passiveEffects;

    // Only shown when Item Type is Cyberware (see ItemDataEditor).
    public CyberwareType cyberwareType;
    [Tooltip("What this cyberware gives by itself while it's installed and its joint is activated. Only its stats, mechanism and active skill are used; the inspector hides the rest.")]
    public SkillInfo cyberwareGives = new SkillInfo();
    [Tooltip("The small tree this cyberware opens. Each entry fills the Cyberware Skill joint with the same Slot number.")]
    public List<SkillInfo> cyberwareSkills = new List<SkillInfo>();


    public void ExecuteItemEffect(Transform _position)
    {
        foreach (var effect in itemEffects)
        {
            effect.ExecuteEffect(_position);
        }
    }

    /// <summary>
    /// This item's text for a tooltip text field (see TooltipFields.IsText), or null when it has none.
    /// Which fields show, and how, is set up in InvDescription's layouts.
    /// </summary>
    public virtual string GetTooltipText(TooltipField _field)
    {
        switch (_field)
        {
            case TooltipField.Name: return itemName;
            case TooltipField.Type: return ItemType == ItemType.Cyberware ? $"Cyberware  {cyberwareType}" : ItemType.ToString();
            case TooltipField.Description: return LiveText.Format(Description, this, InvDescription.ValueStyle);
            default: return null;
        }
    }

    /// <summary>This item's number for a tooltip number field. False when the item doesn't have that field.</summary>
    public virtual bool TryGetTooltipNumber(TooltipField _field, out float _value)
    {
        _value = 0f;

        if (_field == TooltipField.Price)
        {
            _value = price;
            return true;
        }

        if (ItemType != ItemType.Cyberware)
            return false;

        // SkillStat uses TooltipField's numbers, so a stat it gives fills that stat's tooltip line.
        bool found = false;

        foreach (SkillStatBonus bonus in cyberwareGives.stats)
        {
            if ((int)bonus.stat != (int)_field) continue;

            _value += bonus.value;
            found = true;
        }

        return found;
    }


}
