using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

/// <summary>
/// Hoverable terminology in dialogue. In a Yarn line, [term=mod]MOD[/term] is drawn colored and
/// underlined, and resting the mouse on it opens the inventory tooltip (InvDescription) with the
/// DialogueTerm whose ID is "mod". [term]MOD[/term] without an ID uses the word itself as the ID.
/// Put it on the Line Presenter, drag in its Line Text and the DialogueGlossary.
/// </summary>
public class DialogueTerms : ReplacementMarkupHandler
{
    [Tooltip("Empty = the Dialogue Runner's line provider.")]
    [SerializeField] private LineProviderBehaviour lineProvider;
    [Tooltip("The dialogue text to check the mouse against. Empty = the Line Presenter's Line Text.")]
    [SerializeField] private TMP_Text lineText;

    [Header("Look")]
    [SerializeField] private Color termColor = new Color32(0xDB, 0x45, 0x3F, 0xFF);
    [SerializeField] private bool underline = true;

    [Header("Terms")]
    [Tooltip("The asset listing every term (Create > Dialogue > Glossary).")]
    [SerializeField] private DialogueGlossary glossary;

    private HoverDelay hover;
    private HoverDelay Hover => hover ??= new HoverDelay(this);

    // The link under the mouse, null when none.
    private string hoveredID;
    // Only hide the tooltip if this opened it, so an item tooltip is left alone.
    private bool isShowingTooltip;

    private void Start()
    {
        if (lineProvider == null)
            lineProvider = (LineProviderBehaviour)FindAnyObjectByType<DialogueRunner>().LineProvider;

        lineProvider.RegisterMarkerProcessor("term", this);

        // A text dragged in from a prefab file (not the scene) is never shown, so the mouse never finds it.
        if (lineText != null && !lineText.gameObject.scene.IsValid())
        {
            Debug.LogWarning($"[DialogueTerms] Line Text '{lineText.name}' is in a prefab file, not the scene. " +
                             "Using the Line Presenter's Line Text instead.", this);
            lineText = null;
        }

        if (lineText == null)
        {
            LinePresenter presenter = FindAnyObjectByType<LinePresenter>();
            if (presenter != null)
                lineText = presenter.lineText;
        }
    }

    private void OnDisable()
    {
        hoveredID = null;
        Hover.Stop();
        HideTooltip();
    }

    /// <summary>Turns [term=id]word[/term] into a TextMeshPro link, so the word can be found under the mouse.</summary>
    public override List<LineParser.MarkupDiagnostic> ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes, string localeCode)
    {
        if (!marker.TryGetProperty("term", out string id) || string.IsNullOrEmpty(id))
            id = childBuilder.ToString();

        // Only tags go in front, no visible letters, so child markers don't need shifting.
        string color = ColorUtility.ToHtmlStringRGBA(termColor);
        childBuilder.Insert(0, $"<link=\"{id}\"><color=#{color}>" + (underline ? "<u>" : ""));
        childBuilder.Append((underline ? "</u>" : "") + "</color></link>");

        return NoDiagnostics;
    }

    private void Update()
    {
        string id = LinkUnderMouse();
        if (id == hoveredID) return;

        hoveredID = id;
        Hover.Stop();
        HideTooltip();

        DialogueTerm term = FindTerm(id);
        if (term == null) return;

        Hover.Start(() =>
        {
            if (InvDescription.Main == null) return;

            InvDescription.Main.Show(term.title, term.description);
            isShowingTooltip = true;
        });
    }

    /// <summary>The ID of the term link under the mouse, or null.</summary>
    private string LinkUnderMouse()
    {
        if (lineText == null || !lineText.isActiveAndEnabled) return null;
        // The Line Presenter fades the box out instead of switching it off.
        if (lineText.canvasRenderer.GetInheritedAlpha() < 0.01f) return null;

        Canvas canvas = lineText.canvas;
        Camera cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        int index = TMP_TextUtilities.FindIntersectingLink(lineText, Input.mousePosition, cam);
        if (index < 0) return null;

        TMP_LinkInfo link = lineText.textInfo.linkInfo[index];
        // Not typed out yet by the typewriter.
        if (link.linkTextfirstCharacterIndex >= lineText.maxVisibleCharacters) return null;

        return link.GetLinkID();
    }

    private DialogueTerm FindTerm(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        DialogueTerm term = glossary != null ? glossary.Find(id) : null;
        if (term == null)
            Debug.LogWarning($"[DialogueTerms] No term with ID '{id}'. Add it to the Glossary's Terms.", this);

        return term;
    }

    private void HideTooltip()
    {
        if (!isShowingTooltip) return;

        isShowingTooltip = false;
        if (InvDescription.Main != null)
            InvDescription.Main.Hide();
    }
}
