using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A named look for tooltip lines: font, color, alignment and spacing. Lines pick one by name.</summary>
[System.Serializable]
public class TooltipStyle
{
    // First, so the Inspector names each element after it.
    [Tooltip("What lines use to pick this style. Renaming it leaves lines that used the old name on the first style.")]
    public string name = "New Style";

    [Header("Font")]
    public float fontSize = 8f;
    public bool bold;
    public bool italic;
    public Color color = Color.black;
    public HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Left;

    [Header("Spacing")]
    // TextMeshPro only counts positive margins in a line's height, so these can't go below 0.
    // To pull lines closer, use a negative Spacing on the lines' Vertical Layout Group.
    [Tooltip("Extra space above the line, added to the Vertical Layout Group's Spacing.")]
    [Min(0)] public float spaceAbove;
    [Tooltip("Extra space below the line, added to the Vertical Layout Group's Spacing.")]
    [Min(0)] public float spaceBelow;
    [Tooltip("Space added on the left, e.g. to indent mod names under \"Mods\".")]
    public float indent;
    [Tooltip("Gap between the rows of one line when its text wraps or has several rows. The text prefab uses -18.")]
    public float lineSpacing = -18f;
    [Tooltip("Gap between letters. The text prefab uses -7.")]
    public float characterSpacing = -7f;

    public TooltipStyle() { }

    public TooltipStyle(string _name, float _fontSize, bool _bold = false)
    {
        name = _name;
        fontSize = _fontSize;
        bold = _bold;
    }
}

/// <summary>Shows a string field as a dropdown of the style names in InvDescription's Line Styles.</summary>
public class TooltipStyleNameAttribute : PropertyAttribute { }

/// <summary>One line of a tooltip layout: which value it shows, what it says and how it looks.</summary>
[System.Serializable]
public class TooltipEntry
{
    // First, so the Inspector names each element after what the line says.
    [Tooltip("What the line says. {v} is replaced by the value; TextMeshPro rich text works. " +
             "Cleared, it goes back to the field's default. For Custom Text, the text itself.")]
    public string format;
    public TooltipField field;
    [Tooltip("Which of the Line Styles this line uses.")]
    [TooltipStyleName] public string styleName;
    [Tooltip("Leave the line out when the value is 0 (e.g. no Fire Damage). Empty text is always left out.")]
    public bool hideWhenZero = true;
    [Tooltip("Use Color instead of the style's color, e.g. red for Fire Damage.")]
    public bool customColor;
    public Color color = Color.white;

    // The field the format was last filled in for, so changing Field can swap in the new default.
    [SerializeField, HideInInspector] private TooltipField formatField;
    // Lines saved before styles had names; styleName takes over from it.
    [SerializeField, HideInInspector] private LineStyle style = LineStyle.Stat;

    /// <summary>The style to draw with. Lines from before named styles fall back to their old one.</summary>
    public string StyleName => string.IsNullOrEmpty(styleName) ? style.ToString() : styleName;

    public TooltipEntry() { }

    public TooltipEntry(TooltipField _field, string _styleName)
    {
        field = _field;
        styleName = _styleName;
        format = TooltipFields.DefaultFormat(_field);
        formatField = _field;
    }

    /// <summary>
    /// Fills in the default format when it's empty, or when Field changed and the format was still the old default.
    /// Also carries a line from before named styles over to its old style's name.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(format) || (field != formatField && format == TooltipFields.DefaultFormat(formatField)))
            format = TooltipFields.DefaultFormat(field);

        formatField = field;
        styleName = StyleName;
    }

    /// <summary>The line's text for _item, or null when it should be left out.</summary>
    public string BuildText(InventoryItem _item)
    {
        if (field == TooltipField.CustomText)
            return format;

        string value;

        if (TooltipFields.IsText(field))
        {
            value = _item.data.GetTooltipText(field);
            if (string.IsNullOrEmpty(value)) return null;
        }
        else
        {
            float number;

            if (field == TooltipField.StackSize)
                number = _item.stackSize;
            else if (!_item.data.TryGetTooltipNumber(field, out number))
                return null; // this kind of item doesn't have the field

            if (hideWhenZero && Mathf.Approximately(number, 0f)) return null;
            // A stack of one is just the item.
            if (field == TooltipField.StackSize && number <= 1) return null;

            value = TooltipFields.FormatNumber(field, number);
        }

        return format.Replace("{v}", value);
    }
}

