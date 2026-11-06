using System.Collections.Generic;
using UnityEngine;

// Kept out of "Upgrade Panel.cs": Unity ties a script file to the first class in it, and that
// file's name doesn't match UpgradePanel, so a class above it there stops the component loading.

/// <summary>One line of the upgrade panel: which value it shows and how.</summary>
[System.Serializable]
public class PanelRow
{
    // First, so the Inspector names each element after it.
    [Tooltip("What the line says. {v} is replaced by the value; TextMeshPro rich text works. The new value after a change uses it too.")]
    public string format = "{v}";
    public TooltipField field;
    [Tooltip("Shown left of the value, in an area with an Icon Size. Empty = a blank space, so the values still line up.")]
    public Sprite icon;
    public float fontSize = 28f;
    public Color color = Color.black;
    [Tooltip("Leave the line out while the value is 0.")]
    public bool hideWhenZero;

    public PanelRow() { }

    public PanelRow(TooltipField _field, float _fontSize, string _format = "{v}")
    {
        field = _field;
        fontSize = _fontSize;
        format = _format;
    }
}

/// <summary>A column of rows, starting at the top-left of Area and growing downwards.</summary>
[System.Serializable]
public class PanelArea
{
    [Tooltip("Where the rows go. Place it in the scene; rows stack downwards from its top-left corner.")]
    public RectTransform area;
    [Tooltip("Width of the icon column, and the icons' height. 0 = no icons.")]
    public float iconSize;
    [Tooltip("Gap between an icon and its value.")]
    public float iconGap = 16f;
    [Tooltip("Width of the value column. The new value after a change starts right after it, so it lines up on every row. " +
             "0 = right after each value.")]
    public float valueWidth;
    [Tooltip("Gap between rows.")]
    public float spacing = 8f;
    public List<PanelRow> rows = new List<PanelRow>();
}
