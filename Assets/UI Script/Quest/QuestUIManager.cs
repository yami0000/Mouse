using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("Left")]
    public Transform contentParent;
    public GameObject questButtonPrefab;

    [Header("Right")]
    public TMP_Text questNameText;
    public TMP_Text descriptionText;
    public TMP_Text rewardText;

    private Dictionary<string, QuestButtonUI> questButtons = new();

    void Awake()
    {
        Instance = this;
    }

    public void AddQuest(Quest quest)
    {
        GameObject go = Instantiate(questButtonPrefab, contentParent);
        QuestButtonUI button = go.GetComponent<QuestButtonUI>();
        button.Setup(quest);

        questButtons.Add(quest.questID, button);
    }

    public void RemoveQuest(Quest quest)
    {
        if (questButtons.TryGetValue(quest.questID, out var button))
        {
            Destroy(button.gameObject);
            questButtons.Remove(quest.questID);
        }
    }

    public void ShowDetail(Quest quest)
    {
        questNameText.text = quest.data.questName;
        descriptionText.text = quest.data.description;

        rewardText.text = "";

 
    }
}