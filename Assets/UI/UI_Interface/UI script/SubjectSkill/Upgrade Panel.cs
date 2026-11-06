using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour//UIÊÓ¾õ
{
    public static UpgradePanel Instance;

    [Tooltip("Still placed by hand.")]
    [SerializeField] private TextMeshProUGUI Level;
    [HideInInspector] public ItemData_Equipment Equipment;
    public Image Image;

    [Header("Text")]
    [Tooltip("Font of every row. Empty = the font of Level.")]
    [SerializeField] private TMP_FontAsset font;

    [Header("Rows")]
    [Tooltip("A: the name and DPS.")]
    [SerializeField] private PanelArea header = new PanelArea
    {
        rows = { new PanelRow(TooltipField.Name, 40f), new PanelRow(TooltipField.DPS, 72f) }
    };
    [Tooltip("B: the stats, each with an icon in front.")]
    [SerializeField] private PanelArea stats = new PanelArea
    {
        iconSize = 40f,
        valueWidth = 90f,
        rows =
        {
            new PanelRow(TooltipField.CriticalChance, 28f, "{v}%"),
            new PanelRow(TooltipField.CriticalDamage, 28f, "{v}%"),
            new PanelRow(TooltipField.FireDamage, 28f),
            new PanelRow(TooltipField.FrostDamage, 28f),
            new PanelRow(TooltipField.PoisonDamage, 28f),
            new PanelRow(TooltipField.LightningDamage, 28f)
        }
    };

    [Header("Change Preview")]
    [Tooltip("Between the old value and the new one, e.g. 10 -> 16 (an arrow by default). Use > if the font has no arrow.")]
    [SerializeField] private string arrow = ((char)0x2192).ToString(); // the arrow, written so the file stays plain ASCII here
    [Tooltip("Gap between the old value and the arrow.")]
    [SerializeField] private float changeGap = 16f;
    [SerializeField] private Color betterColor = new Color(0.2f, 0.65f, 0.25f);
    [SerializeField] private Color worseColor = new Color(0.85f, 0.2f, 0.2f);

    /// <summary>The texts made for one row.</summary>
    private class RowView
    {
        public GameObject root;
        public TextMeshProUGUI value;
        public TextMeshProUGUI change;
    }

    private readonly List<RowView> headerViews = new List<RowView>();
    private readonly List<RowView> statViews = new List<RowView>();
    private bool rowsDirty = true;

    // The values just before the latest install or uninstall: the left side of "10 ¡ú 16".
    // Empty right after opening, so nothing shows a change yet.
    private Dictionary<TooltipField, float> before = new Dictionary<TooltipField, float>();
    // The values as of the latest change (or opening): what "before" becomes at the next change.
    private Dictionary<TooltipField, float> latest = new Dictionary<TooltipField, float>();

    private TMP_FontAsset Font => font != null ? font : Level != null ? Level.font : TMP_Settings.defaultFontAsset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Inventory.Start may already have opened something.
        if (Equipment == null)
            Initialize();
    }

    private void OnValidate()
    {
        // Rebuild the rows on the next Refresh, so Inspector changes show while playing.
        rowsDirty = true;
    }

    /// <summary>Opens the first equipment that can take mods, or shows nothing when there is none.</summary>
    public void Initialize()
    {
        List<InventoryItem> list = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.UpgradableEquipment() : null;

        if (list != null && list.Count > 0)
            Select((ItemData_Equipment)list[0].data);
        else
            Select(null);
    }

    /// <summary>Opens _equipment: its picture, numbers and mod slots, with no change shown yet.</summary>
    public void Select(ItemData_Equipment _equipment)
    {
        Equipment = _equipment;

        upgradeVisual(_equipment);
        UpgradeSystem.Instance.UpdateModSlot(_equipment);

        before.Clear();
        latest = Snapshot();

        Refresh();
    }

    /// <summary>
    /// Call after a mod was installed or uninstalled: shows "old ¡ú new" for every number that
    /// this change moved, the old one being the value just before it.
    /// </summary>
    public void ModsChanged()
    {
        before = latest;
        latest = Snapshot();

        Refresh();
    }

    /// <summary>Equipment's current number for every row that shows one.</summary>
    private Dictionary<TooltipField, float> Snapshot()
    {
        var values = new Dictionary<TooltipField, float>();

        if (Equipment != null)
            foreach (PanelRow row in AllRows())
                if (Equipment.TryGetTooltipNumber(row.field, out float value))
                    values[row.field] = value;

        return values;
    }

    public void upgradeVisual(ItemData_Equipment E)
    {
        if (Image == null) return;

        // Keep the picture's own proportions inside the box.
        Image.preserveAspect = true;
        Image.sprite = E != null ? E.icon : null;
        Image.color = E != null ? Color.white : Color.clear;
    }

    /// <summary>Shows Equipment's numbers, with "¡ú new" next to the ones the latest mod change moved (see ModsChanged).</summary>
    public void Refresh()
    {
        if (rowsDirty)
            BuildRows();

        if (Level != null)
            Level.text = Equipment != null ? Equipment.Level.ToString() : "";

        ShowRows(header, headerViews);
        ShowRows(stats, statViews);
    }

    private IEnumerable<PanelRow> AllRows()
    {
        foreach (PanelRow row in header.rows) yield return row;
        foreach (PanelRow row in stats.rows) yield return row;
    }

    private void ShowRows(PanelArea _area, List<RowView> _views)
    {
        for (int i = 0; i < _views.Count && i < _area.rows.Count; i++)
            ShowRow(_area.rows[i], _views[i]);
    }

    private void ShowRow(PanelRow _row, RowView _view)
    {
        _view.change.text = "";

        if (Equipment == null)
        {
            _view.root.SetActive(false);
            return;
        }

        if (_row.field == TooltipField.CustomText || TooltipFields.IsText(_row.field))
        {
            string text = _row.field == TooltipField.CustomText ? _row.format : Equipment.GetTooltipText(_row.field);
            _view.root.SetActive(!string.IsNullOrEmpty(text));
            _view.value.text = _row.field == TooltipField.CustomText ? text : _row.format.Replace("{v}", text);
            return;
        }

        if (!Equipment.TryGetTooltipNumber(_row.field, out float now))
        {
            _view.root.SetActive(false);
            return;
        }

        float was = before.TryGetValue(_row.field, out float old) ? old : now;
        bool changed = !Mathf.Approximately(was, now);

        _view.root.SetActive(!_row.hideWhenZero || changed || !Mathf.Approximately(now, 0f));
        _view.value.text = _row.format.Replace("{v}", Format(_row.field, was));

        if (!changed) return;

        _view.change.text = $"{arrow} {_row.format.Replace("{v}", Format(_row.field, now))}";
        _view.change.color = IsBetter(_row.field, was, now) ? betterColor : worseColor;
    }

    private static string Format(TooltipField _field, float _value) =>
        _field == TooltipField.DPS ? ((int)_value).ToString() : _value.ToString("0.##");

    // Less delay between shots is better; for everything else, more is.
    private static bool IsBetter(TooltipField _field, float _was, float _now) =>
        _field == TooltipField.ShotDelay ? _now < _was : _now > _was;

    #region Building the rows

    private void BuildRows()
    {
        rowsDirty = false;
        BuildArea(header, headerViews);
        BuildArea(stats, statViews);
    }

    private void BuildArea(PanelArea _area, List<RowView> _views)
    {
        foreach (RowView view in _views)
            if (view.root != null)
                Destroy(view.root);
        _views.Clear();

        if (_area.area == null) return;

        VerticalLayoutGroup layout = _area.area.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = _area.area.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = _area.spacing;

        foreach (PanelRow row in _area.rows)
            _views.Add(CreateRow(_area, row));
    }

    /// <summary>[icon][gap] value [gap] change, left to right, centred on one line.</summary>
    private RowView CreateRow(PanelArea _area, PanelRow _row)
    {
        RectTransform root = NewChild(_area.area, _row.field.ToString());

        HorizontalLayoutGroup line = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        line.childAlignment = TextAnchor.MiddleLeft;
        line.childControlWidth = true;
        line.childControlHeight = true;
        line.childForceExpandWidth = false;
        line.childForceExpandHeight = false;

        if (_area.iconSize > 0f)
        {
            RectTransform iconRect = NewChild(root, "Icon");
            Fix(iconRect, _area.iconSize, _area.iconSize);

            Image icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = _row.icon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            // No sprite: an empty space the same size, so this row's value lines up with the others.
            icon.enabled = _row.icon != null;

            Fix(NewChild(root, "Gap"), _area.iconGap, 0f);
        }

        TextMeshProUGUI value = NewText(root, "Value", _row);
        if (_area.valueWidth > 0f)
            Fix(value.rectTransform, _area.valueWidth, -1f);

        Fix(NewChild(root, "Gap"), changeGap, 0f);

        return new RowView { root = root.gameObject, value = value, change = NewText(root, "Change", _row) };
    }

    private TextMeshProUGUI NewText(RectTransform _parent, string _name, PanelRow _row)
    {
        TextMeshProUGUI text = NewChild(_parent, _name).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Font;
        text.fontSize = _row.fontSize;
        text.color = _row.color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform NewChild(Transform _parent, string _name)
    {
        GameObject go = new GameObject(_name, typeof(RectTransform));
        go.layer = _parent.gameObject.layer;
        go.transform.SetParent(_parent, false);
        return (RectTransform)go.transform;
    }

    /// <summary>Gives _rect a fixed layout width (and height, when not -1).</summary>
    private static void Fix(RectTransform _rect, float _width, float _height)
    {
        LayoutElement element = _rect.gameObject.AddComponent<LayoutElement>();
        element.minWidth = element.preferredWidth = _width;

        if (_height >= 0f)
            element.minHeight = element.preferredHeight = _height;
    }

    #endregion
}
