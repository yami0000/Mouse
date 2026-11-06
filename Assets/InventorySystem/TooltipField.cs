using System.Text.RegularExpressions;

/// <summary>
/// The five fixed line looks from before InvDescription's Line Styles became a named list.
/// Only kept so lines saved back then still find their style (by this name).
/// </summary>
public enum LineStyle
{
    Title,
    Subtitle,
    BigNumber,
    Stat,
    Body
}

/// <summary>
/// One piece of information a tooltip line can show. Items say what they have for each one
/// (ItemData.GetTooltipText / TryGetTooltipNumber); InvDescription's layouts pick which to show.
/// Saved as numbers in the Inspector, so add new ones with a new number instead of renumbering.
/// </summary>
public enum TooltipField
{
    // Always there
    CustomText = 0,
    Name = 1,
    Type = 2,
    Description = 3,
    Price = 4,
    StackSize = 5,

    // Equipment
    Level = 20,
    DPS = 21,
    Mods = 22,
    BulletSpeed = 23,
    ShotDelay = 24,
    BulletLifetime = 25,

    // Stats, on equipment and mods
    Damage = 43,
    CriticalChance = 44,
    CriticalDamage = 45,
    MaxHP = 46,
    Armor = 47,
    FireResistance = 48,
    FrostResistance = 49,
    LightningResistance = 50,
    PoisonResistance = 51,
    FireDamage = 52,
    FrostDamage = 53,
    PoisonDamage = 54,
    LightningDamage = 55,

    // Mods: how they change the weapon they're installed on
    BulletSpeedMultiplier = 70,
    ShotDelayMultiplier = 71,
    BulletLifetimeBonus = 72
}

public static class TooltipFields
{
    /// <summary>Stat fields (Fire Power to Lightning Damage): the ones installed mods add to.</summary>
    public static bool IsStat(TooltipField _field) => (int)_field >= 40 && (int)_field < 70;

    /// <summary>Fields whose value is text. Every other field except CustomText is a number.</summary>
    public static bool IsText(TooltipField _field) =>
        _field == TooltipField.Name || _field == TooltipField.Type ||
        _field == TooltipField.Description || _field == TooltipField.Mods;

    /// <summary>What a line says until it's changed in the Inspector. {v} is replaced by the value.</summary>
    public static string DefaultFormat(TooltipField _field)
    {
        switch (_field)
        {
            case TooltipField.CustomText: return "Text";
            case TooltipField.Name:
            case TooltipField.Type:
            case TooltipField.Description: return "{v}";
            case TooltipField.StackSize: return "x{v}";
            case TooltipField.Level: return "Lv.{v}";
            case TooltipField.DPS: return "<b>{v}</b> <size=50%>DPS</size>";
            case TooltipField.ShotDelay:
            case TooltipField.BulletLifetime: return Label(_field) + "  {v}s";
            case TooltipField.CriticalChance: return "Crit Chance  {v}%";
            case TooltipField.CriticalDamage: return "Crit Damage  {v}%";
            case TooltipField.BulletSpeedMultiplier: return "Bullet Speed  x{v}";
            case TooltipField.ShotDelayMultiplier: return "Shot Delay  x{v}";
            case TooltipField.BulletLifetimeBonus: return "Bullet Lifetime  {v}s";
            default: return Label(_field) + "  {v}";
        }
    }

    /// <summary>How a number is written: stats signed (+5), counts whole, times and speeds with up to 2 decimals.</summary>
    public static string FormatNumber(TooltipField _field, float _value)
    {
        switch (_field)
        {
            case TooltipField.Price:
            case TooltipField.StackSize:
            case TooltipField.Level:
            case TooltipField.DPS:
                return ((int)_value).ToString();

            case TooltipField.BulletSpeed:
            case TooltipField.ShotDelay:
            case TooltipField.BulletLifetime:
            case TooltipField.BulletSpeedMultiplier:
            case TooltipField.ShotDelayMultiplier:
                return _value.ToString("0.##");

            case TooltipField.BulletLifetimeBonus:
                return _value.ToString("+0.##;-0.##");

            default:
                return _value.ToString("+0;-0");
        }
    }

    // "FireResistance" -> "Fire Resistance", "MaxHP" -> "Max HP"
    private static string Label(TooltipField _field) =>
        Regex.Replace(_field.ToString(), "(?<=[a-z])(?=[A-Z])", " ");
}