/// <summary>The lines shown for one kind of item, top to bottom.</summary>
[System.Serializable]
public class TooltipLayout
{
    [Tooltip("Only a label for this list in the Inspector.")]
    public string name;
    public ItemType itemType;
    [Tooltip("Equipment only. Off = this layout is just for the Equipment Type below; " +
             "a layout for one type wins over an All Equipment Types one.")]
    public bool allEquipmentTypes = true;
    public EquipmentType equipmentType;
    public List<TooltipEntry> lines = new List<TooltipEntry>();
}

/// <summary>
/// The item tooltip. Which lines each kind of item shows is set in Layouts, how they look in
/// Line Styles; the items only supply the values (ItemData.GetTooltipText / TryGetTooltipNumber).
/// The box keeps its width and FitHeight sets its height from the lines; right-click the
/// component > Set Up Auto Size once to add the layout groups that stack them.
/// </summary>
public class InvDescription : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;

    public Transform contentContainer;
    public GameObject textPrefab;

    [Header("Line Styles")]
    [Tooltip("Looks the lines can use, picked by name. The first one is used when a line's style can't be found.")]
    [SerializeField] private List<TooltipStyle> styles = new List<TooltipStyle>();

    [Header("Layouts")]
    [Tooltip("What each kind of item shows. Right-click the component > Reset Tooltip Layouts to get the defaults back.")]
    [SerializeField] private List<TooltipLayout> layouts = DefaultLayouts();

    [Header("Description Values")]
    [Tooltip("How the {values} filled into item descriptions look (see LiveText). The shop's info panel uses it too.")]
    [SerializeField] private LiveTextStyle descriptionValues = LiveTextStyle.Default;

    [Header("Dialogue Terms")]
    [Tooltip("Style of the title when hovering an underlined word in dialogue (DialogueTerms).")]
    [SerializeField, TooltipStyleName] private string termTitleStyle = "Title";
    [Tooltip("Style of the description when hovering an underlined word in dialogue (DialogueTerms).")]
    [SerializeField, TooltipStyleName] private string termBodyStyle = "Body";

    /// <summary>How filled-in description values look: the scene tooltip's setting, or the default without one.</summary>
    public static LiveTextStyle ValueStyle => Main != null ? Main.descriptionValues : LiveTextStyle.Default;

    // The five fixed styles from before Line Styles was a list. Copied into it once, then unused.
    [SerializeField, HideInInspector] private TooltipStyle titleStyle = new TooltipStyle("Title", 16f, true);
    [SerializeField, HideInInspector] private TooltipStyle subtitleStyle = new TooltipStyle("Subtitle", 8f);
    [SerializeField, HideInInspector] private TooltipStyle bigNumberStyle = new TooltipStyle("BigNumber", 20f);
    [SerializeField, HideInInspector] private TooltipStyle statStyle = new TooltipStyle("Stat", 8f);
    [SerializeField, HideInInspector] private TooltipStyle bodyStyle = new TooltipStyle("Body", 8f);
    [SerializeField, HideInInspector] private bool stylesMigrated;

    // Used when Line Styles is empty.
    private static readonly TooltipStyle FallbackStyle = new TooltipStyle("Fallback", 8f);

    [Header("Position")]
    [Tooltip("Gap between the cursor and the box's nearest corner. The box flips to the cursor's other side " +
             "when it would go off the bottom or right of the screen.")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(10f, 10f);

    private bool isShowing = false;
    private Canvas canvas;

    private static InvDescription main;

    /// <summary>
    /// The scene's tooltip, for slots that weren't given one in the Inspector. Found once, even while hidden
    /// (it starts switched off), so the slots don't each need it dragged in.
    /// </summary>
    public static InvDescription Main
    {
        get
        {
            if (main == null)
                main = FindFirstObjectByType<InvDescription>(FindObjectsInactive.Include);
            return main;
        }
    }

    private void Awake()
    {
        MigrateStyles();

        // FitHeight sizes the box. A Content Size Fitter on it would undo that every layout pass,
        // and it also counts the 9-slice Image's sprite borders as a minimum height.
        ContentSizeFitter fitter = backgroundRect != null ? backgroundRect.GetComponent<ContentSizeFitter>() : null;
        if (fitter != null)
            fitter.enabled = false;
    }

    private void Start()
    {
        // Start() runs on the first activation, which can be the Show() call itself,
        // so only hide if nothing has asked to show the tooltip yet.
        if (!isShowing)
            Hide();
    }
    // LateUpdate, so a Show() from another script's Update this frame is already sized.
    private void LateUpdate()
    {
        FollowMouse();
    }

    /// <summary>
    /// Puts the box next to the cursor: below-right by default (pivot at its top-left).
    /// If it would go past the bottom edge it opens upwards instead (pivot at the bottom),
    /// past the right edge it opens leftwards (pivot at the right), or both (bottom-right).
    /// A box too big for either side is then pushed back inside the screen.
    /// </summary>
    private void FollowMouse()
    {
        RectTransform box = (RectTransform)transform;
        RectTransform parent = box.parent as RectTransform;
        if (parent == null) return;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        Camera cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, cam, out Vector2 mouse))
            return;

        // Everything below is in the parent's (the canvas's) units; the box's own scale counts.
        Vector2 size = Vector2.Scale(box.rect.size, box.localScale);
        Rect screen = parent.rect;

        bool openLeft = mouse.x + cursorOffset.x + size.x > screen.xMax;
        bool openUp = mouse.y - cursorOffset.y - size.y < screen.yMin;

        box.pivot = new Vector2(openLeft ? 1f : 0f, openUp ? 0f : 1f);

        Vector2 position = mouse + new Vector2(openLeft ? -cursorOffset.x : cursorOffset.x,
                                               openUp ? cursorOffset.y : -cursorOffset.y);

        // Still sticking out (the box is bigger than the room on either side): slide it back in.
        float left = position.x - box.pivot.x * size.x;
        if (left < screen.xMin) position.x += screen.xMin - left;
        else if (left + size.x > screen.xMax) position.x -= left + size.x - screen.xMax;

        float bottom = position.y - box.pivot.y * size.y;
        if (bottom < screen.yMin) position.y += screen.yMin - bottom;
        else if (bottom + size.y > screen.yMax) position.y -= bottom + size.y - screen.yMax;

        box.localPosition = position;
    }

    public void Show(InventoryItem item)
    {
        if (item?.data == null) return;

        BeginShow();

        foreach (TooltipEntry entry in FindLayout(item.data))
        {
            string text = entry.BuildText(item);
            if (!string.IsNullOrEmpty(text))
                CreateTextElement(text, entry);
        }

        FitHeight();
        FollowMouse(); // placed for its new size before it's drawn
    }

    /// <summary>A title and a description in the Term styles, for things that aren't items (e.g. DialogueTerms).</summary>
    public void Show(string title, string description)
    {
        BeginShow();

        if (!string.IsNullOrEmpty(title))
            CreateTextElement(title, new TooltipEntry(TooltipField.CustomText, termTitleStyle));
        if (!string.IsNullOrEmpty(description))
            CreateTextElement(description, new TooltipEntry(TooltipField.CustomText, termBodyStyle));

        FitHeight();
        FollowMouse(); // placed for its new size before it's drawn
    }

    /// <summary>Ready-made lines (Custom Text entries, Format holding the text), for things with their own layouts (e.g. skills).</summary>
    public void ShowLines(IEnumerable<TooltipEntry> _lines)
    {
        BeginShow();

        foreach (TooltipEntry line in _lines)
            CreateTextElement(line.format, line);

        FitHeight();
        FollowMouse(); // placed for its new size before it's drawn
    }

    private void BeginShow()
    {
        isShowing = true;
        gameObject.SetActive(true);
        // Drawn after every other menu in the canvas (inventory, upgrade screen), so it's never behind one.
        transform.SetAsLastSibling();

        ClearLines();
    }

    /// <summary>
    /// Keeps the box's width and sets its height to the lines' height plus padding.
    /// Asked of the lines' layout group alone: the box's own preferred height would also count the
    /// 9-slice Image, which reports its sprite borders (~88 units here) and kept every short tooltip that tall.
    /// </summary>
    private void FitHeight()
    {
        // First pass gives the lines the box's width, so wrapped text reports its real height.
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);

        LayoutGroup lines = contentContainer.GetComponent<LayoutGroup>();
        float height = lines != null ? lines.preferredHeight : LayoutUtility.GetPreferredHeight((RectTransform)contentContainer);

        // The lines' own padding is already in their preferred height; the box's padding goes around them.
        if (contentContainer != backgroundRect)
        {
            LayoutGroup box = backgroundRect.GetComponent<LayoutGroup>();
            if (box != null)
                height += box.padding.vertical;
        }

        backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        // Second pass lays the lines out in the new height, before this frame is drawn.
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
    }

    /// <summary>The lines for _data: a layout for its exact equipment type first, then one for its item type.</summary>
    private List<TooltipEntry> FindLayout(ItemData _data)
    {
        ItemData_Equipment equip = _data as ItemData_Equipment;
        TooltipLayout match = null;

        foreach (TooltipLayout layout in layouts)
        {
            if (layout.itemType != _data.ItemType) continue;

            if (equip != null && !layout.allEquipmentTypes)
            {
                if (layout.equipmentType == equip.equipmentType)
                    return layout.lines;
                continue;
            }

            if (match == null)
                match = layout;
        }

        return match != null ? match.lines : FallbackLines;
    }

    // Shown for an item type that has no layout yet.
    private static readonly List<TooltipEntry> FallbackLines = new List<TooltipEntry>
    {
        new TooltipEntry(TooltipField.Name, "Title"),
        new TooltipEntry(TooltipField.Type, "Subtitle"),
        new TooltipEntry(TooltipField.Description, "Body")
    };

    private void ClearLines()
    {
        foreach (Transform child in contentContainer)
        {
            // Destroy() waits until the end of the frame; deactivated, the old lines
            // already stop counting towards the size in the rebuild above.
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void CreateTextElement(string text, TooltipEntry entry)
    {
        GameObject newText = Instantiate(textPrefab, contentContainer);
        TextMeshProUGUI tmp = newText.GetComponent<TextMeshProUGUI>();

        // A fixed preferred height would stop the line growing when its text wraps;
        // -1 lets the text report its own height.
        LayoutElement layout = newText.GetComponent<LayoutElement>();
        if (layout != null)
            layout.preferredHeight = -1;

        TooltipStyle style = GetStyle(entry.StyleName);

        tmp.text = text;
        tmp.fontSize = style.fontSize;
        tmp.fontStyle = (style.bold ? FontStyles.Bold : FontStyles.Normal) | (style.italic ? FontStyles.Italic : FontStyles.Normal);
        tmp.color = entry.customColor ? entry.color : style.color;
        tmp.horizontalAlignment = style.alignment;
        // The layout sizes each line to fit its text, so the text has to start at the top of it.
        // The prefab's Baseline puts the first row's baseline in the middle, which pushes text
        // with several rows (Mods with its mod names, a wrapped description) halfway out of the bottom.
        tmp.verticalAlignment = VerticalAlignmentOptions.Top;
        tmp.lineSpacing = style.lineSpacing;
        tmp.characterSpacing = style.characterSpacing;
        // Margins count towards the text's preferred height, so the layout makes room for them.
        tmp.margin = new Vector4(style.indent, style.spaceAbove, 0f, style.spaceBelow);
    }

    /// <summary>The style called _name, else the first one, so a renamed or deleted style never breaks a line.</summary>
    private TooltipStyle GetStyle(string _name)
    {
        foreach (TooltipStyle style in styles)
            if (style != null && style.name == _name)
                return style;

        return styles.Count > 0 && styles[0] != null ? styles[0] : FallbackStyle;
    }

    /// <summary>Copies the five fixed styles from before Line Styles was a list into it, once.</summary>
    private void MigrateStyles()
    {
        if (stylesMigrated) return;

        if (styles == null)
            styles = new List<TooltipStyle>();

        if (styles.Count == 0)
        {
            (TooltipStyle style, string name)[] old =
            {
                (titleStyle, "Title"), (subtitleStyle, "Subtitle"), (bigNumberStyle, "BigNumber"),
                (statStyle, "Stat"), (bodyStyle, "Body")
            };

            foreach (var (style, name) in old)
            {
                TooltipStyle copy = style ?? new TooltipStyle();
                copy.name = name;
                styles.Add(copy);
            }
        }

        stylesMigrated = true;
    }

    public void Hide()
    {
        if (gameObject == null) return;
        isShowing = false;
        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        MigrateStyles();

        if (layouts == null) return;

        foreach (TooltipLayout layout in layouts)
            if (layout?.lines != null)
                foreach (TooltipEntry entry in layout.lines)
                    entry?.Validate();
    }

    /// <summary>The layouts a new tooltip starts with: what each item type showed before layouts existed.</summary>
    private static List<TooltipLayout> DefaultLayouts()
    {
        TooltipField[] stats =
        {
            TooltipField.Damage, TooltipField.CriticalChance, TooltipField.CriticalDamage,
            TooltipField.MaxHP, TooltipField.Armor,
            TooltipField.FireResistance, TooltipField.FrostResistance, TooltipField.LightningResistance, TooltipField.PoisonResistance,
            TooltipField.FireDamage, TooltipField.FrostDamage, TooltipField.PoisonDamage, TooltipField.LightningDamage
        };

        TooltipLayout Layout(string _name, ItemType _type, params TooltipEntry[] _lines) =>
            new TooltipLayout { name = _name, itemType = _type, lines = new List<TooltipEntry>(_lines) };

        TooltipEntry Line(TooltipField _field, string _style) => new TooltipEntry(_field, _style);

        var equipment = Layout("Equipment", ItemType.Equipment,
            Line(TooltipField.Name, "Title"),
            Line(TooltipField.Type, "Subtitle"),
            Line(TooltipField.Level, "Subtitle"),
            Line(TooltipField.DPS, "BigNumber"));
        foreach (TooltipField stat in stats)
            equipment.lines.Add(Line(stat, "Stat"));
        equipment.lines.Add(Line(TooltipField.Mods, "Subtitle"));
        equipment.lines.Add(Line(TooltipField.Description, "Body"));

        var mods = Layout("Mods", ItemType.Mods,
            Line(TooltipField.Name, "Title"),
            Line(TooltipField.Type, "Subtitle"));
        foreach (TooltipField stat in stats)
            mods.lines.Add(Line(stat, "Stat"));
        mods.lines.Add(Line(TooltipField.BulletSpeedMultiplier, "Stat"));
        mods.lines.Add(Line(TooltipField.ShotDelayMultiplier, "Stat"));
        mods.lines.Add(Line(TooltipField.BulletLifetimeBonus, "Stat"));
        mods.lines.Add(Line(TooltipField.Description, "Body"));

        return new List<TooltipLayout>
        {
            equipment,
            mods,
            Layout("Usable Object", ItemType.UsableObject,
                Line(TooltipField.Name, "Title"),
                Line(TooltipField.Type, "Subtitle"),
                Line(TooltipField.Description, "Body")),
            Layout("Material", ItemType.Material,
                Line(TooltipField.Name, "Title"),
                Line(TooltipField.Type, "Subtitle"),
                Line(TooltipField.Description, "Body")),
            Layout("Currency", ItemType.Currency,
                Line(TooltipField.Name, "Title"),
                Line(TooltipField.Description, "Body"))
        };
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Tooltip Layouts")]
    private void ResetLayouts()
    {
        UnityEditor.Undo.RecordObject(this, "Reset Tooltip Layouts");
        layouts = DefaultLayouts();
    }

    /// <summary>
    /// Adds the components that make the background fit its lines: the background keeps its width
    /// and grows downwards (FitHeight, pivot at the top), the container stacks the lines
    /// at full width so long text wraps. Undoable; tweak padding and spacing afterwards in the Inspector.
    /// </summary>
    [ContextMenu("Set Up Auto Size")]
    private void SetUpAutoSize()
    {
        RectTransform background = backgroundRect != null ? backgroundRect : (RectTransform)transform;

        UnityEditor.Undo.RecordObject(background, "Set Up Auto Size");
        background.pivot = new Vector2(0f, 1f);

        VerticalLayoutGroup backgroundLayout = GetOrAdd<VerticalLayoutGroup>(background.gameObject);
        backgroundLayout.padding = new RectOffset(8, 8, 8, 8);
        backgroundLayout.childControlWidth = true;
        backgroundLayout.childControlHeight = true;
        backgroundLayout.childForceExpandWidth = true;
        backgroundLayout.childForceExpandHeight = false;
        backgroundLayout.childScaleWidth = false;
        backgroundLayout.childScaleHeight = false;

        // FitHeight sizes the box from the lines; a Content Size Fitter would fight it (see Awake).
        ContentSizeFitter fitter = background.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            UnityEditor.Undo.DestroyObjectImmediate(fitter);

        if (contentContainer != null && contentContainer != background)
        {
            // No Content Size Fitter here either: the background's layout group already sizes the container.
            VerticalLayoutGroup contentLayout = GetOrAdd<VerticalLayoutGroup>(contentContainer.gameObject);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childScaleWidth = false;
            contentLayout.childScaleHeight = false;

            ContentSizeFitter extraFitter = contentContainer.GetComponent<ContentSizeFitter>();
            if (extraFitter != null)
                UnityEditor.Undo.DestroyObjectImmediate(extraFitter);
        }

        Debug.Log("Tooltip auto size set up. Adjust padding and spacing on the Vertical Layout Groups if needed.", this);
    }

    private static T GetOrAdd<T>(GameObject _go) where T : Component
    {
        T component = _go.GetComponent<T>();
        if (component == null)
            return UnityEditor.Undo.AddComponent<T>(_go);

        UnityEditor.Undo.RecordObject(component, "Set Up Auto Size");
        return component;
    }
#endif
}
