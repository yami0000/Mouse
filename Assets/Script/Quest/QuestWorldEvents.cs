using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place one of these in each scene that needs to react to a quest event.
/// It only activates when the active scene matches the sceneName field,
/// finds its spawn point by ID at runtime, and uses PersistentWorldObject
/// GUIDs for permanent destruction ¡ª no cross-scene direct references needed.
///
/// SETUP PER EVENT SECTION (Accept / Objective / Quest):
///   - sceneName        : Name of the scene this event belongs to
///   - questID          : Must match QuestData.questID
///   - objectiveID      : Must match ObjectiveData.objectiveID
///   - spawnPrefab      : Prefab to instantiate
///   - spawnPointID     : ID of the QuestSpawnPoint in this scene to spawn at
///   - spawnGUID        : Unique ID to prevent double-spawning across reloads
///   - destroyGUID      : GUID on the PersistentWorldObject to destroy forever
/// </summary>
public class QuestWorldEvents : MonoBehaviour
{
    [Header("Quest / Objective IDs to react to")]
    [Tooltip("Must match QuestData.questID. Leave empty to ignore quest-level events.")]
    [SerializeField] private string questID;

    [Tooltip("Must match ObjectiveData.objectiveID. Leave empty to ignore objective events.")]
    [SerializeField] private string objectiveID;

    // ©¤©¤ On Quest Accepted ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    [Header("On Quest Accepted")]
    [Tooltip("Only fires if current scene name matches this. Leave empty to fire in any scene.")]
    [SerializeField] private string acceptSceneName;
    [SerializeField] private GameObject acceptSpawnPrefab;
    [Tooltip("ID of the QuestSpawnPoint in the scene to spawn at.")]
    [SerializeField] private string acceptSpawnPointID;
    [Tooltip("Unique ID for this spawn ¡ª prevents double-spawning on scene reload.")]
    [SerializeField] private string acceptSpawnGUID;
    [Tooltip("GUID of the PersistentWorldObject to destroy permanently when quest is accepted.")]
    [SerializeField] private string acceptDestroyGUID;

    // ©¤©¤ On Objective Completed ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    [Header("On Objective Completed")]
    [SerializeField] private string objectiveSceneName;
    [SerializeField] private GameObject objectiveSpawnPrefab;
    [SerializeField] private string objectiveSpawnPointID;
    [SerializeField] private string objectiveSpawnGUID;
    [SerializeField] private string objectiveDestroyGUID;

    // ©¤©¤ On Quest Completed ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    [Header("On Quest Completed")]
    [SerializeField] private string questSceneName;
    [SerializeField] private GameObject questSpawnPrefab;
    [SerializeField] private string questSpawnPointID;
    [SerializeField] private string questSpawnGUID;
    [SerializeField] private string questDestroyGUID;

    // ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    void OnEnable()
    {
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
        GameEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;

        // Retroactive apply ¡ª fires when this scene loads after events already happened
        if (QuestWorldState.Instance == null) return;

        if (!string.IsNullOrEmpty(questID) && QuestWorldState.Instance.IsQuestAccepted(questID))
            ApplyConsequences(acceptSceneName, acceptSpawnPrefab, acceptSpawnPointID, acceptSpawnGUID, acceptDestroyGUID);

        if (!string.IsNullOrEmpty(objectiveID) && QuestWorldState.Instance.IsObjectiveCompleted(objectiveID))
            ApplyConsequences(objectiveSceneName, objectiveSpawnPrefab, objectiveSpawnPointID, objectiveSpawnGUID, objectiveDestroyGUID);

        if (!string.IsNullOrEmpty(questID) && QuestWorldState.Instance.IsQuestCompleted(questID))
            ApplyConsequences(questSceneName, questSpawnPrefab, questSpawnPointID, questSpawnGUID, questDestroyGUID);
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
        if (string.IsNullOrEmpty(questID) || id != questID) return;
        ApplyConsequences(acceptSceneName, acceptSpawnPrefab, acceptSpawnPointID, acceptSpawnGUID, acceptDestroyGUID);
    }

    void HandleObjectiveCompleted(string key)
    {
        if (string.IsNullOrEmpty(objectiveID) || key != objectiveID) return;
        ApplyConsequences(objectiveSceneName, objectiveSpawnPrefab, objectiveSpawnPointID, objectiveSpawnGUID, objectiveDestroyGUID);
    }

    void HandleQuestCompleted(string id)
    {
        if (string.IsNullOrEmpty(questID) || id != questID) return;
        ApplyConsequences(questSceneName, questSpawnPrefab, questSpawnPointID, questSpawnGUID, questDestroyGUID);
    }

    // ©¤©¤ Core Consequence Logic ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Applies a spawn + destroy consequence only if the current scene matches.
    /// Spawn is skipped if the spawnGUID was already registered (double-spawn guard).
    /// Destroy uses PersistentWorldObject so the target stays gone across sessions.
    /// </summary>
    void ApplyConsequences(string sceneName, GameObject prefab, string spawnPointID, string spawnGUID, string destroyGUID)
    {
        // Scene gate ¡ª if a scene name is specified, only fire in that scene
        if (!string.IsNullOrEmpty(sceneName) &&
            SceneManager.GetActiveScene().name != sceneName)
            return;

        // ©¤©¤ Spawn ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        if (prefab != null)
        {
            // Double-spawn guard
            bool alreadySpawned = !string.IsNullOrEmpty(spawnGUID) &&
                                  QuestWorldState.Instance.WasSpawned(spawnGUID);

            if (!alreadySpawned)
            {
                // Find the spawn point in this scene by ID
                QuestSpawnPoint spawnPoint = QuestSpawnPoint.Find(spawnPointID);

                if (spawnPoint != null)
                {
                    Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity);

                    if (!string.IsNullOrEmpty(spawnGUID))
                        QuestWorldState.Instance.RegisterSpawned(spawnGUID);

                    Debug.Log($"[QuestWorldEvent] Spawned '{prefab.name}' at '{spawnPointID}' (GUID: {spawnGUID}).");
                }
            }
            else
            {
                Debug.Log($"[QuestWorldEvent] Skipped spawn '{spawnGUID}' ¡ª already spawned.");
            }
        }

        // ©¤©¤ Destroy ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        if (!string.IsNullOrEmpty(destroyGUID))
        {
            // Find the PersistentWorldObject with this GUID in the current scene
            foreach (var pwo in FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None))
            {
                if (pwo.GUID == destroyGUID)
                {
                    pwo.DestroyPersistently();
                    Debug.Log($"[QuestWorldEvent] Permanently destroyed '{destroyGUID}'.");
                    break;
                }
            }
        }
    }
}
