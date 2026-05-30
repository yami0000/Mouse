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
        runner.AddCommandHandler<string>("progress_objective", ProgressObjective);
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
    void ProgressObjective(string dialogueKey)
    {
        QuestManager.Instance.ProgressTalkObjective(dialogueKey);
    }
}
