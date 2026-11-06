using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One row of the fast travel list, set in the Inspector.
/// </summary>
[Serializable]
public class FastTravelDestination
{
    [Tooltip("The text shown on the row.")]
    public string displayName = "New Destination";
    [Tooltip("Spawn point ID in AA-BB format. AA is the scene index (\"02-01\" = scene 2, spawn point 02-01).")]
    public string spawnID = "01-01";

    [Header("Text")]
    [Tooltip("Empty = the list's Default Font.")]
    public TMP_FontAsset font;
    [Tooltip("0 = the list's Default Font Size.")]
    public float fontSize = 0f;
    public Color textColor = Color.white;
    [Tooltip("Text color while the row is highlighted (the background turns white, so white text would vanish).")]
    public Color highlightedTextColor = Color.black;
}

/// <summary>
/// Builds the fast travel rows from a list, under a VerticalLayoutGroup (put this on that object).
/// Mouse hover highlights a row; the Up/Down arrows (or PgUp/PgDown) move the highlight and wrap around at either end;
/// Enter or a left click on the highlighted row travels there. Works like Yarn's option list,
/// but picks a spawn point instead of a dialogue option.
/// </summary>
public class UI_FastTravelList : MonoBehaviour
{
    [Tooltip("The menu to close after travelling. Empty = the UI_NPC (FastTravelAni) above this object.")]
    [SerializeField] private UI_NPC menu;

    [SerializeField] private List<FastTravelDestination> destinations = new List<FastTravelDestination>();

    [Header("Row")]
    [SerializeField] private Vector2 rowSize = new Vector2(818f, 198f);
    [Tooltip("Optional background sprite, e.g. a 9-slice frame. Empty = a plain rectangle.")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color normalBackground = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color highlightedBackground = Color.white;

    [Header("Text Defaults")]
    [Tooltip("Used when a destination has no font. Empty = TextMeshPro's default font.")]
    [SerializeField] private TMP_FontAsset defaultFont;
    [SerializeField] private float defaultFontSize = 72f;
    [SerializeField] private TextAlignmentOptions alignment = TextAlignmentOptions.Left;
    [Tooltip("Space between the row's edges and its text (left, right).")]
    [SerializeField] private Vector2 textPadding = new Vector2(40f, 40f);

    private readonly List<UI_FastTravelEntry> entries = new List<UI_FastTravelEntry>();
    private int highlighted = -1;
    private bool travelling;

    private void Awake()
    {
        if (menu == null)
            menu = GetComponentInParent<UI_NPC>(true);

        Build();
    }

    // The menu shows and hides this object with its parent, so each opening starts fresh.
    private void OnEnable()
    {
        travelling = false;
        SetHighlight(-1);
    }

    private void Update()
    {
        if (entries.Count == 0 || travelling) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.PageDown))
            SetHighlight(highlighted < 0 ? 0 : (highlighted + 1) % entries.Count);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.PageUp))
            SetHighlight(highlighted < 0 ? 0 : (highlighted - 1 + entries.Count) % entries.Count);

        if (highlighted >= 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            Travel(highlighted);
    }

    /// <summary>Rebuilds the rows from the list. Call it after changing the list at runtime.</summary>
    public void Build()
    {
        // Rows from an earlier build, plus anything left over in the hierarchy.
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        entries.Clear();
        highlighted = -1;

        for (int i = 0; i < destinations.Count; i++)
            entries.Add(CreateRow(destinations[i], i));
    }

    private UI_FastTravelEntry CreateRow(FastTravelDestination destination, int index)
    {
        GameObject row = new GameObject(destination.displayName, typeof(RectTransform));
        row.layer = gameObject.layer;

        RectTransform rect = (RectTransform)row.transform;
        rect.SetParent(transform, false);
        rect.sizeDelta = rowSize;

        // Background: the rectangle that turns white. It is also what the mouse hovers.
        Image background = row.AddComponent<Image>();
        background.sprite = backgroundSprite;
        background.type = backgroundSprite != null && backgroundSprite.border != Vector4.zero
            ? Image.Type.Sliced
            : Image.Type.Simple;
        background.raycastTarget = true; // still catches the mouse when Normal Background is transparent

        // Text, stretched over the row minus the padding.
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.layer = gameObject.layer;
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(textPadding.x, 0f);
        textRect.offsetMax = new Vector2(-textPadding.y, 0f);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = destination.displayName;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        TMP_FontAsset font = destination.font != null ? destination.font : defaultFont;
        if (font != null) label.font = font;
        label.fontSize = destination.fontSize > 0f ? destination.fontSize : defaultFontSize;

        UI_FastTravelEntry entry = row.AddComponent<UI_FastTravelEntry>();
        entry.Setup(this, index, background, label,
                    normalBackground, highlightedBackground,
                    destination.textColor, destination.highlightedTextColor);
        return entry;
    }

    #region Called by the rows

    public void OnRowEntered(int index) => SetHighlight(index);

    public void OnRowExited(int index)
    {
        // Leaving a row only clears it if it is still the highlighted one;
        // a row picked with the arrow keys stays lit while the mouse moves elsewhere.
        if (index == highlighted)
            SetHighlight(-1);
    }

    public void OnRowClicked(int index)
    {
        if (travelling) return;
        SetHighlight(index);
        Travel(index);
    }

    #endregion

    private void SetHighlight(int index)
    {
        if (highlighted >= 0 && highlighted < entries.Count)
            entries[highlighted].SetHighlighted(false);

        highlighted = index;

        if (highlighted >= 0 && highlighted < entries.Count)
            entries[highlighted].SetHighlighted(true);
    }

    private void Travel(int index)
    {
        if (travelling || index < 0 || index >= destinations.Count) return;

        string spawnID = destinations[index].spawnID;
        if (!GameManager.TryGetSceneIndex(spawnID, out _))
        {
            Debug.LogError($"[UI_FastTravelList] '{destinations[index].displayName}' has spawn ID '{spawnID}', " +
                           "which isn't in the AA-BB format.", this);
            return;
        }

        travelling = true;
        GameManager.Instance.LoadScene(spawnID);

        if (menu != null)
            menu.CloseMenu();
    }
}
