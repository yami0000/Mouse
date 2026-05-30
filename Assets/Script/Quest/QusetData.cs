using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveType
{
    KillEnemy,
    CollectItem,
    TalkToNPC,
    GoSomeWhere,
}

// ©¤©¤ Quest Data (ScriptableObject) ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

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
    public RewardData[] rewards;

    [Header("Settings")]
    public bool isRepeatable = false;
    public List<QuestData> prerequisiteQuests;
}

// ©¤©¤ Objective Data ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

[System.Serializable]
public class ObjectiveData
{
    [Header("Identity")]
    [Tooltip("Unique ID for this objective. Used by QuestWorldEvent to react to it in any scene.")]
    public string objectiveID;

    [Header("Type & Target")]
    public ObjectiveType type;
    public ScriptableObject target;  // EnemyData / ItemData / NPCData

    [Min(1)]
    public int requiredAmount = 1;

    [Header("Dialogue (TalkToNPC only)")]
    [Tooltip("Must match the key used in <<progress_objective KEY>>")]
    public string dialogueKey;

    [Header("Quest Log")]
    [TextArea]
    [Tooltip("Quest log description shown after this objective completes")]
    public string updatedDescription;
}

// ©¤©¤ Reward Data ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

[System.Serializable]
public class RewardData
{
    public ItemData item;
    [Min(1)]
    public int amount;
}

[System.Serializable]
public class ItemReward { }
