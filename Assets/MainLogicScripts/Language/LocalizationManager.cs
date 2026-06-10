using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class LocalizationManager : SingletonMonobehaviour<LocalizationManager>
{
    public string CurrentLanguage { get; private set; } = "en";
    public event Action<string> OnLanguageChanged;   // TMP labels subscribe here

    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private YarnProject yarnProject;   // for UI lookups

    public void SetLanguage(string code)
    {
        if (code == CurrentLanguage) return;
        CurrentLanguage = code;

        if (dialogueRunner != null &&
            dialogueRunner.LineProvider is BuiltinLocalisedLineProvider blp)
            blp.LocaleCode = code;

        OnLanguageChanged?.Invoke(code);     // labels re-pull via Get()
        PlayerPrefs.SetString("lang", code);
    }

    public string Get(string key)
    {
        if (yarnProject == null) return key;
        var loc = yarnProject.GetLocalization(CurrentLanguage);
        return loc != null && loc.ContainsLocalizedString(key)
            ? loc.GetLocalizedString(key)
            : key;   // fall back to the key so missing strings are visible
    }
}
