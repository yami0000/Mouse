using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed,
    Rewarded
}
public class Quest:MonoBehaviour
{
    protected List<QuestObjective> objectives = new();

    public QuestData data { get; private set; }
    public QuestState State { get; private set; }

    public event Action<Quest> OnQuestCompleted;

    public string questID => data.questID;
    public string description => data.description;

    public Quest(QuestData data)
    {
        this.data = data;
        CreateObjectives();
    }

    void CreateObjectives()
    {
        foreach (var objData in data.objectives)
        {
            QuestObjective obj = ObjectiveFactory.Create(objData, this);
            objectives.Add(obj);
        }
    }

    public void Accept()
    {
        if (State != QuestState.NotStarted) return;

        State = QuestState.InProgress;

        foreach (var objective in objectives)
        {
            objective.OnObjectiveProgressed += CheckCompletion;
            objective.Initialize();
        }

        OnQuestStart();
    }

    protected void CheckCompletion()
    {
        if (objectives.All(o => o.IsCompleted))
        {
            Complete();
        }
    }

    void Complete()
    {
        if (State != QuestState.InProgress) return;

        State = QuestState.Completed;

        foreach (var objective in objectives)
        {
            objective.OnObjectiveProgressed -= CheckCompletion;
            objective.Dispose();
        }

        OnQuestComplete();

        OnQuestCompleted?.Invoke(this);
    }

    public void GiveReward()
    {
        if (State != QuestState.Completed) return;

        State = QuestState.Rewarded;



        GenerateDrop();

        OnRewardGiven();
    }

    void OnQuestStart()
    {
        Debug.Log($"Quest {questID} started");
    }

    void OnQuestComplete()
    {
        Debug.Log($"Quest {questID} completed");
    }

    void OnRewardGiven()
    {
        Debug.Log($"Reward given for {questID}");
    }

    private void GenerateDrop()
    {
        foreach (var entry in data.rewards)
        {
            for (int i = 0; i < entry.amount; i++)
            {

                DropItem(entry.item);

            }
        }
    }

    public void DropItem(ItemData _itemData)
    {
        Vector3 Player = PlayerManager.Instance.player.transform.position;
        Player.x += PlayerManager.Instance.player.facingDir == 1 ? 2:-2;

       GameObject newDrop = Instantiate(QuestManager.Instance.Prefab,Player, Quaternion.identity);

        Vector2 randomVelocity = new Vector2(UnityEngine.Random.Range(5, 10), UnityEngine.Random.Range(10, 15));

        newDrop.GetComponent<ItemDropMovement>().SetUpItem(_itemData, randomVelocity);

    }
}


public abstract class QuestObjective
{
    public bool IsCompleted { get; protected set; }

    protected Quest parentQuest;
    protected ObjectiveData data;

    public event Action OnObjectiveProgressed;

    protected QuestObjective(ObjectiveData data, Quest quest)
    {
        this.data = data;
        this.parentQuest = quest;
    }

    protected void Progress()
    {
        OnObjectiveProgressed?.Invoke();
    }

    public abstract void Initialize();
    public abstract void Dispose();
}

public static class GameEvents
{
    public static Action<Enemy> OnEnemyKilled;
    public static Action<ItemData, int> OnItemCollected;
}
public class KillObjective : QuestObjective
{
    private int currentAmount;
    private Enemy targetEnemy;

    public KillObjective(ObjectiveData data, Quest quest)
        : base(data, quest)
    {
        

        if (targetEnemy == null)
        {
            Debug.LogError("KillObjective target is not EnemyData");
        }
    }

    public override void Initialize()
    {
        currentAmount = 0;
        GameEvents.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        if (IsCompleted) return;

        if (enemy != targetEnemy) return;   // 🔥 比较引用，不是字符串

        currentAmount++;

        Debug.Log($"Kill {targetEnemy}: {currentAmount}/{data.requiredAmount}");

        if (currentAmount >= data.requiredAmount)
        {
            IsCompleted = true;
        }

        Progress();
    }

    public override void Dispose()
    {
        GameEvents.OnEnemyKilled -= OnEnemyKilled;
    }
}
public class CollectObjective : QuestObjective
{
    private int currentAmount;
    private ItemData targetItem;

    public CollectObjective(ObjectiveData data, Quest quest)
        : base(data, quest)
    {
        targetItem = data.target as ItemData;

        if (targetItem == null)
        {
            Debug.LogError("CollectObjective target is not ItemData");
        }
    }

    public override void Initialize()
    {
        currentAmount = 0;
        GameEvents.OnItemCollected += OnItemCollected;
    }

    private void OnItemCollected(ItemData item, int amount)
    {
        if (IsCompleted) return;

        if (item != targetItem) return;   // 🔥 引用比较

        currentAmount += amount;

        Debug.Log($"Collect {targetItem.itemName}: {currentAmount}/{data.requiredAmount}");

        if (currentAmount >= data.requiredAmount)
        {
            IsCompleted = true;
        }

        Progress();
    }

    public override void Dispose()
    {
        GameEvents.OnItemCollected -= OnItemCollected;
    }
}


