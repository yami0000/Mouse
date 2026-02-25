using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveType
{
    KillEnemy,
    CollectItem,
    TalkToNPC
}


[CreateAssetMenu(menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questID;
    public string questName;
    [TextArea]
    public string description;

    [Header("Objectives")]
    public List<ObjectiveData> objectives = new();

    [Header("Rewards")]
    public RewardData[] rewards ;

    [Header("Settings")]
    public bool isRepeatable = false;

    [Tooltip("前置任务ID")]
    public List<QuestData> prerequisiteQuests;
}



[System.Serializable]
public class ObjectiveData
{
    public ObjectiveType type;

    [Header("Target")]
    public ScriptableObject target;  // 可以是 EnemyData / ItemData / NPCData

    [Min(1)]
    public int requiredAmount = 1;
}

[System.Serializable]
public class RewardData
{ 

    public ItemData item;
    [Min(1)]
    public int amount;

    
}

[System.Serializable]
public class ItemReward
{
   
}