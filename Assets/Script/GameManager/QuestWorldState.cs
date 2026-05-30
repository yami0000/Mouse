using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWorldState : MonoBehaviour
{
    public static QuestWorldState Instance { get; private set; }

    // Stores which keys have been triggered
    private HashSet<string> completedObjectives = new();
    private HashSet<string> completedQuests = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // survives scene loads
    }

    void OnEnable()
    {
        GameEvents.OnObjectiveCompleted += key => completedObjectives.Add(key);
        GameEvents.OnQuestCompleted += id => completedQuests.Add(id);
    }

    void OnDisable()
    {
        GameEvents.OnObjectiveCompleted -= key => completedObjectives.Add(key);
        GameEvents.OnQuestCompleted -= id => completedQuests.Add(id);
    }

    public bool IsObjectiveCompleted(string key) => completedObjectives.Contains(key);
    public bool IsQuestCompleted(string id) => completedQuests.Contains(id);
}
