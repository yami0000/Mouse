using System.Collections.Generic;
using UnityEngine;

public class QuestWorldState : MonoBehaviour
{
    public static QuestWorldState Instance { get; private set; }

    private HashSet<string> acceptedQuests = new();
    private HashSet<string> completedObjectives = new();
    private HashSet<string> completedQuests = new();

    // Full spawn records ¡ª prefab asset + spawnPointID ¡ª for scene-reload respawning
    private Dictionary<string, SpawnRecord> spawnRecords = new();

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

    public bool WasSpawned(string spawnGUID) => spawnRecords.ContainsKey(spawnGUID);

    /// <summary>
    /// Called by QuestWorldEvent on first spawn ¡ª stores the prefab asset
    /// and spawnPointID needed for scene-reload respawning.
    /// </summary>
    public void RegisterSpawned(string spawnGUID, GameObject prefab, string spawnPointID)
    {
        if (!spawnRecords.ContainsKey(spawnGUID))
            spawnRecords[spawnGUID] = new SpawnRecord(prefab, spawnPointID);
    }

    public IEnumerable<KeyValuePair<string, SpawnRecord>> GetAllSpawnRecords() => spawnRecords;
}

// ©¤©¤ Spawn Record ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

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
