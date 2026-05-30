using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that persists across scenes and remembers which quests/objectives
/// have been accepted, completed, etc. QuestWorldEvent reads from this on
/// scene load to retroactively apply world consequences.
/// </summary>
public class QuestWorldState : MonoBehaviour
{
    public static QuestWorldState Instance { get; private set; }

    private HashSet<string> acceptedQuests = new();
    private HashSet<string> completedObjectives = new();
    private HashSet<string> completedQuests = new();

    // Tracks GUIDs of quest-spawned prefabs so they're never double-spawned
    private HashSet<string> spawnedPrefabGUIDs = new();

    // Named delegates so we can unsubscribe cleanly
    private System.Action<string> onQuestAccepted;
    private System.Action<string> onObjectiveCompleted;
    private System.Action<string> onQuestCompleted;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
         
    }

    void OnEnable()
    {
        onQuestAccepted = id => acceptedQuests.Add(id);
        onObjectiveCompleted = key => completedObjectives.Add(key);
        onQuestCompleted = id => completedQuests.Add(id);

        GameEvents.OnQuestAccepted += onQuestAccepted;
        GameEvents.OnObjectiveCompleted += onObjectiveCompleted;
        GameEvents.OnQuestCompleted += onQuestCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnQuestAccepted -= onQuestAccepted;
        GameEvents.OnObjectiveCompleted -= onObjectiveCompleted;
        GameEvents.OnQuestCompleted -= onQuestCompleted;
    }

    public bool IsQuestAccepted(string id) => acceptedQuests.Contains(id);
    public bool IsObjectiveCompleted(string key) => completedObjectives.Contains(key);
    public bool IsQuestCompleted(string id) => completedQuests.Contains(id);

    // ©¤©¤ Spawn GUID Tracking ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Returns true if this spawn GUID has already been instantiated.
    /// Prevents double-spawning when a scene reloads after the event fired.
    /// </summary>
    public bool WasSpawned(string spawnGUID) => spawnedPrefabGUIDs.Contains(spawnGUID);

    /// <summary>
    /// Records a spawn GUID as done. Called by QuestWorldEvent after spawning.
    /// </summary>
    public void RegisterSpawned(string spawnGUID) => spawnedPrefabGUIDs.Add(spawnGUID);
}
