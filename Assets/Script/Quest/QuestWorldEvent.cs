using System.Collections.Generic;
using UnityEngine;

// ---- Serializable event entries ----------------------------------------------

/// <summary>
/// One prefab spawn. A quest/objective entry can hold any number of these.
/// </summary>
[System.Serializable]
public class SpawnEntry
{
    [Tooltip("Prefab to instantiate when this quest event fires.")]
    public GameObject spawnPrefab;

    [Tooltip("Drag a QuestSpawnPoint from this scene. Used as the spawn position " +
             "and as the fixed anchor for re-instantiation on scene reload.")]
    public QuestSpawnPoint spawnPoint;

    [Tooltip("Unique ID for this spawn. Must match the spawnGUID on the prefab's " +
             "PersistentSpawnObject component. E.g. 'spawn_quest001_npc'")]
    public string spawnGUID;
}

[System.Serializable]
public class QuestEventEntry
{
    [Tooltip("Must match QuestData.questID.")]
    public string questID;

    [Header("Spawns - one element per prefab to instantiate")]
    public List<SpawnEntry> spawns = new();

    [Header("Destroys - GUIDs of PersistentWorldObjects to permanently destroy")]
    public List<string> destroyGUIDs = new();
}

[System.Serializable]
public class ObjectiveEventEntry
{
    [Tooltip("Must match ObjectiveData.objectiveID.")]
    public string objectiveID;

    [Header("Spawns - one element per prefab to instantiate")]
    public List<SpawnEntry> spawns = new();

    [Header("Destroys - GUIDs of PersistentWorldObjects to permanently destroy")]
    public List<string> destroyGUIDs = new();
}

// ---- QuestWorldEvent ---------------------------------------------------------

/// <summary>
/// ONE per scene. Reacts to quest/objective events and applies world
/// consequences (spawn prefabs, permanently destroy objects). Each entry can
/// now declare any number of spawns and any number of destroy targets.
///
/// On scene reload, GameManager.RespawnPersistentObjects() re-instantiates any
/// registered spawns that are no longer alive - using QuestSpawnPoint as the
/// fixed anchor.
/// </summary>
public class QuestWorldEvent : MonoBehaviour
{
    [Header("On Quest Accepted - one entry per quest")]
    [SerializeField] private List<QuestEventEntry> onQuestAccepted = new();

    [Header("On Objective Completed - one entry per objective")]
    [SerializeField] private List<ObjectiveEventEntry> onObjectiveCompleted = new();

    [Header("On Quest Completed - one entry per quest")]
    [SerializeField] private List<QuestEventEntry> onQuestCompleted = new();

    // ---- Lifecycle -----------------------------------------------------------

    void OnEnable()
    {
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
        GameEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;

        // Retroactive apply - if events already fired before this scene loaded,
        // apply consequences now. Skip re-spawn here - GameManager handles that
        // separately so we don't double-instantiate.
        if (QuestWorldState.Instance == null) return;

        foreach (var entry in onQuestAccepted)
            if (QuestWorldState.Instance.IsQuestAccepted(entry.questID))
                ApplyEntry(entry.spawns, entry.destroyGUIDs);

        foreach (var entry in onObjectiveCompleted)
            if (QuestWorldState.Instance.IsObjectiveCompleted(entry.objectiveID))
                ApplyEntry(entry.spawns, entry.destroyGUIDs);

        foreach (var entry in onQuestCompleted)
            if (QuestWorldState.Instance.IsQuestCompleted(entry.questID))
                ApplyEntry(entry.spawns, entry.destroyGUIDs);
    }

    void OnDisable()
    {
        GameEvents.OnQuestAccepted -= HandleQuestAccepted;
        GameEvents.OnObjectiveCompleted -= HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }

    // ---- Event Handlers ------------------------------------------------------

    void HandleQuestAccepted(string id)
    {
        foreach (var entry in onQuestAccepted)
            if (entry.questID == id)
                ApplyEntry(entry.spawns, entry.destroyGUIDs);
    }

    void HandleObjectiveCompleted(string id)
    {
        foreach (var entry in onObjectiveCompleted)
            if (entry.objectiveID == id)
                ApplyEntry(entry.spawns, entry.destroyGUIDs);
    }

    void HandleQuestCompleted(string id)
    {
        foreach (var entry in onQuestCompleted)
            if (entry.questID == id)
                ApplyEntry(entry.spawns, entry.destroyGUIDs);
    }

    // ---- Core Application ----------------------------------------------------

    void ApplyEntry(List<SpawnEntry> spawns, List<string> destroyGUIDs)
    {
        if (spawns != null)
            foreach (var s in spawns)
                Spawn(s.spawnPrefab, s.spawnPoint, s.spawnGUID);

        DestroyByGUIDs(destroyGUIDs);
    }

    void Spawn(GameObject prefab, QuestSpawnPoint spawnPoint, string spawnGUID)
    {
        if (prefab == null || spawnPoint == null) return;

        if (QuestWorldState.Instance == null)
        {
            Debug.LogError("[QuestWorldEvent] QuestWorldState not found - " +
                           "make sure it exists in your bootstrap scene.");
            return;
        }

        // Already registered - GameManager will respawn it on scene reload
        if (!string.IsNullOrEmpty(spawnGUID) && QuestWorldState.Instance.WasSpawned(spawnGUID))
        {
            Debug.Log($"[QuestWorldEvent] '{spawnGUID}' already registered - skipping.");
            return;
        }

        // Register BEFORE Instantiate so PersistentSpawnObject.Awake() sees it
        if (!string.IsNullOrEmpty(spawnGUID))
            QuestWorldState.Instance.RegisterSpawned(spawnGUID, prefab, spawnPoint.spawnPointID);

        Instantiate(prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        Debug.Log($"[QuestWorldEvent] Spawned '{prefab.name}' at '{spawnPoint.spawnPointID}' (GUID: {spawnGUID}).");
    }

    void DestroyByGUIDs(List<string> destroyGUIDs)
    {
        if (destroyGUIDs == null || destroyGUIDs.Count == 0) return;

        // Collect the non-empty targets, then do a single scene scan so multiple
        // destroys don't each trigger their own FindObjectsByType pass.
        var targets = new HashSet<string>();
        foreach (var guid in destroyGUIDs)
            if (!string.IsNullOrEmpty(guid))
                targets.Add(guid);

        if (targets.Count == 0) return;

        foreach (var pwo in FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None))
        {
            if (targets.Contains(pwo.GUID))
            {
                pwo.DestroyPersistently();
                Debug.Log($"[QuestWorldEvent] Permanently destroyed '{pwo.GUID}'.");
            }
        }
    }
}
