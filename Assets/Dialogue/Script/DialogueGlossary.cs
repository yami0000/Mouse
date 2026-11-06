using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>All the hoverable dialogue terms in one asset. DialogueTerms looks terms up here by ID.</summary>
[CreateAssetMenu(fileName = "Dialogue Glossary", menuName = "Dialogue/Glossary")]
public class DialogueGlossary : ScriptableObject
{
    public List<DialogueTerm> terms = new List<DialogueTerm>();

    /// <summary>The term with this ID (not case sensitive), or null.</summary>
    public DialogueTerm Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (DialogueTerm term in terms)
            if (term != null && string.Equals(term.termID, id, StringComparison.OrdinalIgnoreCase))
                return term;

        return null;
    }
}
