using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnQuestCommands : MonoBehaviour
{
    public DialogueRunner runner;

    void Awake()
    {
        runner.AddCommandHandler<string>("accept_quest", AcceptQuest);
    }

    void AcceptQuest(string questID)
    {
        QuestData quest = QuestDatabase.Instance.GetQuest(questID);

        if (quest == null)
        {
            Debug.LogError("Quest not found: " + questID);
            return;
        }

        QuestManager.Instance.AcceptQuest(quest);
    }
}
