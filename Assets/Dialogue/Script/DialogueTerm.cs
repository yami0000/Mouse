using UnityEngine;

/// <summary>
/// A word that can be hovered in dialogue, one entry of a DialogueGlossary. Yarn lines point at it
/// with [term=id]word[/term], and the inventory tooltip shows its title and description (see DialogueTerms).
/// </summary>
[System.Serializable]
public class DialogueTerm
{
    // First, so the Inspector names each list element after it.
    [Tooltip("What Yarn lines use: [term=id]word[/term]. Not case sensitive.")]
    public string termID;
    [Tooltip("The tooltip's first line, in the tooltip's Term Title Style.")]
    public string title;
    [Tooltip("The tooltip's text, in the tooltip's Term Body Style. TextMeshPro rich text works.")]
    [TextArea(3, 10)] public string description;
}
