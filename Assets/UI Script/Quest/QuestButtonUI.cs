using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuestButtonUI : MonoBehaviour
{
    public TMP_Text questNameText;

    private Quest quest;

    public void Setup(Quest quest)
    {
        this.quest = quest;
        questNameText.text = quest.data.questName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        QuestUIManager.Instance.ShowDetail(quest);
    }
}
