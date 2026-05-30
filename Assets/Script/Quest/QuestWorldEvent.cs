using System.Collections.Generic;
using UnityEngine;

// ©¤©¤ Serializable event entries ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

[System.Serializable]
public class QuestEventEntry
{
    [Tooltip("Must match QuestData.questID.")]
    public string questID;

    [Header("Spawn")]
    [Tooltip("Prefab to instantiate when this quest event fires.")]
    public GameObject spawnPrefab;

    [Tooltip("Drag a QuestSpawnPoint from this scene. Used as the spawn position " +
             "and as the fixed anchor for re-instantiation on scene reload.")]
    public QuestSpawnPoint spawnPoint;

    [Tooltip("Unique ID for this spawn. Must match the spawnGUID on the prefab's " +
             "PersistentSpawnObject component. E.g. 'spawn_quest001_npc'")]
    public string spawnGUID;

    [Header("Destroy")]
    [Tooltip("GUID of the PersistentWorldObject in this scene to permanently destroy.")]
    public string destroyGUID;
}

[System.Serializable]
public class ObjectiveEventEntry
{
    [Tooltip("Must match ObjectiveData.objectiveID.")]
    public string objectiveID;

    [Header("Spawn")]
    public GameObject spawnPrefab;
    public QuestSpawnPoint spawnPoint;

    [Tooltip("Unique ID for this spawn. Must match the spawnGUID on the prefab's " +
             "PersistentSpawnObject component.")]
    public string spawnGUID;

    [Header("Destroy")]
    [Tooltip("GUID of the PersistentWorldObject in this scene to permanently destroy.")]
    public string destroyGUID;
}

// ©¤©¤ QuestWorldEvent ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

/// <summary>
/// ONE per scene. Reacts to quest/objective events and applies world
/// consequences (spawn prefabs, permanently destroy objects).
///
/// On scene reload, SceneController.RespawnPersistentObjects() re-instantiates
/// any registered spawns that are no longer alive ¡ª using QuestSpawnPoint as
/// the fixed anchor.
/// </summary>
public class QuestWorldEvent : MonoBehaviour
{
    [Header("On Quest Accepted ¡ª one entry per quest")]
    [SerializeField] private List<QuestEventEntry> onQuestAccepted = new();

    [Header("On Objective Completed ¡ª one entry per objective")]
    [SerializeField] private List<ObjectiveEventEntry> onObjectiveCompleted = new();

    [Header("On Quest Completed ¡ª one entry per quest")]
    [SerializeField] private List<QuestEventEntry> onQuestCompleted = new();

    // ©¤©¤ Lifecycle ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    void OnEnable()
    {
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
        GameEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;

        // Retroactive apply ¡ª if events already fired before this scene loaded,
        // apply consequences now. Skip re-spawn here ¡ª SceneController handles
        // that separately so we don't double-instantiate.
        if (QuestWorldState.Instance == null) return;

        foreach (var entry in onQuestAccepted)
            if (QuestWorldState.Instance.IsQuestAccepted(entry.questID))
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);

        foreach (var entry in onObjectiveCompleted)
            if (QuestWorldState.Instance.IsObjectiveCompleted(entry.objectiveID))
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);

        foreach (var entry in onQuestCompleted)
            if (QuestWorldState.Instance.IsQuestCompleted(entry.questID))
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);
    }

    void OnDisable()
    {
        GameEvents.OnQuestAccepted -= HandleQuestAccepted;
        GameEvents.OnObjectiveCompleted -= HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }

    // ©¤©¤ Event Handlers ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    void HandleQuestAccepted(string id)
    {
        foreach (var entry in onQuestAccepted)
            if (entry.questID == id)
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);
    }

    void HandleObjectiveCompleted(string id)
    {
        foreach (var entry in onObjectiveCompleted)
            if (entry.objectiveID == id)
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);
    }

    void HandleQuestCompleted(string id)
    {
        foreach (var entry in onQuestCompleted)
            if (entry.questID == id)
                ApplyEntry(entry.spawnPrefab, entry.spawnPoint, entry.spawnGUID, entry.destroyGUID);
    }

    // ©¤©¤ Core Application ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    void ApplyEntry(GameObject prefab, QuestSpawnPoint spawnPoint, string spawnGUID, string destroyGUID)
    {
        Spawn(prefab, spawnPoint, spawnGUID);
        DestroyByGUID(destroyGUID);
    }

    void Spawn(GameObject prefab, QuestSpawnPoint spawnPoint, string spawnGUID)
    {
        if (prefab == null || spawnPoint == null) return;

        if (QuestWorldState.Instance == null)
        {
            Debug.LogError("[QuestWorldEvent] QuestWorldState not found ¡ª " +
                           "make sure it exists in your bootstrap scene.");
            return;
        }

        // Already registered ¡ª SceneController will respawn it on scene reload
        if (!string.IsNullOrEmpty(spawnGUID) && QuestWorldState.Instance.WasSpawned(spawnGUID))
        {
            Debug.Log($"[QuestWorldEvent] '{spawnGUID}' already registered ¡ª skipping.");
            return;
        }

        // Register BEFORE Instantiate so PersistentSpawnObject.Awake() sees it
        if (!string.IsNullOrEmpty(spawnGUID))
            QuestWorldState.Instance.RegisterSpawned(spawnGUID, prefab, spawnPoint.spawnPointID);

        Instantiate(prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        Debug.Log($"[QuestWorldEvent] Spawned '{prefab.name}' at '{spawnPoint.spawnPointID}' (GUID: {spawnGUID}).");
    }

    void DestroyByGUID(string destroyGUID)
    {
        if (string.IsNullOrEmpty(destroyGUID)) return;

        foreach (var pwo in FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None))
        {
            if (pwo.GUID == destroyGUID)
            {
                pwo.DestroyPersistently();
                Debug.Log($"[QuestWorldEvent] Permanently destroyed '{destroyGUID}'.");
                return;
            }
        }
    }
}
