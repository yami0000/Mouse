using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DontDestroyOnLoad singleton. Remembers which quests/objectives
/// were accepted/completed, and which prefab spawn GUIDs have been
/// instantiated ¡ª including the prefab and spawnPointID needed to
/// re-instantiate them when a scene reloads.
/// </summary>
public class QuestWorldState : MonoBehaviour
{
    public static QuestWorldState Instance { get; private set; }

    private HashSet<string> acceptedQuests = new();
    private HashSet<string> completedObjectives = new();
    private HashSet<string> completedQuests = new();

    // Stores full spawn records so SceneController can re-instantiate on reload
    private Dictionary<string, SpawnRecord> spawnRecords = new();

    // Named delegates for clean unsubscription
    private System.Action<string> onQuestAccepted;
    private System.Action<string> onObjectiveCompleted;
    private System.Action<string> onQuestCompleted;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    // ©¤©¤ Quest / Objective State ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public bool IsQuestAccepted(string id) => acceptedQuests.Contains(id);
    public bool IsObjectiveCompleted(string key) => completedObjectives.Contains(key);
    public bool IsQuestCompleted(string id) => completedQuests.Contains(id);

    // ©¤©¤ Spawn Record Tracking ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Returns true if this spawnGUID has been registered (spawned at least once).
    /// </summary>
    public bool WasSpawned(string spawnGUID) => spawnRecords.ContainsKey(spawnGUID);

    /// <summary>
    /// Registers a spawn record. Called by QuestWorldEvent the first time
    /// a prefab is instantiated. Stores the prefab + spawnPointID so
    /// SceneController can re-instantiate it on scene reload.
    /// </summary>
    public void RegisterSpawned(string spawnGUID, GameObject prefab, string spawnPointID)
    {
        if (!spawnRecords.ContainsKey(spawnGUID))
            spawnRecords[spawnGUID] = new SpawnRecord(prefab, spawnPointID);
    }

    /// <summary>
    /// Returns all spawn records ¡ª used by SceneController to re-instantiate
    /// any persistent spawns that are no longer alive in the current scene.
    /// </summary>
    public IEnumerable<KeyValuePair<string, SpawnRecord>> GetAllSpawnRecords() => spawnRecords;
}

// ©¤©¤ Spawn Record ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

/// <summary>
/// Everything needed to re-instantiate a persistent quest spawn on scene reload.
/// </summary>
public class SpawnRecord
{
    public GameObject Prefab { get; }
    public string SpawnPointID { get; }

    public SpawnRecord(GameObject prefab, string spawnPointID)
    {
        Prefab = prefab;
        SpawnPointID = spawnPointID;
    }
}
