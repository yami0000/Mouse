using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public GameObject Prefab;

    Dictionary<string, Quest> activeQuests = new();

    void Awake()
    {
        Instance = this;
    }

    public void AcceptQuest(QuestData data)
    {
        if (activeQuests.ContainsKey(data.questID))
            return;

        Quest quest = new Quest(data);

        activeQuests.Add(data.questID, quest);

        quest.OnQuestCompleted += HandleQuestCompleted;

        quest.Accept();

        QuestUIManager.Instance.AddQuest(quest);
    }

    void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest {quest.questID} completed!");

        quest.GiveReward();

        QuestUIManager.Instance.RemoveQuest(quest);

        activeQuests.Remove(quest.questID);

        quest.OnQuestCompleted -= HandleQuestCompleted;


    }
}