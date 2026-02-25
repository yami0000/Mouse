using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
    public static QuestDatabase Instance { get; private set; }

    [SerializeField] private QuestData[] allQuests;

    private Dictionary<string, QuestData> questLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildDatabase();
    }

    private void BuildDatabase()
    {
        questLookup = new Dictionary<string, QuestData>();

        foreach (var quest in allQuests)
        {
            if (quest == null || string.IsNullOrEmpty(quest.questID))
            {
                Debug.LogWarning("Invalid quest entry in database.");
                continue;
            }

            if (questLookup.ContainsKey(quest.questID))
            {
                Debug.LogWarning("Duplicate quest ID: " + quest.questID);
                continue;
            }

            questLookup.Add(quest.questID, quest);
        }
    }

    public QuestData GetQuest(string questID)
    {
        if (questLookup.TryGetValue(questID, out QuestData quest))
        {
            return quest;
        }

        Debug.LogError("Quest not found: " + questID);
        return null;
    }
}
