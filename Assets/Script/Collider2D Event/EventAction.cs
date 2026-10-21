using UnityEngine;

/// <summary>
/// What happens when an event completes (CameraEvent) or an entity dies (Entity).
/// Flags, so any number can be ticked at once. Old values keep their bits, the new ones follow.
/// </summary>
[System.Flags]
public enum Consequences
{
    None               = 0,
    CameraFollowPlayer = 1 << 0,
    EnterBattleMode    = 1 << 1,
    InScriptDeath      = 1 << 2,
    DropReward         = 1 << 3,
    AcceptQuest        = 1 << 4,
    ProgressQuest      = 1 << 5,
    CompleteQuest      = 1 << 6,
    Move               = 1 << 7,
    Dialogue           = 1 << 8,
}

/// <summary>
/// One set of consequences plus the settings each of them reads. The inspector only shows the
/// settings of the consequences that are ticked (see EventActionDrawer).
/// </summary>
[System.Serializable]
public class EventAction
{
    public Consequences consequences;

    [Tooltip("Enemy put into battle mode. Empty = the entity that owns this (death consequences).")]
    public Enemy battleTarget;

    [Tooltip("Items that burst out. Same format as QuestData rewards.")]
    public RewardData[] rewards;
    [Tooltip("Where the rewards burst from. Empty = the event / entity itself.")]
    public Transform rewardOrigin;
    [Tooltip("Max sideways speed of each item, picked randomly left or right.")]
    public float burstSideways = 6f;
    [Tooltip("Upward speed of each item, picked randomly between X and Y.")]
    public Vector2 burstUp = new Vector2(10f, 15f);

    [Tooltip("Quest accepted.")]
    public QuestData questToAccept;
    [Tooltip("Progresses the TalkToNPC objective with this Dialogue Key, same as <<progress_objective KEY>>.")]
    public string objectiveKey;
    [Tooltip("Quest completed right away, rewards included. It has to be accepted already.")]
    public QuestData questToComplete;

    [Tooltip("Moves an object, e.g. pulls a door back up.")]
    public MoveAction move = new MoveAction();

    [Tooltip("Yarn node to play.")]
    public string dialogueName;

    [System.NonSerialized] private bool dialoguePlaying;

    public bool Has(Consequences _consequence) => (consequences & _consequence) != 0;

    /// <param name="E">The entity that owns this, used when Battle Target is empty.</param>
    /// <param name="origin">Fallback burst point for rewards.</param>
    public void Execute(Entity E = null, Transform origin = null)
    {
        if (Has(Consequences.CameraFollowPlayer))
            CameraSetter.Instance.DisablePosition();

        if (Has(Consequences.EnterBattleMode))
        {
            Enemy enemy = battleTarget != null ? battleTarget : E as Enemy;
            if (enemy != null)
                enemy.BattleState = true;
        }

        if (Has(Consequences.InScriptDeath))
        {
            GetPlayer().inScriptedDefeat = true;
            GetPlayer().OnScriptedDefeat += HandleDefeat;
        }

        if (Has(Consequences.DropReward))
            DropRewards(rewardOrigin != null ? rewardOrigin : origin);

        // Accept before progress and complete, so one event can accept a quest and push it on at once.
        if (Has(Consequences.AcceptQuest) && questToAccept != null)
            QuestManager.Instance.AcceptQuest(questToAccept);

        if (Has(Consequences.ProgressQuest) && !string.IsNullOrEmpty(objectiveKey))
            QuestManager.Instance.ProgressTalkObjective(objectiveKey);

        if (Has(Consequences.CompleteQuest) && questToComplete != null)
            QuestManager.Instance.CompleteQuest(questToComplete.questID);

        if (Has(Consequences.Move))
            move?.Begin();

        // An empty node would never report back and leave this busy for good.
        if (Has(Consequences.Dialogue) && !string.IsNullOrEmpty(dialogueName))
        {
            dialoguePlaying = true;
            NarrativeManager.Instance.RequestDialogue(dialogueName, 0f, () => dialoguePlaying = false);
        }
    }

    /// <summary>True while something started by Execute is still going (a Move or a Dialogue).</summary>
    public bool IsBusy => (Has(Consequences.Move) && move != null && move.IsMoving) ||
                          (Has(Consequences.Dialogue) && dialoguePlaying);

    private void DropRewards(Transform origin)
    {
        if (rewards == null) return;

        Vector3 spawnPos = origin != null ? origin.position : GetPlayer().transform.position;

        foreach (var entry in rewards)
        {
            if (entry == null || entry.item == null) continue;

            for (int i = 0; i < entry.amount; i++)
            {
                GameObject newDrop = Object.Instantiate(QuestManager.Instance.Prefab, spawnPos, Quaternion.identity);
                Vector2 randomVelocity = new Vector2(Random.Range(-burstSideways, burstSideways), Random.Range(burstUp.x, burstUp.y));
                newDrop.GetComponent<ItemDropMovement>().SetUpItem(entry.item, randomVelocity);
            }
        }
    }

    private void HandleDefeat()
    {
        GetPlayer().OnScriptedDefeat -= HandleDefeat;
        GetPlayer().inScriptedDefeat = false;           // leaving scripted mode
    }

    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }
}

/// <summary>
/// One entry in an ordered list of consequences (CameraEvent). Everything ticked in one entry
/// happens together; the entries happen one after another, Delay seconds apart.
/// </summary>
[System.Serializable]
public class ConsequenceStep : EventAction, ISequenceStep
{
    [Tooltip("Seconds to wait before this entry starts, counted from when the previous one finished " +
             "(or started, if that one runs with this one).")]
    [Min(0)] public float delay;

    [Tooltip("Start the next entry right away instead of waiting for this one to finish. " +
             "Only Move and Dialogue take time, everything else finishes at once.")]
    public bool runWithNext;

    public float Delay => delay;
    public bool RunWithNext => runWithNext;

    public ConsequenceStep() { }

    /// <summary>Copies an old single EventAction, so data from before the ordered list carries over.</summary>
    public ConsequenceStep(EventAction _source)
    {
        consequences = _source.consequences;
        battleTarget = _source.battleTarget;
        rewards = _source.rewards;
        rewardOrigin = _source.rewardOrigin;
        burstSideways = _source.burstSideways;
        burstUp = _source.burstUp;
        questToAccept = _source.questToAccept;
        objectiveKey = _source.objectiveKey;
        questToComplete = _source.questToComplete;
        move = _source.move ?? new MoveAction();
        dialogueName = _source.dialogueName;
    }
}
