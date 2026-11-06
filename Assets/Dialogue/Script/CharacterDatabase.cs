using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Every dialogue speaker's portrait in one asset, shared by all PortraitDialogueUIs.</summary>
[CreateAssetMenu(fileName = "Character Database", menuName = "Dialogue/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterProfile> characters = new List<CharacterProfile>();

    /// <summary>The character with this name (not case sensitive), or null.</summary>
    public CharacterProfile Find(string characterName)
    {
        if (string.IsNullOrEmpty(characterName)) return null;

        foreach (CharacterProfile character in characters)
            if (character != null && string.Equals(character.characterName, characterName, StringComparison.OrdinalIgnoreCase))
                return character;

        return null;
    }

#if UNITY_EDITOR
    // "Name: text" at the start of a node body line. Options (->), commands (<<), comments (//) and hashtags don't match.
    private static readonly System.Text.RegularExpressions.Regex SpeakerPattern =
        new System.Text.RegularExpressions.Regex(@"^\s*([^\s<>\-/#:\[][^:<>\[\]#]*?)\s*:");

    /// <summary>Lists the speakers in every .yarn file that have no entry here, e.g. a typo like "Regel".</summary>
    [ContextMenu("Check Yarn Speakers")]
    private void CheckYarnSpeakers()
    {
        var missing = new SortedDictionary<string, int>();

        foreach (string path in System.IO.Directory.GetFiles(Application.dataPath, "*.yarn", System.IO.SearchOption.AllDirectories))
        {
            // Node headers (title:, position:) sit between "===" and "---"; only lines inside a node body can have speakers.
            bool inBody = false;

            foreach (string line in System.IO.File.ReadAllLines(path))
            {
                string trimmed = line.Trim();
                if (trimmed == "---") { inBody = true; continue; }
                if (trimmed == "===") { inBody = false; continue; }
                if (!inBody) continue;

                var match = SpeakerPattern.Match(line);
                if (!match.Success) continue;

                string speaker = match.Groups[1].Value;
                if (Find(speaker) == null)
                    missing[speaker] = missing.TryGetValue(speaker, out int count) ? count + 1 : 1;
            }
        }

        if (missing.Count == 0)
        {
            Debug.Log("[CharacterDatabase] Every Yarn speaker has an entry.", this);
            return;
        }

        var report = new System.Text.StringBuilder("[CharacterDatabase] Speakers with no entry:");
        foreach (var pair in missing)
            report.Append($"\n  {pair.Key} ({pair.Value} lines)");

        Debug.LogWarning(report.ToString(), this);
    }
#endif
}
