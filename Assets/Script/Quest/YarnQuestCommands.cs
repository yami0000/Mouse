using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Yarn.Unity;

public class YarnQuestCommands : MonoBehaviour
{
    public DialogueRunner runner;

    void Awake()
    {
        runner.AddCommandHandler<string>("accept_quest", AcceptQuest);
        runner.AddCommandHandler<string>("progress_objective", ProgressObjective);
        runner.AddCommandHandler<string>("get_item", GetItem);
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

    void GetItem(string itemID)
    {
        ItemData item = ItemDatabase.Instance.GetItem(itemID);

        if (item == null)
        {
            Debug.LogError("Item not found: " + itemID);
            return;
        }

        Inventory.Instance.AddItem(item);
    }
}
